using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Application.Tests.Webhooks;

/// <summary>A request received by the fake webhook receiver</summary>
internal sealed record ReceivedRequest(string Address, string Body, string MediaType,
    IReadOnlyDictionary<string, string> Headers);

/// <summary>Fake http handler: records requests and returns configured responses</summary>
internal sealed class FakeHttpHandler : HttpMessageHandler
{
    private readonly Dictionary<string, Func<HttpResponseMessage>> responders = new();
    private readonly Dictionary<string, Func<ReceivedRequest, HttpResponseMessage>> dynamicResponders = new();

    /// <summary>All received requests in the order of arrival</summary>
    public List<ReceivedRequest> Requests { get; } = [];

    /// <summary>Respond to the address with a status code and a body</summary>
    public void Respond(string address, HttpStatusCode status, string body = null) =>
        responders[address] = () => new HttpResponseMessage(status)
        {
            Content = new StringContent(body ?? string.Empty)
        };

    /// <summary>Respond to the address with a status code, a body and the response headers (computed from the request)</summary>
    public void Respond(string address, HttpStatusCode status, string body,
        Func<ReceivedRequest, IReadOnlyDictionary<string, string>> headers) =>
        dynamicResponders[address] = request =>
        {
            var response = new HttpResponseMessage(status) { Content = new StringContent(body ?? string.Empty) };
            foreach (var header in headers(request))
            {
                response.Headers.Add(header.Key, header.Value);
            }
            return response;
        };

    /// <summary>Fail the request to the address with a connection error</summary>
    public void Fail(string address, string message = "connection refused") =>
        responders[address] = () => throw new HttpRequestException(message);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var address = request.RequestUri!.ToString();
        var body = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken) : null;
        var headers = new Dictionary<string, string>();
        foreach (var header in request.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }
        var received = new ReceivedRequest(address, body, request.Content?.Headers.ContentType?.MediaType, headers);
        Requests.Add(received);

        if (dynamicResponders.TryGetValue(address, out var dynamicResponder))
        {
            return dynamicResponder(received);
        }

        // default: 200 without body
        return responders.TryGetValue(address, out var responder)
            ? responder()
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(string.Empty) };
    }
}

/// <summary>Fake http client factory: always returns a client for the same fake handler</summary>
internal sealed class FakeHttpClientFactory(FakeHttpHandler handler) : IHttpClientFactory
{
    /// <summary>The requested client names</summary>
    public List<string> ClientNames { get; } = [];

    public HttpClient CreateClient(string name)
    {
        ClientNames.Add(name);
        return new HttpClient(handler, disposeHandler: false);
    }
}
