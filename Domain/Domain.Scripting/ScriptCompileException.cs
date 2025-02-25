using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Payroll script compile exception
/// </summary>
public class ScriptCompileException : PayrollScriptException
{
    /// <summary>Initializes a new instance of the <see cref="T:PayrollEngine.Domain.Scripting.ScriptCompileException"></see> class.</summary>
    /// <param name="failures">The diagnostic results</param>
    internal ScriptCompileException(IList<string> failures) :
        base(string.Join(Environment.NewLine, failures))
    {
    }
}