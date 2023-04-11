using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class CollectorRuntimeSettings : PayrunRuntimeSettings
{
    /// <summary>The collector</summary>
    public Collector Collector { get; set; }

    /// <summary>The current wage type and collector results</summary>
    public PayrollResultSet CurrentPayrollResult { get; set; }

    /// <summary>Result attributes</summary>
    public CollectorResultSet CurrentCollectorResult { get; set; }
}