using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payrun
/// </summary>
public class Payrun : ScriptDomainObject, INamedObject, IEquatable<Payrun>
{
    private static readonly List<FunctionType> FunctionTypes =
    [
        FunctionType.PayrunStart,
        FunctionType.PayrunEmployeeAvailable,
        FunctionType.PayrunEmployeeStart,
        FunctionType.PayrunEmployeeEnd,
        FunctionType.PayrunWageTypeAvailable,
        FunctionType.PayrunEnd
    ];
    private static readonly IEnumerable<string> EmbeddedScriptNames = new[]
    {
        "Cache\\Cache.cs",
        "Function\\PayrunFunction.cs"
    };

    /// <summary>
    /// The payroll id (immutable)
    /// </summary>
    public int PayrollId { get; set; }

    /// <summary>
    /// The payrun name (immutable)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized payrun name
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The default payrun reason
    /// </summary>
    public string DefaultReason { get; set; }

    /// <summary>
    /// The localized default payrun reasons
    /// </summary>
    public Dictionary<string, string> DefaultReasonLocalizations { get; set; }

    /// <summary>
    /// The payrun start expression
    /// </summary>
    public string StartExpression { get; set; }

    /// <summary>
    /// The expression which evaluates if the employee is available
    /// </summary>
    public string EmployeeAvailableExpression { get; set; }

    /// <summary>
    /// The expression evaluates the employee start
    /// </summary>
    public string EmployeeStartExpression { get; set; }

    /// <summary>
    /// The expression evaluates the employee end
    /// </summary>
    public string EmployeeEndExpression { get; set; }

    /// <summary>
    /// The expression which evaluates if the wage type is available
    /// </summary>
    public string WageTypeAvailableExpression { get; set; }

    /// <summary>
    /// The payrun end expression
    /// </summary>
    public string EndExpression { get; set; }

    /// <summary>
    /// The payrun retro time type
    /// </summary>
    public RetroTimeType RetroTimeType { get; set; }

    /// <inheritdoc/>
    public Payrun()
    {
    }

    /// <inheritdoc/>
    public Payrun(Payrun copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(Payrun compare) =>
        CompareTool.EqualProperties(this, compare);

    #region Scripting

    /// <inheritdoc/>
    public override bool HasExpression =>
        GetFunctionScripts().Values.Any(x => !string.IsNullOrWhiteSpace(x));

    /// <inheritdoc/>
    public override bool HasObjectScripts => false;

    /// <inheritdoc/>
    public override List<FunctionType> GetFunctionTypes() => FunctionTypes;

    /// <inheritdoc/>
    public override IDictionary<FunctionType, string> GetFunctionScripts()
    {
        var scripts = new Dictionary<FunctionType, string>();
        if (!string.IsNullOrWhiteSpace(StartExpression))
        {
            scripts.Add(FunctionType.PayrunStart, StartExpression);
        }
        if (!string.IsNullOrWhiteSpace(EmployeeAvailableExpression))
        {
            scripts.Add(FunctionType.PayrunEmployeeAvailable, EmployeeAvailableExpression);
        }
        if (!string.IsNullOrWhiteSpace(EmployeeStartExpression))
        {
            scripts.Add(FunctionType.PayrunEmployeeStart, EmployeeStartExpression);
        }
        if (!string.IsNullOrWhiteSpace(EmployeeEndExpression))
        {
            scripts.Add(FunctionType.PayrunEmployeeEnd, EmployeeEndExpression);
        }
        if (!string.IsNullOrWhiteSpace(WageTypeAvailableExpression))
        {
            scripts.Add(FunctionType.PayrunWageTypeAvailable, WageTypeAvailableExpression);
        }
        if (!string.IsNullOrWhiteSpace(EndExpression))
        {
            scripts.Add(FunctionType.PayrunEnd, EndExpression);
        }
        return scripts;
    }

    /// <inheritdoc/>
    public override IEnumerable<string> GetEmbeddedScriptNames() =>
        EmbeddedScriptNames;

    #endregion

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}