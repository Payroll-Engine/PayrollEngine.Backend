// ReSharper disable UnusedAutoPropertyAccessor.Global
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The employee API object
/// </summary>
public class Employee : ApiObjectBase
{
    /// <summary>
    /// The employee identifier
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Identifier { get; set; }

    /// <summary>
    /// The first name of the employee
    /// </summary>
    [Required]
    [StringLength(128)]
    public string FirstName { get; set; }

    /// <summary>
    /// The last name of the employee
    /// </summary>
    [Required]
    [StringLength(128)]
    public string LastName { get; set; }

    /// <summary>
    /// Employee division names
    /// </summary>
    public List<string> Divisions { get; set; }
        
    /// <summary>
    /// The employee culture name based on RFC 4646 (fallback: division culture)
    /// </summary>
    [StringLength(128)]
    public string Culture { get; set; }
    
    /// <summary>
    /// The employee calendar (fallback: division calendar)
    /// </summary>
    public string Calendar { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Identifier} {base.ToString()}";
}