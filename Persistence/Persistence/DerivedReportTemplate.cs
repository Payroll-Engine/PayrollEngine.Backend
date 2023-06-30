using PayrollEngine.Domain.Model;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Persistence;

/// <summary>Derived report template</summary>
public sealed class DerivedReportTemplate : ReportTemplate
{
    /// <summary>The layer level</summary>
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}