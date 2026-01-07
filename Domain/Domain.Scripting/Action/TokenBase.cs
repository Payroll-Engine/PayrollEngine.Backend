using System;
using PayrollEngine.Client.Scripting;

namespace PayrollEngine.Domain.Scripting.Action;

/// <summary>
/// Base action token
/// </summary>
internal abstract class TokenBase
{
    /// <summary>
    /// Length of the token marker
    /// </summary>
    internal const int TokenMarkerLength = 2;

    /// <summary>
    /// Token type
    /// </summary>
    internal TokenType TokenType { get; }

    /// <summary>
    /// Token action
    /// </summary>
    private string Action { get; }

    /// <summary>
    /// Token start index within the action string
    /// </summary>
    internal int StartIndex { get; }

    /// <summary>
    /// Action namespace
    /// </summary>
    internal string Namespace { get; }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="tokenType">Token type</param>
    /// <param name="action">Token action</param>
    /// <param name="startIndex">Token start index within the action string</param>
    /// <param name="namespace">The action namespace</param>
    protected TokenBase(TokenType tokenType, string action, int startIndex, string @namespace = null)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("Empty token action.", nameof(action));
        }
        if (startIndex >= action.Length)
        {
            throw new ArgumentException("Invalid action token start index.", nameof(startIndex));
        }

        TokenType = tokenType;
        Action = action;
        StartIndex = startIndex;
        Namespace = @namespace;
    }

    /// <summary>
    /// Evaluate the token
    /// </summary>
    /// <returns>Token result including the token code</returns>
    internal TokenResultData EvaluateToken()
    {
        var parseData = ParseToken();
        return parseData == null ? null : EvaluateToken(parseData);
    }

    /// <summary>
    /// Evaluate token parse data
    /// </summary>
    /// <param name="parseData">Parse data</param>
    /// <returns>Token result data</returns>
    protected abstract TokenResultData EvaluateToken(TokenParseData parseData);

    /// <summary>
    /// Test for supported functions
    /// </summary>
    /// <param name="functionType">Function type</param>
    /// <returns>True if this token is supported by the function</returns>
    internal virtual bool SupportedFunction(FunctionType functionType) => true;

    /// <summary>
    /// Parse action token
    /// </summary>
    /// <returns>Token parse data</returns>
    protected virtual TokenParseData ParseToken()
    {
        var start = StartIndex + TokenMarkerLength;
        string postCode = null;
        var propertyIndex = -1;
        var parameterIndex = -1;
        var parameterCount = 0;
        var textLen = 0;
        var endIndex = 0;
        for (var index = start; index < Action.Length; index++)
        {
            var c = Action[index];

            // property start
            if (propertyIndex < 0 && parameterIndex < 0 && c == '.')
            {
                propertyIndex = index;
            }
            // argument start
            else if (TokenType == TokenType.Method && c == '(')
            {
                if (parameterIndex < 0)
                {
                    parameterIndex = index;
                }
                parameterCount++;
            }
            // argument end
            else if (TokenType == TokenType.Method && c == ')')
            {
                parameterCount--;
                if (parameterCount == 0)
                {
                    // token type method end
                    endIndex = index;
                    break;
                }
            }
            // within argument
            else if (TokenType == TokenType.Method && parameterCount > 0)
            {
                continue;
            }
            // end of text, ignoring spaces before the token name
            else if (!char.IsLetterOrDigit(c) && !(textLen == 0 && c == ' '))
            {
                // write property
                if (TokenType == TokenType.ReadWriteProperty)
                {
                    var assignmentIndex = AssignmentIndex(index);
                    if (assignmentIndex > index)
                    {
                        // token type read/write property end
                        endIndex = assignmentIndex + 1;
                        postCode = ")";
                        break;
                    }
                }
                // token type read property end
                endIndex = index;
                break;
            }

            if (propertyIndex < 0 && parameterIndex < 0)
            {
                textLen++;
            }
        }

        // text
        if (textLen <= 0)
        {
            throw new ScriptException($"Action {TokenTypeName} token: invalid name in {Action}.");
        }
        var text = Action.Substring(start, textLen).Trim();

        // property at line end
        if (endIndex == 0)
        {
            endIndex = Action.Length;
            endIndex = Math.Min(endIndex, Action.Length);
            endIndex = Math.Max(endIndex, start + textLen);
            endIndex = Math.Max(endIndex, parameterIndex);
        }

        // property
        string property = null;
        if (propertyIndex > 0)
        {
            property = Action.Substring(propertyIndex + 1, endIndex - propertyIndex - 1).Trim();
        }

        // parameters
        if (parameterCount != 0)
        {
            throw new ScriptException($"Action {TokenTypeName} token: unbalanced parameter brackets in line {Action}.");
        }
        string parameters = null;
        if (parameterIndex > 0)
        {
            parameters = Action.Substring(parameterIndex + 1, endIndex - parameterIndex - 1).Trim();
        }

        return new TokenParseData(
            endIndex: endIndex,
            text: text,
            property: property,
            parameters: parameters,
            parameterIndex: parameterIndex,
            postCode: postCode);
    }

    /// <summary>
    /// Get index of token assignment character
    /// </summary>
    /// <param name="index">Start index</param>
    private int AssignmentIndex(int index)
    {
        if (index >= Action.Length || !Action.Substring(index).TrimStart().StartsWith('='))
        {
            return -1;
        }
        return Action.IndexOf('=', index);
    }

    /// <summary>
    /// Token type name
    /// </summary>
    internal string TokenTypeName =>
        GetType().Name.RemoveFromEnd(nameof(TokenBase));

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Action.Substring(StartIndex)} [{TokenTypeName}]";
    }
}