// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Cluster set name references for a payroll.
/// Each property holds the name of a <see cref="ClusterSet"/> entry defined in
/// <see cref="Payroll.ClusterSets"/>. An empty or null value means no cluster filtering
/// is applied for that context. Persisted as a single JSON column on the Payroll table.
/// </summary>
public class PayrollClusterSets
{
    /// <summary>The case cluster set name (undefined: all).</summary>
    public string ClusterSetCase { get; set; }

    /// <summary>The case field cluster set name (undefined: all).</summary>
    public string ClusterSetCaseField { get; set; }

    /// <summary>The collector cluster set name (undefined: all).</summary>
    public string ClusterSetCollector { get; set; }

    /// <summary>The collector cluster set name for retro payrun jobs (undefined: all).</summary>
    public string ClusterSetCollectorRetro { get; set; }

    /// <summary>The wage type cluster set name (undefined: all).</summary>
    public string ClusterSetWageType { get; set; }

    /// <summary>The wage type cluster set name for retro payrun jobs (undefined: all).</summary>
    public string ClusterSetWageTypeRetro { get; set; }

    /// <summary>The case value cluster set name (undefined: none, *: all).</summary>
    public string ClusterSetCaseValue { get; set; }

    /// <summary>The wage type period result cluster set name (undefined: none).</summary>
    public string ClusterSetWageTypePeriod { get; set; }

    /// <summary>
    /// The wage type YTD cache cluster set name (undefined: no YTD pre-loading).
    /// WageTypes matching this cluster set are bulk-loaded once per employee at
    /// PayrunEmployeeStart, eliminating repeated DB round-trips for YTD queries.
    /// </summary>
    public string ClusterSetWageTypeYtd { get; set; }

    /// <summary>
    /// The wage type consolidated cache cluster set name (undefined: no Cons pre-loading).
    /// WageTypes matching this cluster set are bulk-loaded once per employee at
    /// PayrunEmployeeStart via a single <c>GetConsolidatedWageTypeResults</c> call,
    /// eliminating repeated DB round-trips for consolidated (retro-merged) YTD queries.
    /// </summary>
    public string ClusterSetWageTypeCons { get; set; }
}
