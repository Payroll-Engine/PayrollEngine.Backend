using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// Lookup including the lookup value
/// </summary>
public class LookupSet : Lookup
{
    /// <summary>
    /// The lookup values
    /// </summary>
    [Required]
    public LookupValue[] Values { get; set; }
}