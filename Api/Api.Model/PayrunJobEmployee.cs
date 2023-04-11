using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The payrun job employee API object
/// </summary>
public class PayrunJobEmployee : ApiObjectBase
{
    /// <summary>
    /// The employee id (immutable)
    /// </summary>
    [Required]
    public int EmployeeId { get; set; }
}