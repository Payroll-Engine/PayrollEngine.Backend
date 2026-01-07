using System;
using System.Linq;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// runtime for a case script function
/// </summary>
public abstract class CaseRelationRuntimeBase : PayrollRuntimeBase, ICaseRelationRuntime
{
    /// <summary>
    /// The runtime settings
    /// </summary>
    protected new CaseRelationRuntimeSettings Settings => base.Settings as CaseRelationRuntimeSettings;

    /// <summary>
    /// The case relation
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public CaseRelation CaseRelation { get; }

    /// <summary>
    /// The case values of the relation source
    /// </summary>
    internal CaseSet SourceCaseSet => Settings.SourceCaseSet;

    /// <summary>
    /// The case values of the relation target
    /// </summary>
    internal CaseSet TargetCaseSet => Settings.TargetCaseSet;

    /// <inheritdoc />
    protected CaseRelationRuntimeBase(CaseRelation caseRelation, CaseRelationRuntimeSettings settings) :
        base(settings)
    {
        CaseRelation = caseRelation ?? throw new ArgumentNullException(nameof(caseRelation));
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwner => RelationName;

    /// <summary>Gets the case relation name</summary>
    private string RelationName
    {
        get
        {
            var sourceCase = string.IsNullOrWhiteSpace(SourceCaseSet.CaseSlot)
                ? SourceCaseName
                : $"{SourceCaseName}.{SourceCaseSet.CaseSlot}";
            var targetCase = string.IsNullOrWhiteSpace(TargetCaseSet.CaseSlot)
                ? TargetCaseName
                : $"{TargetCaseName}.{TargetCaseSet.CaseSlot}";
            return $"{sourceCase} > {targetCase}";
        }
    }

    #region Source Case

    /// <inheritdoc />
    public string SourceCaseName => SourceCaseSet.Name;

    /// <inheritdoc />
    public string SourceCaseSlot => SourceCaseSet.CaseSlot;

    /// <inheritdoc />
    public DateTime? SourceCaseCancellationDate => SourceCaseSet.CancellationDate;

    /// <inheritdoc />
    public string[] GetSourceFieldNames() =>
        SourceCaseSet.Fields?.Select(x => x.Name).ToArray();

    /// <inheritdoc />
    public bool HasSourceFields() =>
        SourceCaseSet.Fields?.Any() ?? false;

    /// <inheritdoc />
    public bool HasSourceField(string caseFieldName) =>
        SourceCaseSet.Fields?.Any(x => string.Equals(caseFieldName, x.Name)) ?? false;

    /// <inheritdoc />
    public bool IsSourceFieldComplete(string caseFieldName) =>
        GetSourceCaseField(caseFieldName).IsComplete();

    /// <inheritdoc />
    public bool IsSourceFieldEmpty(string caseFieldName) =>
        !HasSourceStart(caseFieldName) && !HasSourceEnd(caseFieldName) && !HasSourceValue(caseFieldName);

    /// <inheritdoc />
    public bool HasSourceStart(string caseFieldName) =>
        GetSourceCaseField(caseFieldName).Start != null;

    /// <inheritdoc />
    public DateTime? GetSourceStart(string caseFieldName) =>
        GetSourceCaseField(caseFieldName).Start;

    /// <inheritdoc />
    public bool HasSourceEnd(string caseFieldName) =>
        GetSourceCaseField(caseFieldName).End != null;

    /// <inheritdoc />
    public DateTime? GetSourceEnd(string caseFieldName) =>
        GetSourceCaseField(caseFieldName).End;

    /// <inheritdoc />
    public int GetSourceValueType(string caseFieldName) =>
        (int)GetSourceCaseField(caseFieldName).ValueType;

    /// <inheritdoc />
    public bool HasSourceValue(string caseFieldName) =>
        GetSourceCaseField(caseFieldName).Value != null;

    /// <inheritdoc />
    public object GetSourceValue(string caseFieldName) =>
        GetSourceCaseField(caseFieldName).GetValue(TenantCulture);

    private CaseFieldSet GetSourceCaseField(string caseFieldName)
    {
        var caseField = SourceCaseSet.FindCaseField(caseFieldName);
        if (caseField == null)
        {
            throw new ScriptException($"Unknown source case field {caseFieldName}.");
        }
        return caseField;
    }

    /// <inheritdoc />
    public object GetSourceCaseAttribute(string attributeName) =>
        SourceCaseSet.Attributes?.GetValue<object>(attributeName);

    /// <inheritdoc />
    public object GetSourceCaseFieldAttribute(string caseFieldName, string attributeName) =>
        GetSourceCaseField(caseFieldName)?.Attributes?.GetValue<object>(attributeName);

    /// <inheritdoc />
    public object GetSourceCaseValueAttribute(string caseFieldName, string attributeName) =>
        GetSourceCaseField(caseFieldName)?.ValueAttributes?.GetValue<object>(attributeName);

    #endregion

    #region Target Case

    /// <inheritdoc />
    public string TargetCaseName => TargetCaseSet.Name;

    /// <inheritdoc />
    public string TargetCaseSlot => TargetCaseSet.CaseSlot;

    /// <inheritdoc />
    public DateTime? TargetCaseCancellationDate => TargetCaseSet.CancellationDate;

    /// <inheritdoc />
    public string[] GetTargetFieldNames() =>
        TargetCaseSet.Fields?.Select(x => x.Name).ToArray();

    /// <inheritdoc />
    public bool HasTargetFields() =>
        TargetCaseSet.Fields?.Any() ?? false;

    /// <inheritdoc />
    public bool HasTargetField(string caseFieldName) =>
        TargetCaseSet.Fields?.Any(x => string.Equals(caseFieldName, x.Name)) ?? false;

    /// <inheritdoc />
    public bool IsTargetFieldComplete(string caseFieldName) =>
        GetTargetCaseField(caseFieldName).IsComplete();

    /// <inheritdoc />
    public bool IsTargetFieldEmpty(string caseFieldName) =>
        !HasTargetStart(caseFieldName) && !HasTargetEnd(caseFieldName) && !HasTargetValue(caseFieldName);

    /// <inheritdoc />
    public bool HasTargetStart(string caseFieldName) =>
        GetTargetCaseField(caseFieldName).Start != null;

    /// <inheritdoc />
    public DateTime? GetTargetStart(string caseFieldName) =>
        GetTargetCaseField(caseFieldName).Start;

    /// <inheritdoc />
    public void SetTargetStart(string caseFieldName, DateTime? start) =>
        GetTargetCaseField(caseFieldName).Start = start;

    /// <inheritdoc />
    public void InitTargetStart(string caseFieldName, DateTime? start)
    {
        if (!HasTargetStart(caseFieldName))
        {
            SetTargetStart(caseFieldName, start);
        }
    }

    /// <inheritdoc />
    public bool HasTargetEnd(string caseFieldName) =>
        GetTargetCaseField(caseFieldName).End != null;

    /// <inheritdoc />
    public DateTime? GetTargetEnd(string caseFieldName) =>
        GetTargetCaseField(caseFieldName).End;

    /// <inheritdoc />
    public void SetTargetEnd(string caseFieldName, DateTime? end) =>
        GetTargetCaseField(caseFieldName).End = end;

    /// <inheritdoc />
    public void InitTargetEnd(string caseFieldName, DateTime? end)
    {
        if (!HasTargetEnd(caseFieldName))
        {
            SetTargetEnd(caseFieldName, end);
        }
    }

    /// <inheritdoc />
    public int GetTargetValueType(string caseFieldName) =>
        (int)GetTargetCaseField(caseFieldName).ValueType;

    /// <inheritdoc />
    public bool HasTargetValue(string caseFieldName) =>
        GetTargetCaseField(caseFieldName).Value != null;

    /// <inheritdoc />
    public object GetTargetValue(string caseFieldName) =>
        GetTargetCaseField(caseFieldName).GetValue(TenantCulture);

    /// <inheritdoc />
    public void SetTargetValue(string caseFieldName, object value) =>
        GetTargetCaseField(caseFieldName).SetValue(value);

    /// <inheritdoc />
    public void InitTargetValue(string caseFieldName, object value)
    {
        if (!HasTargetValue(caseFieldName))
        {
            SetTargetValue(caseFieldName, value);
        }
    }

    /// <inheritdoc />
    public bool TargetFieldAvailable(string caseFieldName) =>
        GetTargetCaseField(caseFieldName).Status == ObjectStatus.Active;

    /// <inheritdoc />
    public void TargetFieldAvailable(string caseFieldName, bool available) =>
        GetTargetCaseField(caseFieldName).Status = available ? ObjectStatus.Active : ObjectStatus.Inactive;

    private CaseFieldSet GetTargetCaseField(string caseFieldName)
    {
        var caseField = TargetCaseSet.FindCaseField(caseFieldName);
        if (caseField == null)
        {
            throw new ScriptException($"Unknown target case field {caseFieldName}.");
        }
        return caseField;
    }

    /// <inheritdoc />
    public object GetTargetCaseAttribute(string attributeName) =>
        TargetCaseSet.Attributes?.GetValue<object>(attributeName);

    /// <inheritdoc />
    public object GetTargetCaseFieldAttribute(string caseFieldName, string attributeName) =>
        GetTargetCaseField(caseFieldName)?.Attributes?.GetValue<object>(attributeName);

    /// <inheritdoc />
    public object GetTargetCaseValueAttribute(string caseFieldName, string attributeName) =>
        GetTargetCaseField(caseFieldName)?.ValueAttributes?.GetValue<object>(attributeName);

    #endregion

    #region Init and Copy

    /// <inheritdoc />
    public virtual void InitStart(string sourceFieldName, string targetFieldName)
    {
        if (!HasTargetStart(targetFieldName))
        {
            CopyStart(sourceFieldName, targetFieldName);
        }
    }

    /// <inheritdoc />
    public virtual void CopyStart(string sourceFieldName, string targetFieldName) =>
        SetTargetStart(targetFieldName, GetSourceStart(sourceFieldName));

    /// <inheritdoc />
    public virtual void InitEnd(string sourceFieldName, string targetFieldName)
    {
        if (!HasTargetEnd(targetFieldName))
        {
            CopyEnd(sourceFieldName, targetFieldName);
        }
    }

    /// <inheritdoc />
    public virtual void CopyEnd(string sourceFieldName, string targetFieldName) =>
        SetTargetEnd(targetFieldName, GetSourceEnd(sourceFieldName));

    /// <inheritdoc />
    public virtual void InitValue(string sourceFieldName, string targetFieldName)
    {
        if (!HasTargetValue(targetFieldName))
        {
            CopyValue(sourceFieldName, targetFieldName);
        }
    }

    /// <inheritdoc />
    public virtual void CopyValue(string sourceFieldName, string targetFieldName) =>
        SetTargetValue(targetFieldName, GetSourceValue(sourceFieldName));

    #endregion

}