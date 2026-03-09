namespace PayrollEngine.Domain.Application;

/// <summary>
/// Processing mode for the <see cref="PayrunProcessor"/>
/// </summary>
public enum PayrunProcessorMode
{
    /// <summary>Standard mode: results are persisted to the database</summary>
    Persist,

    /// <summary>Preview mode: results are collected in-memory, no DB writes for results or job</summary>
    Preview
}
