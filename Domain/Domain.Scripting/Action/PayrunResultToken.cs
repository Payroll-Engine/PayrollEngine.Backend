using PayrollEngine.Client.Scripting.Function;

namespace PayrollEngine.Domain.Scripting.Action;

/// <summary>
/// Action token for payrun result
/// </summary>
internal sealed class PayrunResultToken : TokenBase
{
    /// <summary>Default constructor</summary>
    /// <param name="action">Action text</param>
    /// <param name="startIndex">Token start index</param>
    internal PayrunResultToken(string action, int startIndex) :
        base(TokenType.ReadWriteProperty, action, startIndex)
    {
    }

    /// <inheritdoc />
    protected override TokenResultData EvaluateToken(TokenParseData parseData)
    {
        var valueName = parseData.Text;
        if (string.IsNullOrWhiteSpace(valueName))
        {
            return null;
        }

        // set runtime value code
        if (!string.IsNullOrWhiteSpace(parseData.PostCode))
        {
            return new(parseData, $"{nameof(PayrunFunction.SetPayrunResultValue)}(\"{valueName}\", ");
        }

        // get runtime value code
        return new(parseData, $"{nameof(PayrunFunction.GetPayrunResultValue)}(\"{valueName}\")");
    }
}