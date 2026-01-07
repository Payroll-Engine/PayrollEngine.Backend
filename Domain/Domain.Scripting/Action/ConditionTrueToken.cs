namespace PayrollEngine.Domain.Scripting.Action;

/// <summary>
/// Action token for true condition
/// </summary>
internal sealed class ConditionTrueToken : TokenBase
{
    /// <summary>
    /// Action token for condition
    /// </summary>
    /// <param name="action">Action text</param>
    /// <param name="startIndex">Token start index</param>
    internal ConditionTrueToken(string action, int startIndex) : 
        base(TokenType.ConditionTrue, action, startIndex)
    {
    }

    /// <inheritdoc />
    protected override TokenResultData EvaluateToken(TokenParseData parseData) =>
        new(parseData, null);

    /// <inheritdoc />
    protected override TokenParseData ParseToken() =>
        new(StartIndex + TokenMarkerLength);
}