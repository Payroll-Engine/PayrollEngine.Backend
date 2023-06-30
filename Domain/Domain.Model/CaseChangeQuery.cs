
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace PayrollEngine.Domain.Model;

/// <summary>
/// Case change query parameters
/// </summary>
public class CaseChangeQuery : Query
{
    /// <summary>
    /// The division id
    /// </summary>
    public int? DivisionId { get; set; }

    /// <summary>
    /// The culture
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// Exclude global changes
    /// </summary>
    public bool ExcludeGlobal { get; set; }
}