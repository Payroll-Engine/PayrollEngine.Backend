namespace PayrollEngine.Api.Model;

/// <summary>Database runtime information</summary>
public class BackendDatabaseInformation
{
    /// <summary>Database type (e.g. "SqlServer")</summary>
    public string Type { get; init; }

    /// <summary>Database catalog name</summary>
    public string Name { get; init; }

    /// <summary>Database server version (e.g. "16.0.4175.1")</summary>
    public string Version { get; init; }
}
