using System.Collections.Generic;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll lookup audit
/// </summary>
public class LookupAudit : ScriptAuditDomainObject
{
    /// <summary>
    /// The case relation id
    /// </summary>
    public int LookupId { get; set; }

    /// <summary>
    /// The lookup name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized lookup names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The regulation description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized regulation descriptions
    /// </summary>
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// Lookup range mode
    /// </summary>
    public LookupRangeMode RangeMode { get; set; }

    /// <summary>
    /// The lookup range size
    /// </summary>
    public decimal? RangeSize { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public LookupAudit()
    {
    }

    /// <inheritdoc/>
    public LookupAudit(LookupAudit copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }
}