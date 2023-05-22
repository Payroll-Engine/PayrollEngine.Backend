using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The regulation permission API object
/// </summary>
public class RegulationShare : ApiObjectBase
{
    /// <summary>The provider tenant id</summary>
    [Required]
    public int ProviderTenantId { get; set; }

    /// <summary>The provider regulation id</summary>
    [Required]
    public int ProviderRegulationId { get; set; }

    /// <summary>The consumer tenant id</summary>
    [Required]
    public int ConsumerTenantId { get; set; }

    /// <summary>The consumer division id</summary>
    public int? ConsumerDivisionId { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{ProviderRegulationId} {base.ToString()}";
}