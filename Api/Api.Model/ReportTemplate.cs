﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The report template API object
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class ReportTemplate : ApiObjectBase
{
    /// <summary>
    /// The payroll report template name
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// The report culture
    /// </summary>
    [Required]
    public string Culture { get; set; }

    /// <summary>
    /// The report content (client owned)
    /// </summary>
    [Required]
    public string Content { get; set; }

    /// <summary>
    /// The report content type
    /// </summary>
    [StringLength(128)]
    public string ContentType { get; set; }
        
    /// <summary>
    /// The report schema (client owned)
    /// </summary>
    public string Schema { get; set; }

    /// <summary>
    /// The report external resource
    /// </summary>
    [StringLength(256)]
    public string Resource { get; set; }
    
    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Culture} {base.ToString()}";
}