﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// Case field value
/// </summary>
public class CaseFieldValue
{
    /// <summary>
    /// The case field name
    /// </summary>
    [Required]
    public string CaseFieldName { get; set; }

    /// <summary>
    /// The localized case field names
    /// </summary>
    public Dictionary<string, string> CaseFieldNameLocalizations { get; set; }

    /// <summary>
    /// The created date
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// The period start date
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// The period end date
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// The case value type
    /// </summary>
    public ValueType ValueType { get; set; }

    /// <summary>
    /// The case period value as JSON
    /// </summary>
    [Required]
    public string Value { get; set; }

    /// <summary>
    /// Cancellation date
    /// </summary>
    public DateTime? CancellationDate { get; set; }

    /// <summary>
    /// The case value tags
    /// </summary>
    public List<string> Tags { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }
}