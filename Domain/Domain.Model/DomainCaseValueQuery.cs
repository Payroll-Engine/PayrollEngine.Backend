
namespace PayrollEngine.Domain.Model;

/// <summary>
/// Query case value
/// </summary>
public class DomainCaseValueQuery
{
    /// <summary>
    /// The parent id
    /// </summary>
    public int ParentId { get; set; }

    /// <summary>
    /// The division scope
    /// </summary>
    public DivisionScope DivisionScope { get; set; }

    /// <summary>
    /// The division id
    /// </summary>
    public int? DivisionId { get; set; }

    /// <summary>
    /// The forecast name
    /// </summary>
    public string Forecast { get; set; }
}