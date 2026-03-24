using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>A regulation share — grants a consumer tenant access to a shared regulation</summary>
public class RegulationShare : DomainObjectBase, IDomainAttributeObject, IEquatable<RegulationShare>
{
    /// <summary>The provider tenant id</summary>
    public int ProviderTenantId { get; set; }

    /// <summary>The provider regulation id</summary>
    public int ProviderRegulationId { get; set; }

    /// <summary>The consumer tenant id</summary>
    public int ConsumerTenantId { get; set; }

    /// <summary>The consumer division id</summary>
    public int? ConsumerDivisionId { get; set; }

    /// <summary>The isolation level granted to the consumer tenant.
    /// Consolidation: cross-tenant result access for report scripts — regulation not added as payroll layer.
    /// Write: regulation available as full payroll layer (default).</summary>
    public TenantIsolationLevel IsolationLevel { get; set; } = TenantIsolationLevel.Write;

    /// <summary>Custom attributes</summary>
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
    public bool Equals(RegulationShare compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString()
    {
        var scope = IsolationLevel == TenantIsolationLevel.Consolidation ? " [Consolidation]" : string.Empty;
        return ConsumerDivisionId.HasValue ?
            $"Provider (Tenant={ProviderTenantId}, Regulation={ProviderRegulationId}) -> " +
                $"Consumer (Tenant={ConsumerTenantId}, Division={ConsumerDivisionId}){scope}" :
            $"Provider (Tenant={ProviderTenantId}, Regulation={ProviderRegulationId}) -> " +
                $"Consumer (Tenant={ConsumerTenantId}){scope}";
    }
}
