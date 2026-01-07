namespace PayrollEngine.Domain.Scripting.Action;

/// <summary>
/// Action token for false condition
/// </summary>
internal sealed class ConditionFalseToken : TokenBase
{
    /// <summary>
    /// Action token for condition
    /// </summary>
    /// <param name="action">Action text</param>
    /// <param name="startIndex">Token start index</param>
    internal ConditionFalseToken(string action, int startIndex) : 
        base(TokenType.ConditionFalse, action, startIndex)
    {
    }

    /// <inheritdoc />
    protected override TokenResultData EvaluateToken(TokenParseData parseData) =>
        new(parseData, null);

    /// <inheritdoc />
    protected override TokenParseData ParseToken() =>
        new(StartIndex + TokenMarkerLength);
}