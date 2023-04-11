
namespace PayrollEngine.Api.Model;

/// <summary>
/// Case field reference
/// </summary>
public class CaseFieldReference
{
    /// <summary>
    /// The case field name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The case field order
    /// </summary>
    public int? Order { get; set; }
}