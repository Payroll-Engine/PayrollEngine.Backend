using System;
using Microsoft.AspNetCore.DataProtection;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Protects webhook secrets with ASP.NET Core data protection.
/// The protected value is prefixed with <c>enc1:</c> to allow future format changes.
/// The data protection key ring must be persisted and shared by all backend instances,
/// see <see cref="PayrollServerConfiguration.DataProtectionKeyPath"/>.
/// </summary>
public class WebhookSecretProtector(IDataProtectionProvider dataProtectionProvider) : IWebhookSecretProtector
{
    private const string Prefix = "enc1:";
    private const string Purpose = "PayrollEngine.Webhook.Secret.v1";

    private IDataProtector Protector { get; } =
        (dataProtectionProvider ?? throw new ArgumentNullException(nameof(dataProtectionProvider)))
        .CreateProtector(Purpose);

    /// <inheritdoc />
    public string Protect(string secret)
    {
        if (string.IsNullOrEmpty(secret))
        {
            throw new ArgumentException("Missing secret", nameof(secret));
        }
        return Prefix + Protector.Protect(secret);
    }

    /// <inheritdoc />
    public string Unprotect(string protectedSecret)
    {
        if (string.IsNullOrEmpty(protectedSecret))
        {
            throw new ArgumentException("Missing protected secret", nameof(protectedSecret));
        }
        if (!protectedSecret.StartsWith(Prefix, StringComparison.Ordinal))
        {
            throw new PayrollException("Unsupported webhook secret format.");
        }
        return Protector.Unprotect(protectedSecret[Prefix.Length..]);
    }
}
