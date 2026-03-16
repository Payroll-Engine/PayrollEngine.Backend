using System;

namespace PayrollEngine.Api.Model;

/// <summary>Backend server information for client diagnostics and compatibility checks</summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class BackendInformation
{
    /// <summary>Backend assembly version (e.g. "0.10.0-beta.1")</summary>
    public string Version { get; init; }

    /// <summary>Backend assembly build date (UTC)</summary>
    public DateTime BuildDate { get; init; }

    /// <summary>REST API version (e.g. "1.0")</summary>
    public string ApiVersion { get; init; }

    /// <summary>REST API name</summary>
    public string ApiName { get; init; }

    /// <summary>Authentication configuration (no secrets)</summary>
    public BackendAuthInformation Authentication { get; init; }

    /// <summary>Database runtime information</summary>
    public BackendDatabaseInformation Database { get; init; }

    /// <summary>Runtime configuration</summary>
    public BackendRuntimeInformation Runtime { get; init; }
}
