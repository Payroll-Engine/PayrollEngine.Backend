using System;
using PayrollEngine.Action;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;

namespace PayrollEngine.Domain.Scripting.Action;

/// <summary>
/// Action token for source case field
/// </summary>
internal sealed class SourceCaseFieldToken : TokenBase
{
    /// <summary>Default constructor</summary>
    /// <param name="action">Action text</param>
    /// <param name="startIndex">Token start index</param>
    /// <param name="namespace">The token namespace</param>
    internal SourceCaseFieldToken(string action, int startIndex, string @namespace = null) :
        base(TokenType.ReadProperty, action, startIndex, @namespace)
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

        if (!string.IsNullOrWhiteSpace(parseData.PostCode))
        {
            throw new ScriptException($"The action source case field is read-only ({caseFieldName}).");
        }

        // code
        switch (property)
        {
            case FieldProperty.Value:
                // get source case field value
                return new(parseData, $"new {nameof(ActionValue)}({nameof(CaseRelationFunction.GetSourceValue)}(\"{caseFieldName}\"))");
            case FieldProperty.Start:
                // get source case field start date
                return new(parseData, $"new {nameof(ActionValue)}({nameof(CaseRelationFunction.GetSourceStart)}(\"{caseFieldName}\"))");
            case FieldProperty.End:
                // get source case field end date
                return new(parseData, $"new {nameof(ActionValue)}({nameof(CaseRelationFunction.GetSourceEnd)}(\"{caseFieldName}\"))");
            case FieldProperty.Duration:
                // source case field period duration
                return new(parseData, $"{nameof(CaseRelationFunction.GetSourcePeriod)}(\"{caseFieldName}\").{nameof(Client.Scripting.DatePeriod.Duration)}");
            case FieldProperty.Period:
                // source case field period
                return new(parseData, $"{nameof(CaseRelationFunction.GetSourcePeriod)}(\"{caseFieldName}\")");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}