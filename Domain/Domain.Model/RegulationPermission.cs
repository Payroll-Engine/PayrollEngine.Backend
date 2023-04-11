using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A regulation division
/// </summary>
public class RegulationPermission : DomainObjectBase, IDomainAttributeObject, IEquatable<RegulationPermission>
{
    /// <summary>
    /// The tenant id
    /// </summary>
    public int TenantId { get; set; }

    /// <summary>
    /// The regulation id
    /// </summary>
    public int RegulationId { get; set; }

    /// <summary>
    /// The permission tenant id
    /// </summary>
    public int PermissionTenantId { get; set; }

    /// <summary>
    /// The permission division id
    /// </summary>
    public int? PermissionDivisionId { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public RegulationPermission()
    {
    }

    /// <inheritdoc/>
    public RegulationPermission(RegulationPermission copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(RegulationPermission compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{RegulationId} (Tenant {PermissionTenantId}, Division {PermissionDivisionId}) {base.ToString()}";
}