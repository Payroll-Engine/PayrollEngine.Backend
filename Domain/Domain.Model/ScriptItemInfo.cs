using System.Linq;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Script item info
/// </summary>
public class ScriptItemInfo
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="expression">Script expression</param>
    /// <param name="actions">Script actions</param>
    /// <param name="functionType">Script function type</param>
    public ScriptItemInfo(string expression, List<string> actions, FunctionType functionType)
    {
        Expression = expression;
        Actions = actions;
        FunctionType = functionType;
    }

    /// <summary>
    /// Script expression
    /// </summary>
    private string Expression { get; }

    /// <summary>
    /// Test for expression
    /// </summary>
    public bool HasExpression => !string.IsNullOrWhiteSpace(Expression);

    /// <summary>
    /// Script actions
    /// </summary>
    private List<string> Actions { get; }

    /// <summary>
    /// Test for actions
    /// </summary>
    public bool HasActions => Actions != null && Actions.Any();

    /// <summary>
    /// Script function type
    /// </summary>
    public FunctionType FunctionType { get; }
}