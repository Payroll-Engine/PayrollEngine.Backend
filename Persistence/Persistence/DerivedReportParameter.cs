using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

/// <summary>Derived report parameter</summary>
public sealed class DerivedReportParameter : ReportParameter
{
    /// <summary>The layer level</summary>
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}