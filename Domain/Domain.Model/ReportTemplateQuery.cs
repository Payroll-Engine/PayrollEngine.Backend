// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Report templates query
/// </summary>
public class ReportTemplateQuery : Query
{
    /// <summary>
    /// Report culture
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string Culture { get; set; }

    /// <summary>
    /// Exclude report content
    /// </summary>
    public bool ExcludeContent { get; set; }
}