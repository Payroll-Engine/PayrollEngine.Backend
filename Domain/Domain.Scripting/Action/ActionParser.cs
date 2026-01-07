using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using PayrollEngine.Action;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;

namespace PayrollEngine.Domain.Scripting.Action;

/// <summary>Action parser</summary>
internal sealed class ActionParser
{
    /// <summary>Action line type</summary>
    private enum ActionLineType
    {
        /// <summary>Regular action</summary>
        Action,
        /// <summary>Action break condition</summary>
        BreakCondition,
        /// <summary>Condition action</summary>
        Condition,
        /// <summary>Assign action</summary>
        Assign
    }

    /// <summary>Default constructor</summary>
    /// <param name="namespace">The action namespace</param>
    internal ActionParser(string @namespace = null)
    {
        Namespace = @namespace;
    }

    private string Namespace { get; }

    /// <summary>Parse object actions</summary>
    /// <param name="functionType">The function type</param>
    /// <param name="actions">The script actions</param>
    /// <returns>Parsed c# code</returns>
    internal List<ActionResult> Parse(FunctionType functionType, ICollection<string> actions)
    {
        var codes = new List<ActionResult>();
        var index = 1;

        // parse actions
        var conditionTrueMarker = MarkerType.ConditionTrue.GetSyntax();
        var conditionFalseMarker = MarkerType.ConditionFalse.GetSyntax();
        var decimalFunction = functionType.IsDecimalResult();
        foreach (var action in actions)
        {
            // line
            // allow single quoting for constant action strings
            var line = action.Replace('\'', '"').Trim();
            // empty line or comment
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(ActionSpecification.ActionCommentMarker))
            {
                continue;
            }

            // line type
            ActionLineType lineType = ActionLineType.Action;
            // decimal function: set lats action as assignment
            if (decimalFunction && action == actions.Last())
            {
                lineType = ActionLineType.Assign;
            }
            // action condition
            else if (action.StartsWith(ActionSpecification.ActionConditionMarker))
            {
                lineType = action.Contains(conditionTrueMarker) || action.Contains(conditionFalseMarker) ?
                    ActionLineType.Condition :
                    ActionLineType.BreakCondition;
            }

            // parse action code
            var actionCode = ParseAction(functionType, line, lineType, index);
            if (actionCode != null)
            {
                codes.Add(actionCode);
            }
            index++;
        }

        return codes;
    }

    /// <summary>Parse action line</summary>
    /// <param name="functionType">The function type</param>
    /// <param name="line">The script action</param>
    /// <param name="lineType">The action line type</param>
    /// <param name="index">The script index</param>
    /// <returns>Parsed c# code</returns>
    private ActionResult ParseAction(FunctionType functionType, string line, ActionLineType lineType, int index)
    {
        // tokens
        var tokens = ExtractTokens(line, Namespace);
        ValidateTokens(functionType, tokens);
        var tokenCode = GetTokenCode(line, lineType, tokens);

        // action token
        return lineType switch
        {
            // action
            ActionLineType.Action => new(
                GetActionInvokeCode(index),
                GetActionCode(tokenCode, index)),
            // condition
            ActionLineType.BreakCondition => new(
                GetBreakConditionInvokeCode(functionType, index),
                GetBreakConditionCode(tokenCode, index)),
            // condition
            ActionLineType.Condition =>
                new(GetActionInvokeCode(index),
                    GetActionCode(tokenCode, index)),
            // assign
            ActionLineType.Assign =>
                new(GetActionAssignInvokeCode(index),
                    GetActionAssignCode(functionType, tokenCode, index)),
            _ => throw new ArgumentOutOfRangeException(nameof(lineType))
        };
    }

    #region Action Break Condition

    /// <summary>
    /// Get condition action invoke code
    /// </summary>
    /// <param name="functionType">The function type</param>
    /// <param name="index">Method index used as method name postfix</param>
    private static string GetBreakConditionInvokeCode(FunctionType functionType, int index)
    {
        string value;
        switch (functionType)
        {
            // boolean function result
            case FunctionType.CaseAvailable:
            case FunctionType.CaseBuild:
            case FunctionType.CaseValidate:
            case FunctionType.CaseRelationBuild:
            case FunctionType.CaseRelationValidate:
                value = "false";
                break;
            // object function result
            case FunctionType.CollectorStart:
            case FunctionType.CollectorEnd:
            case FunctionType.WageTypeResult:
                value = "null";
                break;
            // decimal function result
            case FunctionType.CollectorApply:
            case FunctionType.WageTypeValue:
                value = "0m";
                break;
            default:
                return GetActionInvokeCode(index);
        }

        var builder = new StringBuilder();
        builder.AppendLine($"        if (!Action{index}())");
        builder.AppendLine("        {");
        builder.AppendLine($"          return {value};");
        builder.AppendLine("        }");
        return builder.ToString();
    }

    /// <summary>
    /// Get action invoke code
    /// </summary>
    /// <param name="code">Action code</param>
    /// <param name="index">Method index used as method name postfix</param>
    private static string GetBreakConditionCode(string code, int index)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"    private bool Action{index}()");
        builder.AppendLine("    {");
        builder.AppendLine($"      {code}");
        builder.AppendLine("    }");
        return builder.ToString();
    }

    #endregion

    #region Action Assing

    /// <summary>
    /// Get condition action assign code
    /// </summary>
    /// <param name="index">Method index used as method name postfix</param>
    private static string GetActionAssignInvokeCode(int index)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"        var actionValue{index} = Action{index}();");
        builder.AppendLine($"        if (actionValue{index} != null)");
        builder.AppendLine("        {");
        builder.AppendLine($"          return actionValue{index};");
        builder.AppendLine("        }");
        return builder.ToString();
    }

    /// <summary>
    /// Get action assign code
    /// </summary>
    /// <param name="functionType">The function type</param>
    /// <param name="code">Action code</param>
    /// <param name="index">Method index used as method name postfix</param>
    private static string GetActionAssignCode(FunctionType functionType, string code, int index)
    {
        var type = functionType.GetFunctionValueType();
        if (type != null)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"    private {type.Name}? Action{index}()");
            builder.AppendLine("    {");
            builder.AppendLine($"      {code}");
            builder.AppendLine("    }");
            return builder.ToString();
        }
        throw new ScriptException($"Function {functionType} has no value assignment.");
    }

    #endregion

    #region Action

    /// <summary>
    /// Get action invoke code
    /// </summary>
    /// <param name="index">Method index used as method name postfix</param>
    private static string GetActionInvokeCode(int index) =>
        $"        Action{index}();";

    /// <summary>
    /// Get action code
    /// </summary>
    /// <param name="code">Action code</param>
    /// <param name="index">Method index used as method name postfix</param>
    private static string GetActionCode(string code, int index)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"    private void Action{index}()");
        builder.AppendLine("    {");
        builder.AppendLine($"      {code}");
        builder.AppendLine("    }");
        return builder.ToString();
    }

    #endregion

    #region Action Token

    /// <summary>
    /// Get action tokens to code
    /// </summary>
    /// <param name="line">Action line</param>
    /// <param name="lineType">Action line type</param>
    /// <param name="tokens">Action tokens</param>
    private static string GetTokenCode(string line, ActionLineType lineType, List<TokenBase> tokens)
    {
        var codeBuffer = new StringBuilder();
        var postCodeBuffer = new StringBuilder();
        var processedTokens = new List<TokenBase>();

        // parse tokens
        var parsedTokens = EvaluateTokens(tokens);

        // collect case value names
        var caseValueTokens = new List<TokenBase>();
        var caseValueNames = new HashSet<string>();
        foreach (var token in tokens.Where(token => token is CaseValueToken))
        {
            caseValueTokens.Add(token);
            caseValueNames.Add(parsedTokens[token].Text);
        }

        // multi case value access
        var multiValueTokens = caseValueNames.Count > 1;
        // pre code
        if (multiValueTokens)
        {
            codeBuffer.Append(GetCaseValuesCode(caseValueTokens, parsedTokens));
        }

        // return statement
        if (lineType is ActionLineType.Assign)
        {
            codeBuffer.Append("return ");
        }

        // condition
        var hasTrueCondition = tokens.Any(x => x.TokenType == TokenType.ConditionTrue);

        // process action line
        var inCondition = false;
        var index = 0;
        while (index < line.Length)
        {
            var token = tokens.FirstOrDefault(x => x.StartIndex == index);

            // unhandled character
            if (token == null)
            {
                codeBuffer.Append(line[index]);
                index++;
                continue;
            }

            // tokens
            processedTokens.Add(token);
            var tokenResult = parsedTokens[token];

            // condition
            switch (token.TokenType)
            {
                case TokenType.Condition:
                    codeBuffer.Append(hasTrueCondition ? "          if (" : "        return ");
                    index = tokenResult.EndIndex;
                    continue;
                case TokenType.ConditionTrue:
                    codeBuffer.AppendLine(")");
                    codeBuffer.AppendLine("        { ");
                    inCondition = true;
                    index = tokenResult.EndIndex;
                    continue;
                case TokenType.ConditionFalse:
                    EnsureEnd(codeBuffer, ';');
                    codeBuffer.AppendLine();
                    codeBuffer.AppendLine("        }");
                    codeBuffer.AppendLine("        else");
                    codeBuffer.AppendLine("        {");
                    index = tokenResult.EndIndex;
                    continue;
            }

            // code
            var code = token is CaseValueToken && multiValueTokens ?
                tokenResult.AlternateCode :
                tokenResult.Code;
            if (!string.IsNullOrWhiteSpace(code))
            {
                code = code.Trim('\r', '\n', ' ');

                // condition action
                if (lineType is ActionLineType.Condition)
                {
                    code = code.TrimStart(ActionSpecification.ActionConditionMarker, ' ');
                }
                codeBuffer.Append(code);

                // post code (write property)
                if (!string.IsNullOrWhiteSpace(tokenResult.PostCode))
                {
                    var postCode = tokenResult.PostCode.Trim('\r', '\n', ' ');
                    postCodeBuffer.Insert(0, postCode);
                }
            }

            // index update
            var endIndex = tokenResult.ParameterIndex > 0 ?
                tokenResult.ParameterIndex + 1 :
                tokenResult.EndIndex;
            if (endIndex <= index)
            {
                throw new ScriptException($"Invalid line syntax in action: {line}.");
            }
            index = endIndex;
        }

        // closing condition
        if (inCondition)
        {
            EnsureEnd(codeBuffer, ';');
            codeBuffer.AppendLine();
            codeBuffer.AppendLine("        }");
        }

        // ensure all tokens are processed
        if (processedTokens.Count != tokens.Count)
        {
            throw new ScriptException($"Invalid token syntax in action: {line}.");
        }

        // action code
        var actionCode = (codeBuffer.ToString() + postCodeBuffer).Trim();
        if (!actionCode.EndsWith('}'))
        {
            actionCode = actionCode.EnsureEnd(";");
        }
        return actionCode;
    }

    private static void EnsureEnd(StringBuilder builder, char c, bool trim = true)
    {
        if (trim)
        {
            while (builder[builder.Length - 1] == ' ')
            {
                builder.Remove(builder.Length - 1, 1);
            }
        }
        if (builder[builder.Length - 1] != c)
        {
            builder.Append(c);
        }
    }

    /// <summary>
    /// Get code for multiple case values
    /// </summary>
    /// <param name="tokens">Action tokens</param>
    /// <param name="tokenData">Token data</param>
    private static string GetCaseValuesCode(List<TokenBase> tokens, Dictionary<TokenBase, TokenResultData> tokenData)
    {
        var code = new StringBuilder();
        code.Append($"var {CaseValueToken.CaseValuesVariableName} = {nameof(PayrollFunction.GetCaseValues)}(");
        foreach (var caseValueToken in tokens)
        {
            if (caseValueToken != tokens.First())
            {
                code.Append(", ");
            }
            code.Append($"\"{tokenData[caseValueToken].Text}\"");
        }
        code.AppendLine(");");
        return code.ToString();
    }

    /// <summary>
    /// Evaluate action tokens
    /// </summary>
    /// <param name="tokens"></param>
    /// <returns></returns>
    private static Dictionary<TokenBase, TokenResultData> EvaluateTokens(List<TokenBase> tokens)
    {
        var parsedTokens = new Dictionary<TokenBase, TokenResultData>();
        foreach (var token in tokens)
        {
            parsedTokens[token] = token.EvaluateToken();
        }
        return parsedTokens;
    }

    /// <summary>.
    /// Validate the tokens by function type
    /// </summary>
    /// <param name="functionType">Function type</param>
    /// <param name="tokens">Action tokens</param>
    private static void ValidateTokens(FunctionType functionType, List<TokenBase> tokens)
    {
        // supported function tokens
        foreach (var token in tokens)
        {
            if (!token.SupportedFunction(functionType))
            {
                throw new ScriptException($"Unsupported token {token.TokenTypeName} in function {functionType}.");
            }
        }
    }

    /// <summary>
    /// Extract line tokens
    /// </summary>
    /// <param name="line">Action line</param>
    /// <param name="namespace">Action namespace</param>
    private static List<TokenBase> ExtractTokens(string line, string @namespace = null)
    {
        var tokens = new List<TokenBase>();

        var inConditionToken = false;
        var inRefToken = false;
        var index = 0;
        TokenBase conditionTrueToken = null;
        TokenBase conditionFalseToken = null;
        foreach (var c in line)
        {
            TokenBase token = null;
            if (inConditionToken)
            {
                var tokenIndex = index - 1;
                // condition true
                if (c == ActionSpecification.ActionConditionTrueMarker)
                {
                    if (conditionTrueToken != null)
                    {
                        throw new ScriptException($"Duplicated condition-true token in action {line}.");
                    }
                    token = new ConditionTrueToken(line, tokenIndex);
                    conditionTrueToken = token;
                }
                // condition false
                else if (c == ActionSpecification.ActionConditionFalseMarker)
                {
                    if (conditionFalseToken != null)
                    {
                        throw new ScriptException($"Duplicated condition-false token in action {line}.");
                    }
                    token = new ConditionFalseToken(line, tokenIndex);
                    conditionFalseToken = token;
                }
                inConditionToken = false;
            }
            else if (inRefToken)
            {
                var tokenIndex = index - 1;
                if (c == ActionSpecification.LookupTokenMarker)
                {
                    // lookup
                    token = new LookupToken(line, tokenIndex, @namespace);
                }
                else if (c == ActionSpecification.CaseFieldTokenMarker)
                {
                    // case field
                    token = new CaseFieldToken(line, tokenIndex, @namespace);
                }
                else if (c == ActionSpecification.SourceCaseFieldTokenMarker)
                {
                    // source case field
                    token = new SourceCaseFieldToken(line, tokenIndex, @namespace);
                }
                else if (c == ActionSpecification.TargetCaseFieldTokenMarker)
                {
                    // target case field
                    token = new TargetCaseFieldToken(line, tokenIndex, @namespace);
                }
                else if (c == ActionSpecification.CaseValueTokenMarker)
                {
                    // case value
                    token = new CaseValueToken(line, tokenIndex, @namespace);
                }
                else if (c == ActionSpecification.RuntimeValueTokenMarker)
                {
                    // runtime value
                    token = new RuntimeValueToken(line, tokenIndex);
                }
                else if (c == ActionSpecification.PayrunResultTokenMarker)
                {
                    // payrun result
                    token = new PayrunResultToken(line, tokenIndex);
                }
                else if (c == ActionSpecification.CollectorTokenMarker)
                {
                    // collector
                    token = new CollectorToken(line, tokenIndex, @namespace);
                }
                else if (c == ActionSpecification.WageTypeTokenMarker)
                {
                    // wage type
                    token = new WageTypeToken(line, tokenIndex, @namespace);
                }
                inRefToken = false;
            }
            else if (c == ActionSpecification.RefTokenMarker)
            {
                inRefToken = true;
            }
            else if (c == ActionSpecification.ActionConditionMarker)
            {
                if (index == 0)
                {
                    // condition
                    token = new ConditionToken(line, index);
                }
                else
                {
                    inConditionToken = true;
                }
            }

            // new token
            if (token != null)
            {
                tokens.Add(token);
            }

            index++;
        }
        return tokens;
    }

    #endregion

}
