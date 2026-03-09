using System.Diagnostics.CodeAnalysis;

namespace PayrollEngine.Api.Core;

/// <summary>
/// OAuth 2.0 / JWT Bearer authentication settings for the Payroll Engine API.
/// Used when <see cref="AuthenticationConfiguration.Mode"/> is <see cref="AuthenticationMode.OAuth"/>.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class OAuthConfiguration
{
    /// <summary>Token authority / issuer URL</summary>
    public string Authority { get; set; }

    /// <summary>Expected audience claim</summary>
    public string Audience { get; set; }

    /// <summary>Require HTTPS metadata (default: true)</summary>
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>OAuth client secret for Swagger UI (Option B: confidential client)</summary>
    public string ClientSecret { get; set; }
}