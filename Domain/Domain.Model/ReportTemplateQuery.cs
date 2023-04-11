
namespace PayrollEngine.Domain.Model;

/// <summary>
/// Report templates query
/// </summary>
public class ReportTemplateQuery : Query
{
    /// <summary>
    /// Report language
    /// </summary>
    public Language? Language { get; set; }

    /// <summary>
    /// Exclude report content
    /// </summary>
    public bool ExcludeContent { get; set; }
}