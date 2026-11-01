using System;
using System.Security.Cryptography;
using System.Text;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// HMAC-SHA256 signatures for webhook messages.
/// <para>
/// Request headers: <c>X-PE-Timestamp</c> (unix seconds) and <c>X-PE-Signature: v1=&lt;hex&gt;</c>,
/// signed content <c>{timestamp}.{body}</c>.
/// </para>
/// <para>
/// Response header (function requests): <c>X-PE-Response-Signature: v1=&lt;hex&gt;</c>,
/// signed content <c>{timestamp}.{requestSignature}.{responseBody}</c>.
/// The binding to the request signature prevents the reuse of a response for another request.
/// </para>
/// </summary>
public static class WebhookSignature
{
    /// <summary>Request header with the unix timestamp in seconds</summary>
    public const string TimestampHeader = "X-PE-Timestamp";

    /// <summary>Request header with the request signature</summary>
    public const string SignatureHeader = "X-PE-Signature";

    /// <summary>Response header with the response signature</summary>
    public const string ResponseSignatureHeader = "X-PE-Response-Signature";

    private const string Version = "v1=";

    /// <summary>Sign a webhook request</summary>
    /// <param name="secret">The webhook secret</param>
    /// <param name="timestamp">The unix timestamp in seconds</param>
    /// <param name="body">The request body</param>
    /// <returns>The signature, like v1=3f2a...</returns>
    public static string SignRequest(string secret, long timestamp, string body) =>
        Compute(secret, $"{timestamp}.{body}");

    /// <summary>Sign a webhook response</summary>
    /// <param name="secret">The webhook secret</param>
    /// <param name="timestamp">The unix timestamp of the request in seconds</param>
    /// <param name="requestSignature">The request signature</param>
    /// <param name="body">The response body</param>
    /// <returns>The signature, like v1=3f2a...</returns>
    public static string SignResponse(string secret, long timestamp, string requestSignature, string body) =>
        Compute(secret, $"{timestamp}.{requestSignature}.{body}");

    /// <summary>Verify the signature of a webhook response</summary>
    /// <param name="secret">The webhook secret</param>
    /// <param name="timestamp">The unix timestamp of the request in seconds</param>
    /// <param name="requestSignature">The request signature</param>
    /// <param name="body">The response body</param>
    /// <param name="responseSignature">The signature received with the response</param>
    /// <returns>True for a valid signature</returns>
    public static bool VerifyResponse(string secret, long timestamp, string requestSignature,
        string body, string responseSignature)
    {
        if (string.IsNullOrWhiteSpace(responseSignature))
        {
            return false;
        }
        var expected = Encoding.ASCII.GetBytes(SignResponse(secret, timestamp, requestSignature, body ?? string.Empty));
        var actual = Encoding.ASCII.GetBytes(responseSignature.Trim());
        // constant time comparison
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    private static string Compute(string secret, string content)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(content));
        return Version + Convert.ToHexStringLower(hash);
    }
}
