﻿using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll service tenant
/// </summary>
// ReSharper disable PropertyCanBeMadeInitOnly.Global
public class Tenant : DomainObjectBase, IIdentifiableObject, IDomainAttributeObject, IEquatable<Tenant>
{
    /// <summary>
    /// The unique identifier of the tenant (immutable)
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    /// The tenant culture name based on RFC 4646 (fallback: system culture)
    /// </summary>
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
    public Tenant()
    {
    }

    /// <inheritdoc/>
    public Tenant(Tenant copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(Tenant compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Identifier} {base.ToString()}";
}