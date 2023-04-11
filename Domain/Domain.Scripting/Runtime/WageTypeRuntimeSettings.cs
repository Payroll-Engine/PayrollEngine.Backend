using System.Collections.Generic;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class WageTypeRuntimeSettings : PayrunRuntimeSettings
{
    /// <summary>The execution count</summary>
    public int ExecutionCount { get; set; }

    /// <summary>The wage type</summary>
    public WageType WageType { get; set; }

    /// <summary>The wage type attributes</summary>
    public Dictionary<string, object> WageTypeAttributes { get; set; }

    /// <summary>The disabled collectors</summary>
    public List<string> DisabledCollectors { get; set; }

    /// <summary>The current wage type and collector results</summary>
    public PayrollResultSet CurrentPayrollResult { get; set; }

    /// <summary>The current wage type result</summary>
    public WageTypeResultSet CurrentWageTypeResult { get; set; }
}