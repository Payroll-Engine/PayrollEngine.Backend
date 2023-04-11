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
    /// The employees language
    /// </summary>
    public Language Language { get; set; }

    /// <summary>
    /// Employee division names
    /// </summary>
    public List<string> Divisions { get; set; }
        
    /// <summary>
    /// The culture including the calendar, fallback is the division culture
    /// </summary>
    [StringLength(128)]
    public string Culture { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Identifier} {base.ToString()}";
}