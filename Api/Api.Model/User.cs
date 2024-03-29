﻿// ReSharper disable UnusedAutoPropertyAccessor.Global
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The payroll user API object
/// </summary>
public class User : ApiObjectBase
{
    /// <summary>
    /// The user unique identifier
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Identifier { get; set; }

    /// <summary>
    /// The user password
    /// </summary>
    [StringLength(128)]
    public string Password { get; set; }

    /// <summary>
    /// The first name of the user
    /// </summary>
    [Required]
    [StringLength(128)]
    public string FirstName { get; set; }

    /// <summary>
    /// The last name of the user
    /// </summary>
    [Required]
    [StringLength(128)]
    public string LastName { get; set; }

    /// <summary>
    /// The user culture name based on RFC 4646
    /// </summary>
    [StringLength(128)]
    public string Culture { get; set; }

    /// <summary>
    /// The user type
    /// </summary>
    public UserType UserType { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Identifier} {base.ToString()}";
}