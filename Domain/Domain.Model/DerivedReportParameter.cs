// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Domain.Model;

/// <summary>Derived report parameter</summary>
public sealed class DerivedReportParameter : ReportParameter
{
    /// <summary>The regulation id</summary>
    public int RegulationId { get; set; }

    /// <summary>The layer level</summary>
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}