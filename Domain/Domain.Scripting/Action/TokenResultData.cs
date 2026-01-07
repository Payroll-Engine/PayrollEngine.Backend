namespace PayrollEngine.Domain.Scripting.Action;

/// <summary>
/// Token result data
/// </summary>
internal sealed class TokenResultData : TokenParseData
{
    /// <summary>Default constructor</summary>
    /// <param name="parseData">Token parse data</param>
    /// <param name="code">Token code</param>
    /// <param name="alternateCode">Token alternate code (see condition token)</param>
    internal TokenResultData(TokenParseData parseData, string code, string alternateCode = null) : 
        base(parseData)
    {
        Code = code;
        AlternateCode = alternateCode;
    }

    internal string Code { get; }
    internal string AlternateCode { get; }
}