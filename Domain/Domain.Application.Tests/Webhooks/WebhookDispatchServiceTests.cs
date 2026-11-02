using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using PayrollEngine.Domain.Model;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application.Tests.Webhooks;

public class WebhookDispatchServiceTests
{
    private const string AddressA = "http://receiver.test/a";
    private const string AddressB = "http://receiver.test/b";

    // -------------------------------------------------------------------------
    // Test setup
    // -------------------------------------------------------------------------

    internal sealed class Fixture
    {
        public Tenant Tenant { get; } = new() { Id = 1, Identifier = "Tenant1" };
        public User User { get; } = new() { Id = 7, Identifier = "user@example.com" };

        public FakeTenantRepository Tenants { get; } = new();
        public FakeUserRepository Users { get; } = new();
        public FakeWebhookRepository Webhooks { get; } = new();
        public FakeWebhookMessageRepository Messages { get; } = new();
        public FakeHttpHandler Http { get; } = new();
        public FakeHttpClientFactory HttpFactory { get; }
        public FakeSecretProtector Protector { get; } = new();
        public WebhookDispatchService Service { get; }

        public Fixture(WebhookTracking tracking = WebhookTracking.None)
        {
            Tenants.Items.Add(Tenant);
            Users.Items.Add(User);
            HttpFactory = new FakeHttpClientFactory(Http);
            Service = new WebhookDispatchService(Tenants, Users, Webhooks, Messages, HttpFactory, Protector, tracking);
        }

        public Webhook AddWebhook(WebhookAction action, string address,
            ObjectStatus status = ObjectStatus.Active, string secret = null)
        {
            var webhook = new Webhook
            {
                Id = Webhooks.Items.Count + 100,
                Name = $"Webhook{Webhooks.Items.Count + 1}",
                Action = action,
                ReceiverAddress = address,
                Status = status,
                // stored in the protected form
                Secret = secret != null ? Protector.Protect(secret) : null
            };
            Webhooks.Items.Add(webhook);
            return webhook;
        }
    }

    internal static WebhookDispatchMessage Message(WebhookAction action, string operation = null,
        string request = null) =>
        new()
        {
            Action = action,
            RequestOperation = operation,
            RequestMessage = request
        };

    // the database context is only passed through to the repositories: the fakes ignore it
    internal static readonly IDbContext Context = null;

    // -------------------------------------------------------------------------
    // HasWebhooksAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HasWebhooks_WithoutWebhooks_ReturnsFalse()
    {
        var fixture = new Fixture();
        Assert.False(await fixture.Service.HasWebhooksAsync(Context, fixture.Tenant.Id));
    }

    [Fact]
    public async Task HasWebhooks_WithActiveWebhook_ReturnsTrue()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, AddressA);
        Assert.True(await fixture.Service.HasWebhooksAsync(Context, fixture.Tenant.Id));
    }

    [Fact]
    public async Task HasWebhooks_WithInactiveWebhookOnly_ReturnsFalse()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, AddressA, ObjectStatus.Inactive);
        Assert.False(await fixture.Service.HasWebhooksAsync(Context, fixture.Tenant.Id));
    }

    // -------------------------------------------------------------------------
    // InvokeAsync (scripting, synchronous with response)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Invoke_WithoutWebhook_ReturnsNull()
    {
        var fixture = new Fixture();

        var result = await fixture.Service.InvokeAsync(Context, fixture.Tenant, fixture.User,
            Message(WebhookAction.CaseFunctionRequest, "MinWage"));

        Assert.Null(result);
        Assert.Empty(fixture.Http.Requests);
    }

    [Fact]
    public async Task Invoke_WebhookForOtherAction_ReturnsNull()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, AddressA);

        var result = await fixture.Service.InvokeAsync(Context, fixture.Tenant, fixture.User,
            Message(WebhookAction.CaseFunctionRequest, "MinWage"));

        Assert.Null(result);
        Assert.Empty(fixture.Http.Requests);
    }

    [Fact]
    public async Task Invoke_InactiveWebhook_IsIgnored()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, AddressA, ObjectStatus.Inactive);

        var result = await fixture.Service.InvokeAsync(Context, fixture.Tenant, fixture.User,
            Message(WebhookAction.CaseFunctionRequest, "MinWage"));

        Assert.Null(result);
        Assert.Empty(fixture.Http.Requests);
    }

    [Fact]
    public async Task Invoke_WithTwoWebhooksForTheAction_ReturnsNullWithoutSending()
    {
        // a function request needs exactly one receiver
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, AddressA);
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, AddressB);

        var result = await fixture.Service.InvokeAsync(Context, fixture.Tenant, fixture.User,
            Message(WebhookAction.CaseFunctionRequest, "MinWage"));

        Assert.Null(result);
        Assert.Empty(fixture.Http.Requests);
    }

    [Fact]
    public async Task Invoke_WithSingleWebhook_ReturnsTheResponseBody()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, AddressA);
        fixture.Http.Respond(AddressA, HttpStatusCode.Created, "2000");

        var result = await fixture.Service.InvokeAsync(Context, fixture.Tenant, fixture.User,
            Message(WebhookAction.CaseFunctionRequest, "MinWage", "{\"year\":2026}"));

        Assert.Equal("2000", result);
        Assert.Single(fixture.Http.Requests);
    }

    [Fact]
    public async Task Invoke_PostsTheRuntimeMessageAsJson()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, AddressA);

        await fixture.Service.InvokeAsync(Context, fixture.Tenant, fixture.User,
            Message(WebhookAction.CaseFunctionRequest, "MinWage", "{\"year\":2026}"));

        var request = Assert.Single(fixture.Http.Requests);
        Assert.Equal(AddressA, request.Address);
        Assert.Equal("application/json", request.MediaType);

        // payload: camel case, tenant and user identifiers, no response data
        using var json = JsonDocument.Parse(request.Body);
        var root = json.RootElement;
        Assert.Equal("Tenant1", root.GetProperty("tenant").GetString());
        Assert.Equal("user@example.com", root.GetProperty("user").GetString());
        Assert.Equal("CaseFunctionRequest", root.GetProperty("actionName").GetString());
        Assert.Equal("MinWage", root.GetProperty("requestOperation").GetString());
        Assert.Equal("{\"year\":2026}", root.GetProperty("requestMessage").GetString());
        Assert.Equal(AddressA, root.GetProperty("receiverAddress").GetString());
        Assert.False(root.TryGetProperty("responseStatus", out _));
    }

    [Fact]
    public async Task Invoke_UsesTheNamedWebhookHttpClient()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, AddressA);

        await fixture.Service.InvokeAsync(Context, fixture.Tenant, fixture.User,
            Message(WebhookAction.CaseFunctionRequest, "MinWage"));

        Assert.Equal(WebhookDispatchService.HttpClientName, Assert.Single(fixture.HttpFactory.ClientNames));
    }

    [Fact]
    public async Task Invoke_ReceiverReturnsError_Throws()
    {
        // a failed status must abort, not yield a bogus value
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, AddressA);
        fixture.Http.Respond(AddressA, HttpStatusCode.InternalServerError, "failed");

        var exception = await Assert.ThrowsAsync<PayrollException>(() => fixture.Service.InvokeAsync(Context,
            fixture.Tenant, fixture.User, Message(WebhookAction.CaseFunctionRequest, "MinWage")));

        Assert.Contains("500", exception.Message);
    }

    [Fact]
    public async Task Invoke_ReceiverNotReachable_Throws()
    {
        // the scripting function uses the response: a connection error must abort, not yield a bogus value
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, AddressA);
        fixture.Http.Fail(AddressA);

        var exception = await Assert.ThrowsAsync<PayrollException>(() => fixture.Service.InvokeAsync(Context,
            fixture.Tenant, fixture.User, Message(WebhookAction.CaseFunctionRequest, "MinWage")));

        Assert.Contains("connection refused", exception.Message);
    }

    [Fact]
    public async Task Invoke_RepeatedCalls_QueryTheWebhooksOnce()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, AddressA);

        for (var i = 0; i < 3; i++)
        {
            await fixture.Service.InvokeAsync(Context, fixture.Tenant, fixture.User,
                Message(WebhookAction.CaseFunctionRequest, "MinWage"));
        }

        Assert.Equal(1, fixture.Webhooks.QueryCount);
    }

    [Fact]
    public async Task Invoke_DoesNotLoadTenantOrUser()
    {
        // tenant and user objects are passed in: no reload from the repositories
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, AddressA);

        await fixture.Service.InvokeAsync(Context, fixture.Tenant, fixture.User,
            Message(WebhookAction.CaseFunctionRequest, "MinWage"));

        Assert.Equal(0, fixture.Users.GetCount);
    }

    [Fact]
    public async Task Invoke_WithoutTracking_DoesNotWriteMessages()
    {
        // notification tracking does not include the scripting calls
        var fixture = new Fixture(WebhookTracking.Notification);
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, AddressA);

        await fixture.Service.InvokeAsync(Context, fixture.Tenant, fixture.User,
            Message(WebhookAction.CaseFunctionRequest, "MinWage"));

        Assert.Empty(fixture.Messages.Created);
        Assert.Empty(fixture.Messages.Updated);
    }

    [Fact]
    public async Task Invoke_WithTracking_CreatesAndThenUpdatesTheMessageWithTheResponse()
    {
        var fixture = new Fixture(WebhookTracking.All);
        var webhook = fixture.AddWebhook(WebhookAction.CaseFunctionRequest, AddressA);
        fixture.Http.Respond(AddressA, HttpStatusCode.OK, "2000");

        await fixture.Service.InvokeAsync(Context, fixture.Tenant, fixture.User,
            Message(WebhookAction.CaseFunctionRequest, "MinWage"));

        // created before the call: no response yet
        var created = Assert.Single(fixture.Messages.Created);
        Assert.Equal(webhook.Id, created.WebhookId);
        Assert.Equal("CaseFunctionRequest", created.ActionName);
        Assert.Equal(0, created.ResponseStatus);
        Assert.Equal(default, created.ResponseDate);

        // updated after the call: response data available
        var updated = Assert.Single(fixture.Messages.Updated);
        Assert.Equal(webhook.Id, updated.WebhookId);
        Assert.Equal(200, updated.ResponseStatus);
        Assert.Equal("2000", updated.ResponseMessage);
        Assert.NotEqual(default, updated.ResponseDate);
    }

    [Fact]
    public async Task Invoke_WithNullArguments_Throws()
    {
        var fixture = new Fixture();
        var message = Message(WebhookAction.CaseFunctionRequest, "MinWage");

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            fixture.Service.InvokeAsync(Context, null, fixture.User, message));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            fixture.Service.InvokeAsync(Context, fixture.Tenant, null, message));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            fixture.Service.InvokeAsync(Context, fixture.Tenant, fixture.User, null));
    }

    // -------------------------------------------------------------------------
    // SendMessageAsync (notification, no response)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Send_WithoutWebhook_SendsNothingAndDoesNotLoadTheUser()
    {
        var fixture = new Fixture();

        await fixture.Service.SendMessageAsync(Context, fixture.Tenant.Id, fixture.User.Id,
            Message(WebhookAction.CaseChangeAdded, request: "{}"));

        Assert.Empty(fixture.Http.Requests);
        Assert.Equal(0, fixture.Users.GetCount);
    }

    [Fact]
    public async Task Send_WithSingleWebhook_PostsTheMessageWithTenantAndUser()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, AddressA);

        await fixture.Service.SendMessageAsync(Context, fixture.Tenant.Id, fixture.User.Id,
            Message(WebhookAction.CaseChangeAdded, request: "{\"id\":1}"));

        var request = Assert.Single(fixture.Http.Requests);
        using var json = JsonDocument.Parse(request.Body);
        Assert.Equal("Tenant1", json.RootElement.GetProperty("tenant").GetString());
        Assert.Equal("user@example.com", json.RootElement.GetProperty("user").GetString());
        Assert.Equal("CaseChangeAdded", json.RootElement.GetProperty("actionName").GetString());
        Assert.Equal("{\"id\":1}", json.RootElement.GetProperty("requestMessage").GetString());
    }

    [Fact]
    public async Task Send_WithTwoWebhooks_SendsToAll()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, AddressA);
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, AddressB);

        await fixture.Service.SendMessageAsync(Context, fixture.Tenant.Id, fixture.User.Id,
            Message(WebhookAction.CaseChangeAdded, request: "{}"));

        Assert.Equal([AddressA, AddressB], fixture.Http.Requests.Select(x => x.Address).Order());
    }

    [Fact]
    public async Task Send_OnlyMatchingActionIsSent()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, AddressA);
        fixture.AddWebhook(WebhookAction.TaskChange, AddressB);

        await fixture.Service.SendMessageAsync(Context, fixture.Tenant.Id, fixture.User.Id,
            Message(WebhookAction.TaskChange, request: "{}"));

        Assert.Equal(AddressB, Assert.Single(fixture.Http.Requests).Address);
    }

    [Fact]
    public async Task Send_FailingReceiver_DoesNotStopTheOtherReceivers()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, AddressA);
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, AddressB);
        fixture.Http.Fail(AddressA);

        await fixture.Service.SendMessageAsync(Context, fixture.Tenant.Id, fixture.User.Id,
            Message(WebhookAction.CaseChangeAdded, request: "{}"));

        Assert.Equal(2, fixture.Http.Requests.Count);
        Assert.Contains(fixture.Http.Requests, x => x.Address == AddressB);
    }

    [Fact]
    public async Task Send_LoadsTheUserOncePerCall()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, AddressA);
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, AddressB);

        await fixture.Service.SendMessageAsync(Context, fixture.Tenant.Id, fixture.User.Id,
            Message(WebhookAction.CaseChangeAdded, request: "{}"));

        Assert.Equal(1, fixture.Users.GetCount);
    }

    [Fact]
    public async Task Send_WithTracking_WritesAMessagePerWebhook()
    {
        var fixture = new Fixture(WebhookTracking.Notification);
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, AddressA);
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, AddressB);

        await fixture.Service.SendMessageAsync(Context, fixture.Tenant.Id, fixture.User.Id,
            Message(WebhookAction.CaseChangeAdded, request: "{}"));

        Assert.Equal(2, fixture.Messages.Created.Count);
        Assert.Equal(2, fixture.Messages.Updated.Count);
        Assert.All(fixture.Messages.Updated, x => Assert.Equal(200, x.ResponseStatus));
    }

    [Fact]
    public async Task Send_TrackingNone_DoesNotWriteMessages()
    {
        var fixture = new Fixture(WebhookTracking.None);
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, AddressA);

        await fixture.Service.SendMessageAsync(Context, fixture.Tenant.Id, fixture.User.Id,
            Message(WebhookAction.CaseChangeAdded, request: "{}"));

        Assert.Empty(fixture.Messages.Created);
        Assert.Empty(fixture.Messages.Updated);
    }

    [Fact]
    public async Task Send_UnknownTenant_Throws()
    {
        var fixture = new Fixture();
        await Assert.ThrowsAsync<PayrollException>(() =>
            fixture.Service.SendMessageAsync(Context, 99, fixture.User.Id,
                Message(WebhookAction.CaseChangeAdded)));
    }

    [Fact]
    public async Task Send_UnknownUser_ThrowsWhenAWebhookMatches()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, AddressA);

        await Assert.ThrowsAsync<PayrollException>(() =>
            fixture.Service.SendMessageAsync(Context, fixture.Tenant.Id, 99,
                Message(WebhookAction.CaseChangeAdded)));
        Assert.Empty(fixture.Http.Requests);
    }

    [Fact]
    public async Task Send_InvalidIds_Throw()
    {
        var fixture = new Fixture();
        var message = Message(WebhookAction.CaseChangeAdded);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            fixture.Service.SendMessageAsync(Context, 0, fixture.User.Id, message));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            fixture.Service.SendMessageAsync(Context, fixture.Tenant.Id, 0, message));
    }

    [Fact]
    public async Task Send_WithoutMessage_Throws()
    {
        var fixture = new Fixture();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            fixture.Service.SendMessageAsync(Context, fixture.Tenant.Id, fixture.User.Id, null));
    }
}
