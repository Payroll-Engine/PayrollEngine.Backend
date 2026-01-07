using PayrollEngine.Client.Scripting.Function;

namespace PayrollEngine.Domain.Scripting.Action;

/// <summary>
/// Action token for lookup value
/// </summary>
internal sealed class LookupToken : TokenBase
{
    /// <summary>Default constructor</summary>
    /// <param name="action">Action text</param>
    /// <param name="startIndex">Token start index</param>
    /// <param name="namespace">The token namespace</param>
    internal LookupToken(string action, int startIndex, string @namespace = null) : 
        base(TokenType.Method, action, startIndex, @namespace)
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
        var lookupName = parseData.Text.EnsureNamespace(Namespace);

        // code
        var code = $"{nameof(PayrollFunction.GetLookupValue)}(\"{lookupName}\"";
        return new(parseData, string.IsNullOrWhiteSpace(parseData.Parameters) ?
            code :
            code + ',');
    }
}