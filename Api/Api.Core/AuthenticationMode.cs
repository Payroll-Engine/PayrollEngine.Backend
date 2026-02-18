namespace PayrollEngine.Api.Core;

/// <summary>Authentication mode for the Payroll API</summary>
public enum AuthenticationMode
{
    /// <summary>No authentication — all requests are allowed (development/internal only)</summary>
    None,

    /// <summary>Static API key passed via request header</summary>
    ApiKey,

    /// <summary>OAuth 2.0 / JWT Bearer token</summary>
    OAuth
}