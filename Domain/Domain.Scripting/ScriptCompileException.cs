using System.Collections.Generic;
using System.Text;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Payroll script compile exception
/// </summary>
public class ScriptCompileException : PayrollScriptException
{
    /// <summary>Initializes a new instance of the <see cref="T:PayrollEngine.Domain.Scripting.ScriptCompileException"></see> class.</summary>
    /// <param name="failures">The diagnostic results</param>
    internal ScriptCompileException(IList<string> failures) :
        base(GetMessage(failures))
    {
    }

    private static string GetMessage(IList<string> failures)
    {
        if (failures.Count == 1)
        {
            return failures[0];
        }

        var buffer = new StringBuilder();
        buffer.AppendLine($"{failures.Count} compile errors:");
        foreach (var failure in failures)
        {
            buffer.AppendLine(failure);
        }
        return buffer.ToString();
    }
}