using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll case relation audit
/// </summary>
public class LookupValueAudit : ScriptAuditDomainObject
{
    /// <summary>
    /// The case relation id
    /// </summary>
    public int LookupValueId { get; set; }

    /// <summary>
    /// The lookup value key
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The lookup key hash code
    /// The hash is used by database indexes
    /// </summary>
    public int KeyHash { get; set; }

    /// <summary>
    /// The lookup range value
    /// </summary>
    public decimal? RangeValue { get; set; }

    /// <summary>
    /// The lookup value as JSON
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The localized lookup values
    /// </summary>
    public Dictionary<string, string> ValueLocalizations { get; set; }

    /// <summary>
    /// The lookup hash code
    /// The hash is used by database indexes
    /// </summary>
    public int LookupHash { get; set; }

    /// <inheritdoc/>
    public LookupValueAudit()
    {
    }

    /// <inheritdoc/>
    public LookupValueAudit(LookupValueAudit copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }
}