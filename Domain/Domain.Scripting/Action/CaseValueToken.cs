using PayrollEngine.Client.Scripting.Function;

namespace PayrollEngine.Domain.Scripting.Action;

/// <summary>
/// Action token for case value
/// </summary>
internal sealed class CaseValueToken : TokenBase
{
    internal static string CaseValuesVariableName => "caseValues";

    /// <summary>Default constructor</summary>
    /// <param name="action">Action text</param>
    /// <param name="startIndex">Token start index</param>
    /// <param name="namespace">Token namespace</param>
    internal CaseValueToken(string action, int startIndex, string @namespace = null) :
        base(TokenType.ReadProperty, action, startIndex, @namespace)
    {
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

        // code
        return new(parseData: parseData,
            // single case value access
            code: $"{nameof(PayrollFunction.GetFieldValue)}(\"{caseFieldName}\")",
            // multi case value access
            alternateCode: $"{CaseValuesVariableName}[\"{caseFieldName}\"]");
    }
}