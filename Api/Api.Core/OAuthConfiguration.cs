using System.Diagnostics.CodeAnalysis;

namespace PayrollEngine.Api.Core;

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