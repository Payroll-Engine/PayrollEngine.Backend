using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A regulation division
/// </summary>
public class RegulationShare : DomainObjectBase, IDomainAttributeObject, IEquatable<RegulationShare>
{
    /// <summary>
    /// The provider tenant id
    /// </summary>
    public int ProviderTenantId { get; set; }

    /// <summary>
    /// The provider regulation id
    /// </summary>
    public int ProviderRegulationId { get; set; }

    /// <summary>
    /// The consumer tenant id
    /// </summary>
    public int ConsumerTenantId { get; set; }

    /// <summary>
    /// The consumer division id
    /// </summary>
    public int? ConsumerDivisionId { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public RegulationShare()
    {
    }

    /// <inheritdoc/>
    public RegulationShare(RegulationShare copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(RegulationShare compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString()
    {
        return ConsumerDivisionId.HasValue ?
            $"Provider (Tenant={ProviderTenantId}, Regulation={ProviderRegulationId}) -> " +
                $"Consumer (Tenant={ConsumerTenantId}, Division={ConsumerDivisionId})" :
            $"Provider (Tenant={ProviderTenantId}, Regulation={ProviderRegulationId}) -> " +
                $"Consumer (Tenant={ConsumerTenantId})";
    }
}