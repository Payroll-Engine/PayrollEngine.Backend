﻿// ReSharper disable UnusedAutoPropertyAccessor.Global
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The tenant API object
/// </summary>
public class Tenant : ApiObjectBase
{
    /// <summary>
    /// The unique identifier of the tenant (immutable)
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Identifier { get; set; }

    /// <summary>
    /// The tenant culture name based on RFC 4646 (fallback: system culture)
    /// </summary>
    [StringLength(128)]
    public string Culture { get; set; }

    /// <summary>
    /// The tenant calendar (fallback: default calendar)
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