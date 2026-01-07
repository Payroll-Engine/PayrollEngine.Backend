namespace PayrollEngine.Domain.Scripting.Action;

/// <summary>
/// Action  condition token
/// </summary>
internal sealed class ConditionToken : TokenBase
{
    /// <summary>
    /// Action token for condition
    /// </summary>
    /// <param name="action">Action text</param>
    /// <param name="startIndex">Token start index</param>
    internal ConditionToken(string action, int startIndex) : 
        base(TokenType.Condition, action, startIndex)
    {
    }

    /// <inheritdoc />
    protected override TokenResultData EvaluateToken(TokenParseData parseData) =>
        new(parseData, null);

    /// <inheritdoc />
    protected override TokenParseData ParseToken() =>
        // marker only
        new(StartIndex + 1);
}