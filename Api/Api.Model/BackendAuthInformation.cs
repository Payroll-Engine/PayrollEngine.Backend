namespace PayrollEngine.Api.Model;

/// <summary>Backend authentication information (no secrets exposed)</summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class BackendAuthInformation
{
    /// <summary>Authentication mode: None, ApiKey or OAuth</summary>
    public string Mode { get; init; }

    /// <summary>OAuth token authority URL — only set when Mode is OAuth</summary>
    public string OAuthAuthority { get; init; }

    /// <summary>OAuth expected audience — only set when Mode is OAuth</summary>
    public string OAuthAudience { get; init; }
}
