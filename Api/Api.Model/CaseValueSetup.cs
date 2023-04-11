
namespace PayrollEngine.Api.Model;

/// <summary>
/// The payroll case value setup API object (immutable)
/// </summary>
public class CaseValueSetup : CaseValue
{
    /// <summary>
    /// Case documents
    /// </summary>
    public CaseDocument[] Documents { get; set; }
}