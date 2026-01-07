using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class CollectorRuntimeSettings : PayrunRuntimeSettings
{
    /// <summary>The collector</summary>
    public DerivedCollector Collector { get; init; }

    /// <summary>The current wage type and collector results</summary>
    public PayrollResultSet CurrentPayrollResult { get; init; }

    /// <summary>Result attributes</summary>
    public CollectorResultSet CurrentCollectorResult { get; init; }
}