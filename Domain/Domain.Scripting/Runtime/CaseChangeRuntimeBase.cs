using System;
using System.Linq;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for a case change function
/// </summary>
public abstract class CaseChangeRuntimeBase : CaseRuntimeBase, ICaseChangeRuntime
{
    /// <summary>
    /// The runtime settings
    /// </summary>
    private new CaseChangeRuntimeSettings Settings => base.Settings as CaseChangeRuntimeSettings;

    /// <summary>The case set</summary>
    protected new CaseSet Case => (CaseSet)base.Case;

    /// <summary>The case provider</summary>
    private ICaseProvider CaseProvider => Settings.CaseProvider;

    /// <summary>The case field provider</summary>
    private ICaseFieldProvider CaseFieldProvider => CaseValueProvider.CaseFieldProvider;

    /// <summary>Initializes a new instance of the <see cref="CaseChangeRuntimeBase"/> class</summary>
    /// <param name="settings">The runtime settings</param>
    protected CaseChangeRuntimeBase(CaseChangeRuntimeSettings settings) :
        base(settings)
    {
    }

    #region Case

    /// <inheritdoc />
    public virtual DateTime? CancellationDate => Case.CancellationDate;

    /// <inheritdoc />
    public virtual bool CaseAvailable(string caseName) => GetCase(caseName) != null;

    /// <inheritdoc />
    public virtual void SetCaseAttribute(string caseName, string attributeName, object value)
    {
        if (string.IsNullOrWhiteSpace(caseName))
        {
            throw new ArgumentException(nameof(caseName));
        }
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }

        // case
        var @case = GetCase(caseName);

        // ensure attribute collection
        @case.Attributes ??= new();

        // set or update attribute value
        @case.Attributes[attributeName] = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc />
    public virtual bool RemoveCaseAttribute(string caseName, string attributeName)
    {
        if (string.IsNullOrWhiteSpace(caseName))
        {
            throw new ArgumentException(nameof(caseName));
        }
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }

        // case
        var @case = GetCase(caseName);

        // missing attribute
        if (@case.Attributes == null || !@case.Attributes.ContainsKey(attributeName))
        {
            return false;
        }

        // remove attribute
        return @case.Attributes.Remove(attributeName);
    }

    /// <inheritdoc />
    public string GetReason() => 
        Case.Reason;

    /// <inheritdoc />
    public void SetReason(string reason) => 
        Case.Reason = reason;

    /// <inheritdoc />
    public string GetForecast() => 
        Case.Forecast;

    /// <inheritdoc />
    public void SetForecast(string forecast) => 
        Case.Forecast = forecast;

    /// <summary>
    /// Get case by name
    /// </summary>
    /// <param name="caseName">The name of the case</param>
    /// <returns>The case set matching the name, null on missing case</returns>
    private Case GetCase(string caseName)
    {
        if (string.IsNullOrWhiteSpace(caseName))
        {
            throw new ArgumentException(nameof(caseName));
        }

        // namespace
        caseName = caseName.EnsureNamespace(Namespace);

        // cache or search
        return Case.FindCase(caseName) ?? CaseProvider.GetCaseAsync(Settings.DbContext, Payroll.Id, caseName).Result;
    }

    #endregion

    #region Case Fields

    /// <inheritdoc />
    public string[] GetFieldNames() =>
        Case.Fields?.Select(x => x.Name).ToArray();

    /// <inheritdoc />
    public bool HasFields() =>
        Case.Fields?.Any() ?? false;

    /// <inheritdoc />
    public bool HasField(string caseFieldName)
    {
        // namespace
        caseFieldName = caseFieldName.EnsureNamespace(Namespace);
        return Case.Fields?.Any(x => string.Equals(caseFieldName, x.Name)) ?? false;
    }

    /// <inheritdoc />
    public bool IsFieldComplete(string caseFieldName) =>
        GetCaseFieldSet(caseFieldName).IsComplete();

    /// <inheritdoc />
    public bool IsFieldEmpty(string caseFieldName) =>
        !HasStart(caseFieldName) && !HasEnd(caseFieldName) && !HasValue(caseFieldName);

    /// <inheritdoc />
    public bool FieldAvailable(string caseFieldName) =>
        GetCaseFieldSet(caseFieldName).Status == ObjectStatus.Active;

    /// <inheritdoc />
    public void FieldAvailable(string caseFieldName, bool available) =>
        GetCaseFieldSet(caseFieldName).Status = available ? ObjectStatus.Active : ObjectStatus.Inactive;

    /// <inheritdoc />
    public bool HasStart(string caseFieldName) =>
        GetCaseFieldSet(caseFieldName).Start != null;

    /// <inheritdoc />
    public DateTime? GetStart(string caseFieldName) =>
        GetCaseFieldSet(caseFieldName).Start;

    /// <inheritdoc />
    public void SetStart(string caseFieldName, DateTime? start) =>
        GetCaseFieldSet(caseFieldName).Start = start;

    /// <inheritdoc />
    public void InitStart(string caseFieldName, DateTime? start)
    {
        var caseFieldSet = GetCaseFieldSet(caseFieldName, true);
        caseFieldSet.Start ??= start;
    }

    /// <inheritdoc />
    public bool MandatoryEnd(string caseFieldName) =>
        GetCaseFieldSet(caseFieldName).EndMandatory;

    /// <inheritdoc />
    public bool HasEnd(string caseFieldName) =>
        GetCaseFieldSet(caseFieldName).End != null;

    /// <inheritdoc />
    public DateTime? GetEnd(string caseFieldName) =>
        GetCaseFieldSet(caseFieldName).End;

    /// <inheritdoc />
    public void SetEnd(string caseFieldName, DateTime? end) =>
        GetCaseFieldSet(caseFieldName).End = end;

    /// <inheritdoc />
    public void InitEnd(string caseFieldName, DateTime? end)
    {
        var caseFieldSet = GetCaseFieldSet(caseFieldName, true);
        caseFieldSet.End ??= end;
    }

    /// <inheritdoc />
    public bool MandatoryValue(string caseFieldName) =>
        GetCaseFieldSet(caseFieldName).ValueMandatory;

    /// <inheritdoc />
    public int GetValueType(string caseFieldName) =>
        (int)GetCaseFieldSet(caseFieldName).ValueType;

    /// <inheritdoc />
    public bool HasValue(string caseFieldName) =>
        GetCaseFieldSet(caseFieldName).HasValue;

    /// <inheritdoc />
    public object GetValue(string caseFieldName) =>
        GetCaseFieldSet(caseFieldName).GetValue(TenantCulture);

    /// <inheritdoc />
    public void SetValue(string caseFieldName, object value) =>
        GetCaseFieldSet(caseFieldName).SetValue(value);

    /// <inheritdoc />
    public virtual void InitValue(string caseFieldName, object value)
    {
        var caseFieldSet = GetCaseFieldSet(caseFieldName, true);
        if (caseFieldSet.Value == null)
        {
            caseFieldSet.SetValue(value);
        }
    }

    /// <inheritdoc />
    public virtual bool AddCaseValueTag(string caseFieldName, string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException(nameof(tag));
        }

        var caseFieldSet = GetCaseFieldSet(caseFieldName);
        if (caseFieldSet == null)
        {
            return false;
        }
        caseFieldSet.Tags ??= [];
        if (!caseFieldSet.Tags.Contains(tag))
        {
            caseFieldSet.Tags.Add(tag);
        }
        return caseFieldSet.Tags.Contains(tag);
    }

    /// <inheritdoc />
    public virtual bool RemoveCaseValueTag(string caseFieldName, string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException(nameof(tag));
        }

        var caseFieldSet = GetCaseFieldSet(caseFieldName);
        if (caseFieldSet?.Tags == null || !caseFieldSet.Tags.Contains(tag))
        {
            return false;
        }
        caseFieldSet.Tags.Remove(tag);
        return !caseFieldSet.Tags.Contains(tag);
    }

    /// <inheritdoc />
    public override object GetCaseFieldAttribute(string caseFieldName, string attributeName) =>
        GetCaseFieldSet(caseFieldName)?.Attributes?.GetValue<object>(attributeName);

    /// <inheritdoc />
    public virtual void SetCaseFieldAttribute(string caseFieldName, string attributeName, object value)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }

        // case field
        var caseField = GetCaseFieldSet(caseFieldName);
        if (caseField == null)
        {
            throw new ArgumentException($"unknown case field {caseFieldName}.");
        }
        // ensure case field attribute collection
        caseField.Attributes ??= new();

        // set or update case field attribute value
        caseField.Attributes[attributeName] = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc />
    public virtual bool RemoveCaseFieldAttribute(string caseFieldName, string attributeName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }

        // case field
        var caseField = GetCaseFieldSet(caseFieldName);
        if (caseField == null)
        {
            throw new ArgumentException($"unknown case field {caseFieldName}.");
        }
        if (caseField.Attributes == null || !caseField.Attributes.ContainsKey(attributeName))
        {
            return false;
        }

        // remove case field attribute
        return caseField.Attributes.Remove(attributeName);
    }

    /// <inheritdoc />
    public override object GetCaseValueAttribute(string caseFieldName, string attributeName) =>
        GetCaseFieldSet(caseFieldName)?.ValueAttributes?.GetValue<object>(attributeName);

    /// <inheritdoc />
    public virtual void SetCaseValueAttribute(string caseFieldName, string attributeName, object value)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }

        // case field
        var caseField = GetCaseFieldSet(caseFieldName);
        if (caseField == null)
        {
            throw new ArgumentException($"unknown case field {caseFieldName}");
        }
        // ensure case value attribute collection
        caseField.ValueAttributes ??= new();

        // set or update case value attribute value
        caseField.ValueAttributes[attributeName] = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc />
    public virtual bool RemoveCaseValueAttribute(string caseFieldName, string attributeName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }

        // case field
        var caseField = GetCaseFieldSet(caseFieldName);
        if (caseField == null)
        {
            throw new ArgumentException($"Unknown case field {caseFieldName}.");
        }
        if (caseField.ValueAttributes == null || !caseField.ValueAttributes.ContainsKey(attributeName))
        {
            return false;
        }

        // remove case value attribute
        return caseField.ValueAttributes.Remove(attributeName);
    }

    /// <summary>
    /// Get case field by name
    /// </summary>
    /// <param name="caseFieldName">The name of the case field</param>
    /// <param name="addField">Add unknown field</param>
    /// <returns>The case field matching the name, script exception on missing case field</returns>
    protected CaseFieldSet GetCaseFieldSet(string caseFieldName, bool addField = false)
    {
        // namespace
        caseFieldName = caseFieldName.EnsureNamespace(Namespace);

        var caseFieldSet = Case.FindCaseField(caseFieldName);
        if (caseFieldSet == null)
        {
            var caseField = CaseFieldProvider.GetCaseFieldAsync(Settings.DbContext, caseFieldName).Result;
            if (caseField == null)
            {
                throw new ScriptException($"Unknown case field {caseFieldName}.");
            }
            caseFieldSet = new(caseField)
            {
                CaseSlot = new CaseValueReference(caseFieldName).CaseSlot
            };
            if (addField)
            {
                Case.Fields.Add(caseFieldSet);
            }
        }
        return caseFieldSet;
    }

    #endregion

}