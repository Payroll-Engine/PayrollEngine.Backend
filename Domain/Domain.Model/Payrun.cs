using System;
using System.Collections.Generic;
using PayrollEngine.Client.Scripting;

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
    public override bool HasAnyExpression =>
        !string.IsNullOrWhiteSpace(StartExpression) ||
        !string.IsNullOrWhiteSpace(EmployeeAvailableExpression) ||
        !string.IsNullOrWhiteSpace(EmployeeStartExpression) ||
        !string.IsNullOrWhiteSpace(EmployeeEndExpression) ||
        !string.IsNullOrWhiteSpace(WageTypeAvailableExpression) ||
        !string.IsNullOrWhiteSpace(EndExpression);

    /// <inheritdoc/>
    public override bool HasAnyAction => false;

    /// <inheritdoc/>
    public override bool HasObjectScripts => false;

    /// <inheritdoc/>
    public override List<FunctionType> GetFunctionTypes() => FunctionTypes;

    /// <inheritdoc/>
    public override string GetFunctionScript(FunctionType functionType) =>
        functionType switch
        {
            FunctionType.PayrunStart => StartExpression,
            FunctionType.PayrunEmployeeAvailable => EmployeeAvailableExpression,
            FunctionType.PayrunEmployeeStart => EmployeeStartExpression,
            FunctionType.PayrunEmployeeEnd => EmployeeEndExpression,
            FunctionType.PayrunWageTypeAvailable => WageTypeAvailableExpression,
            FunctionType.PayrunEnd => EndExpression,
            _ => null
        };

    /// <inheritdoc/>
    public override List<string> GetFunctionActions(FunctionType functionType) => null;

    /// <inheritdoc/>
    public override IEnumerable<string> GetEmbeddedScriptNames() =>
        ScriptProvider.GetEmbeddedScriptNames(FunctionType.PayrunBase);

    #endregion

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}