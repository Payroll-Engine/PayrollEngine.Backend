using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using PayrollEngine.Domain.Model;
using Xunit;
using Task = System.Threading.Tasks.Task;
using Fixture = PayrollEngine.Domain.Application.Tests.Webhooks.WebhookDispatchServiceTests.Fixture;

namespace PayrollEngine.Domain.Application.Tests.Webhooks;

public class WebhookSignatureTests
{
    private const string Address = "http://receiver.test/a";
    private const string Secret = "s3cret-value";

    // -------------------------------------------------------------------------
    // Signature algorithm (independent computation with the documented format)
    // -------------------------------------------------------------------------

    private static string Hmac(string secret, string content) =>
        "v1=" + System.Convert.ToHexString(
            HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(content))).ToLowerInvariant();

    [Fact]
    public void SignRequest_SignsTimestampAndBody()
    {
        Assert.Equal(Hmac(Secret, "1700000000.{\"a\":1}"),
            WebhookSignature.SignRequest(Secret, 1700000000, "{\"a\":1}"));
    }

    [Fact]
    public void SignRequest_DependsOnSecretTimestampAndBody()
    {
        var signature = WebhookSignature.SignRequest(Secret, 1, "body");
        Assert.NotEqual(signature, WebhookSignature.SignRequest("other", 1, "body"));
        Assert.NotEqual(signature, WebhookSignature.SignRequest(Secret, 2, "body"));
        Assert.NotEqual(signature, WebhookSignature.SignRequest(Secret, 1, "body2"));
    }

    [Fact]
    public void SignResponse_IsBoundToTheRequestSignature()
    {
        var request = WebhookSignature.SignRequest(Secret, 1, "request");
        Assert.Equal(Hmac(Secret, $"1.{request}.2000"), WebhookSignature.SignResponse(Secret, 1, request, "2000"));
        Assert.NotEqual(WebhookSignature.SignResponse(Secret, 1, request, "2000"),
            WebhookSignature.SignResponse(Secret, 1, "v1=other", "2000"));
    }

    [Fact]
    public void VerifyResponse_AcceptsOnlyTheValidSignature()
    {
        var request = WebhookSignature.SignRequest(Secret, 1, "request");
        var response = WebhookSignature.SignResponse(Secret, 1, request, "2000");

        Assert.True(WebhookSignature.VerifyResponse(Secret, 1, request, "2000", response));
        Assert.False(WebhookSignature.VerifyResponse(Secret, 1, request, "2001", response));
        Assert.False(WebhookSignature.VerifyResponse("other", 1, request, "2000", response));
        Assert.False(WebhookSignature.VerifyResponse(Secret, 1, request, "2000", null));
        Assert.False(WebhookSignature.VerifyResponse(Secret, 1, request, "2000", ""));
        Assert.False(WebhookSignature.VerifyResponse(Secret, 1, request, "2000", "v1=00"));
    }

    // -------------------------------------------------------------------------
    // Request signing
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Invoke_WithoutSecret_SendsNoSignatureHeaders()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, Address);

        await fixture.Service.InvokeAsync(WebhookDispatchServiceTests.Context, fixture.Tenant, fixture.User,
            WebhookDispatchServiceTests.Message(WebhookAction.CaseFunctionRequest, "MinWage"));

        var request = Assert.Single(fixture.Http.Requests);
        Assert.DoesNotContain(WebhookSignature.TimestampHeader, request.Headers.Keys);
        Assert.DoesNotContain(WebhookSignature.SignatureHeader, request.Headers.Keys);
    }

    [Fact]
    public async Task Invoke_WithSecret_SendsAValidSignatureOverTimestampAndBody()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, Address, secret: Secret);
        fixture.Http.Respond(Address, HttpStatusCode.OK, "2000", SignedResponse("2000"));

        await fixture.Service.InvokeAsync(WebhookDispatchServiceTests.Context, fixture.Tenant, fixture.User,
            WebhookDispatchServiceTests.Message(WebhookAction.CaseFunctionRequest, "MinWage"));

        var request = Assert.Single(fixture.Http.Requests);
        var timestamp = long.Parse(request.Headers[WebhookSignature.TimestampHeader], CultureInfo.InvariantCulture);
        Assert.Equal(WebhookSignature.SignRequest(Secret, timestamp, request.Body),
            request.Headers[WebhookSignature.SignatureHeader]);

        // timestamp is current
        var now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Assert.InRange(timestamp, now - 30, now + 30);
    }

    [Fact]
    public async Task Send_WithSecret_SignsTheNotification()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, Address, secret: Secret);

        await fixture.Service.SendMessageAsync(WebhookDispatchServiceTests.Context, fixture.Tenant.Id, fixture.User.Id,
            WebhookDispatchServiceTests.Message(WebhookAction.CaseChangeAdded, request: "{}"));

        var request = Assert.Single(fixture.Http.Requests);
        var timestamp = long.Parse(request.Headers[WebhookSignature.TimestampHeader], CultureInfo.InvariantCulture);
        Assert.Equal(WebhookSignature.SignRequest(Secret, timestamp, request.Body),
            request.Headers[WebhookSignature.SignatureHeader]);
    }

    [Fact]
    public async Task Send_WithSecret_DoesNotRequireAResponseSignature()
    {
        // notifications: the response is only logged
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, Address, secret: Secret);
        fixture.Http.Respond(Address, HttpStatusCode.OK, "ok");

        await fixture.Service.SendMessageAsync(WebhookDispatchServiceTests.Context, fixture.Tenant.Id, fixture.User.Id,
            WebhookDispatchServiceTests.Message(WebhookAction.CaseChangeAdded, request: "{}"));

        Assert.Single(fixture.Http.Requests);
    }

    // -------------------------------------------------------------------------
    // Response verification (function requests)
    // -------------------------------------------------------------------------

    private static System.Func<ReceivedRequest, IReadOnlyDictionary<string, string>> SignedResponse(
        string body, string secret = Secret) =>
        request =>
        {
            var timestamp = long.Parse(request.Headers[WebhookSignature.TimestampHeader], CultureInfo.InvariantCulture);
            var requestSignature = request.Headers[WebhookSignature.SignatureHeader];
            return new Dictionary<string, string>
            {
                [WebhookSignature.ResponseSignatureHeader] =
                    WebhookSignature.SignResponse(secret, timestamp, requestSignature, body)
            };
        };

    [Fact]
    public async Task Invoke_ValidResponseSignature_ReturnsTheResponse()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, Address, secret: Secret);
        fixture.Http.Respond(Address, HttpStatusCode.OK, "2000", SignedResponse("2000"));

        var result = await fixture.Service.InvokeAsync(WebhookDispatchServiceTests.Context, fixture.Tenant,
            fixture.User, WebhookDispatchServiceTests.Message(WebhookAction.CaseFunctionRequest, "MinWage"));

        Assert.Equal("2000", result);
    }

    [Fact]
    public async Task Invoke_MissingResponseSignature_Throws()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, Address, secret: Secret);
        fixture.Http.Respond(Address, HttpStatusCode.OK, "2000");

        await Assert.ThrowsAsync<PayrollException>(() => fixture.Service.InvokeAsync(
            WebhookDispatchServiceTests.Context, fixture.Tenant, fixture.User,
            WebhookDispatchServiceTests.Message(WebhookAction.CaseFunctionRequest, "MinWage")));
    }

    [Fact]
    public async Task Invoke_ResponseSignedWithAnotherSecret_Throws()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, Address, secret: Secret);
        fixture.Http.Respond(Address, HttpStatusCode.OK, "2000", SignedResponse("2000", "other-secret"));

        await Assert.ThrowsAsync<PayrollException>(() => fixture.Service.InvokeAsync(
            WebhookDispatchServiceTests.Context, fixture.Tenant, fixture.User,
            WebhookDispatchServiceTests.Message(WebhookAction.CaseFunctionRequest, "MinWage")));
    }

    [Fact]
    public async Task Invoke_ResponseBodyChangedAfterSigning_Throws()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, Address, secret: Secret);
        // signature of 2000, body 9000
        fixture.Http.Respond(Address, HttpStatusCode.OK, "9000", SignedResponse("2000"));

        await Assert.ThrowsAsync<PayrollException>(() => fixture.Service.InvokeAsync(
            WebhookDispatchServiceTests.Context, fixture.Tenant, fixture.User,
            WebhookDispatchServiceTests.Message(WebhookAction.CaseFunctionRequest, "MinWage")));
    }

    [Fact]
    public async Task Invoke_WithoutSecret_DoesNotRequireAResponseSignature()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, Address);
        fixture.Http.Respond(Address, HttpStatusCode.OK, "2000");

        var result = await fixture.Service.InvokeAsync(WebhookDispatchServiceTests.Context, fixture.Tenant,
            fixture.User, WebhookDispatchServiceTests.Message(WebhookAction.CaseFunctionRequest, "MinWage"));

        Assert.Equal("2000", result);
    }

    [Fact]
    public async Task Invoke_ReceiverError_ThrowsWithoutSignatureCheck()
    {
        // an error response carries no value: it fails as an error status, not as a signature problem
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, Address, secret: Secret);
        fixture.Http.Respond(Address, HttpStatusCode.InternalServerError, "failed");

        var exception = await Assert.ThrowsAsync<PayrollException>(() => fixture.Service.InvokeAsync(
            WebhookDispatchServiceTests.Context, fixture.Tenant, fixture.User,
            WebhookDispatchServiceTests.Message(WebhookAction.CaseFunctionRequest, "MinWage")));

        Assert.Contains("500", exception.Message);
    }

    // -------------------------------------------------------------------------
    // Unreadable secret
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Invoke_UnreadableSecret_ThrowsAndSendsNothing()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseFunctionRequest, Address, secret: Secret);
        fixture.Protector.FailUnprotect = true;

        await Assert.ThrowsAsync<PayrollException>(() => fixture.Service.InvokeAsync(
            WebhookDispatchServiceTests.Context, fixture.Tenant, fixture.User,
            WebhookDispatchServiceTests.Message(WebhookAction.CaseFunctionRequest, "MinWage")));
        Assert.Empty(fixture.Http.Requests);
    }

    [Fact]
    public async Task Send_UnreadableSecret_ThrowsAndSendsNothing()
    {
        var fixture = new Fixture();
        fixture.AddWebhook(WebhookAction.CaseChangeAdded, Address, secret: Secret);
        fixture.Protector.FailUnprotect = true;

        await Assert.ThrowsAsync<PayrollException>(() => fixture.Service.SendMessageAsync(
            WebhookDispatchServiceTests.Context, fixture.Tenant.Id, fixture.User.Id,
            WebhookDispatchServiceTests.Message(WebhookAction.CaseChangeAdded, request: "{}")));
        Assert.Empty(fixture.Http.Requests);
    }

    // -------------------------------------------------------------------------
    // Webhook service: secret protection before storage
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Service_Create_ProtectsThePlainSecret()
    {
        var protector = new FakeSecretProtector();
        var repository = new FakeWebhookRepository();
        var service = new WebhookService(repository, protector);

        var created = await service.CreateAsync(null, 1, new Webhook { Name = "a", Secret = Secret });

        Assert.Equal(protector.Protect(Secret), created.Secret);
        Assert.True(created.Signed);
    }

    [Fact]
    public async Task Service_Update_NullKeepsEmptyRemovesValueReplaces()
    {
        var protector = new FakeSecretProtector();
        var repository = new FakeWebhookRepository();
        var service = new WebhookService(repository, protector);

        // null: keep (the repository ignores a null secret)
        var keep = await service.UpdateAsync(null, 1, new Webhook { Id = 1, Secret = null });
        Assert.Null(keep.Secret);

        // empty: remove (passed unchanged to the repository)
        var remove = await service.UpdateAsync(null, 1, new Webhook { Id = 1, Secret = "" });
        Assert.Equal("", remove.Secret);
        Assert.False(remove.Signed);

        // value: replace with the protected secret
        var replace = await service.UpdateAsync(null, 1, new Webhook { Id = 1, Secret = "new" });
        Assert.Equal(protector.Protect("new"), replace.Secret);
    }
}
