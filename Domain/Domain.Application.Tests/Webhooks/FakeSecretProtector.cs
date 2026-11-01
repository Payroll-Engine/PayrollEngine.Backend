using System;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Application.Tests.Webhooks;

/// <summary>Fake secret protector: reversible marker instead of encryption</summary>
internal sealed class FakeSecretProtector : IWebhookSecretProtector
{
    private const string Prefix = "protected:";

    /// <summary>When set, <see cref="Unprotect"/> fails (lost key ring)</summary>
    public bool FailUnprotect { get; set; }

    public string Protect(string secret) => Prefix + secret;

    public string Unprotect(string protectedSecret)
    {
        if (FailUnprotect)
        {
            throw new InvalidOperationException("key not found in the key ring");
        }
        if (!protectedSecret.StartsWith(Prefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("not a protected secret");
        }
        return protectedSecret[Prefix.Length..];
    }
}
