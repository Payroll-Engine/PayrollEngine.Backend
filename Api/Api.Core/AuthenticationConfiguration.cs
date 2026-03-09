using System.Diagnostics.CodeAnalysis;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Authentication configuration for the Payroll Engine API.
/// Supports <see cref="AuthenticationMode.None"/>, <see cref="AuthenticationMode.ApiKey"/>,
/// and <see cref="AuthenticationMode.OAuth"/> modes.
/// </summary>
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class AuthenticationConfiguration
{
    /// <summary>Authentication mode (default: None)</summary>
    public AuthenticationMode Mode { get; set; } = AuthenticationMode.None;

    /// <summary>Api key — only used when Mode is ApiKey</summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string ApiKey { get; set; }

    /// <summary>OAuth 2.0 / JWT Bearer settings — only used when Mode is OAuth</summary>
    public OAuthConfiguration OAuth { get; set; } = new();
}