using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// Case slot
/// </summary>
public class CaseSlot
{
    /// <summary>
    /// The case slot name
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// The localized case slot names
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public Dictionary<string, string> NameLocalizations { get; set; }
}