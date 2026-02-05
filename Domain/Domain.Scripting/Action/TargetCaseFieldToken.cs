using System;
using PayrollEngine.Action;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;

namespace PayrollEngine.Domain.Scripting.Action;

/// <summary>
/// Action token for target case field
/// </summary>
internal sealed class TargetCaseFieldToken : TokenBase
{
    /// <summary>Default constructor</summary>
    /// <param name="action">Action text</param>
    /// <param name="startIndex">Token start index</param>
    /// <param name="namespace">The token namespace</param>
    internal TargetCaseFieldToken(string action, int startIndex, string @namespace = null) :
        base(TokenType.ReadWriteProperty, action, startIndex, @namespace)
    {
    }

    /// <inheritdoc />
    internal override bool SupportedFunction(FunctionType functionType) =>
        MarkerType.CaseField.SupportedFunction(functionType);

    /// <summary>
    /// Case field token property
    /// </summary>
    private enum FieldProperty
    {
        /// <summary>
        /// Case field value (default)
        /// </summary>
        Value,
        /// <summary>
        /// Case field start
        /// </summary>
        Start,
        /// <summary>
        /// Case field end
        /// </summary>
        End,
        /// <summary>
        /// Duration between start and end (read-only)
        /// </summary>
        Duration,
        /// <summary>
        /// Period between start and end (read-only)
        /// </summary>
        Period
    }

    /// <inheritdoc />
    protected override TokenResultData EvaluateToken(TokenParseData parseData)
    {
        if (string.IsNullOrWhiteSpace(parseData.Text))
        {
            return null;
        }

        // namespace
        var caseFieldName = parseData.Text.EnsureNamespace(Namespace);

        // token property
        var property = parseData.GetProperty<FieldProperty>();

        // code
        switch (property)
        {
            case FieldProperty.Value:
                // set target case field value
                if (!string.IsNullOrWhiteSpace(parseData.PostCode))
                {
                    return new TokenResultData(parseData, $"{nameof(CaseRelationFunction.SetTargetValue)}(\"{caseFieldName}\", ");
                }
                // get target case field value
                return new(parseData, $"new {nameof(ActionValue)}({nameof(CaseRelationFunction.GetTargetValue)}(\"{caseFieldName}\"))");
            case FieldProperty.Start:
                // set target case field start date
                if (!string.IsNullOrWhiteSpace(parseData.PostCode))
                {
                    return new TokenResultData(parseData, $"{nameof(CaseRelationFunction.SetTargetStart)}(\"{caseFieldName}\", ");
                }
                // get case field start date
                return new(parseData, $"new {nameof(ActionValue)}({nameof(CaseRelationFunction.GetTargetStart)}(\"{caseFieldName}\"))");
            case FieldProperty.End:
                // set target case field end date
                if (!string.IsNullOrWhiteSpace(parseData.PostCode))
                {
                    return new TokenResultData(parseData, $"{nameof(CaseRelationFunction.SetTargetEnd)}(\"{caseFieldName}\", ");
                }
                // get case field end date
                return new(parseData, $"new {nameof(ActionValue)}({nameof(CaseRelationFunction.GetTargetEnd)}(\"{caseFieldName}\"))");
            case FieldProperty.Duration:
                if (!string.IsNullOrWhiteSpace(parseData.PostCode))
                {
                    throw new ScriptException($"The action target case field duration is read-only ({caseFieldName}).");
                }
                // target case field period duration
                return new(parseData, $"{nameof(CaseRelationFunction.GetTargetPeriod)}(\"{caseFieldName}\").{nameof(Client.Scripting.DatePeriod.Duration)}");
            case FieldProperty.Period:
                if (!string.IsNullOrWhiteSpace(parseData.PostCode))
                {
                    throw new ScriptException($"The action target case field period is read-only ({caseFieldName}).");
                }
                // target case field period
                return new(parseData, $"{nameof(CaseRelationFunction.GetTargetPeriod)}(\"{caseFieldName}\")");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}