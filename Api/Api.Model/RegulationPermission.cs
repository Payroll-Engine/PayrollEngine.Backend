using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The regulation permission API object
/// </summary>
public class RegulationPermission : ApiObjectBase
{
    /// <summary>The tenant id</summary>
    [Required]
    public int TenantId { get; set; }

    /// <summary>The regulation id</summary>
    [Required]
    public int RegulationId { get; set; }

    /// <summary>The permission tenant id</summary>
    [Required]
    public int PermissionTenantId { get; set; }

    /// <summary>The permission division id</summary>
    public int? PermissionDivisionId { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{RegulationId} {base.ToString()}";
}