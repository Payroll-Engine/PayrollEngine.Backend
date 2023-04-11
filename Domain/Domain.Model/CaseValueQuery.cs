
namespace PayrollEngine.Domain.Model;

/// <summary>
/// Query by object name parameters
/// </summary>
public class CaseValueQuery : Query
{
    /// <summary>
    /// The division scope
    /// </summary>
    public DivisionScope DivisionScope { get; set; }

    /// <summary>
    /// The division id
    /// </summary>
    public int? DivisionId { get; set; }

    /// <summary>
    /// The case name
    /// </summary>
    public string CaseName { get; set; }

    /// <summary>
    /// The case field name
    /// </summary>
    public string CaseFieldName { get; set; }
}