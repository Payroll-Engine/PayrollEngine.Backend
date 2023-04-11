using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Script template
/// </summary>
public class ScriptCompiler
{
    /// <summary>
    /// Script type
    /// </summary>
    private Type ScriptType { get; }

    /// <summary>
    /// Object codes
    /// </summary>
    private IReadOnlyDictionary<string, string> ObjectCodes { get; }

    /// <summary>
    /// Object scripts
    /// </summary>
    private IList<Script> Scripts { get; }

    /// <summary>
    /// Function codes
    /// </summary>
    /// <value>The function codes.</value>
    private IReadOnlyDictionary<FunctionType, string> FunctionCodes { get; }

    private const string ReturnStatement = "return";

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptCompiler"/> class
    /// </summary>
    public ScriptCompiler(Type scriptType, IDictionary<FunctionType, string> functionScripts,
        IEnumerable<Script> scripts = null, IEnumerable<string> embeddedScripts = null)
    {
        ScriptType = scriptType ?? throw new ArgumentNullException(nameof(scriptType));
        if (!typeof(IScriptObject).IsAssignableFrom(scriptType))
        {
            throw new ArgumentException(nameof(scriptType));
        }
        if (functionScripts == null)
        {
            throw new ArgumentNullException(nameof(functionScripts));
        }

        // object codes
        var objectCodes = new Dictionary<string, string>();
        // system embedded scripts
        foreach (var scriptName in CodeFactory.EmbeddedScriptNames)
        {
            objectCodes.Add(scriptName, CodeFactory.GetEmbeddedCodeFile(scriptName));
        }
        // function embedded scripts
        if (embeddedScripts != null)
        {
            foreach (var embeddedScript in embeddedScripts)
            {
                var codeName = embeddedScript.EnsureEnd(".cs");
                objectCodes.Add(codeName, CodeFactory.GetEmbeddedCodeFile(codeName));
            }
        }
        ObjectCodes = new ReadOnlyDictionary<string, string>(objectCodes);

        // scripts
        Scripts = scripts?.ToList();
        var functionCodes = new Dictionary<FunctionType, string>();
        foreach (var functionScript in functionScripts)
        {
            if (string.IsNullOrWhiteSpace(functionScript.Value))
            {
                // empty function
                continue;
            }

            var functionType = functionScript.Key;
            // create function code
            var functionCode = GetFunctionCode(functionType, functionScripts[functionType]);
            functionCodes[functionType] = functionCode;
        }
        FunctionCodes = functionCodes;
    }

    /// <summary>
    /// Compile the assembly
    /// </summary>
    /// <returns>The compile result</returns>
    public ScriptCompileResult Compile()
    {
        if (FunctionCodes.Count == 0)
        {
            throw new PayrollException("Missing function code");
        }

        // collect code to compile
        var codes = new List<string>();
        codes.AddRange(ObjectCodes.Values);
        codes.AddRange(FunctionCodes.Values);
        // global scripts
        if (Scripts != null)
        {
            foreach (var script in Scripts)
            {
                // only global scripts
                if (string.IsNullOrWhiteSpace(script.Value))
                {
                    continue;
                }
                codes.Add(script.Value);
            }
        }

        // compile code
        var compiler = new CSharpCompiler(ScriptType.FullName);
        return compiler.CompileAssembly(new List<string>(codes));
    }

    /// <summary>
    /// Apply template code to a source code region
    /// </summary>
    /// <param name="functionType">The function type</param>
    /// <param name="function">The function code</param>
    private string GetFunctionCode(FunctionType functionType, string function)
    {
        if (string.IsNullOrWhiteSpace(function))
        {
            throw new ArgumentException(nameof(function));
        }

        var functionCode = CodeFactory.GetEmbeddedCodeFile($"Function\\{functionType}Function");
        if (string.IsNullOrWhiteSpace(functionCode))
        {
            throw new PayrollException($"Missing embedded code for function {functionType} in {ScriptType}");
        }

        // apply function expression to the source code region
        functionCode = SetupRegion(functionCode, ScriptingSpecification.FunctionRegion, GetFunctionCode(function));
        return functionCode;
    }

    private string SetupRegion(string template, string regionName, string code)
    {
        // start
        var startMarker = $"#region {regionName}";
        var startIndex = template.IndexOf(startMarker, StringComparison.InvariantCulture);
        if (startIndex < 0)
        {
            throw new PayrollException($"Missing start region with name {regionName} in script type {ScriptType}");
        }

        // end
        var endMarker = "#endregion";
        var endIndex = template.IndexOf(endMarker, startIndex + startMarker.Length, StringComparison.InvariantCulture);
        if (endIndex < 0)
        {
            throw new PayrollException($"Missing end region with name {regionName} in script type {ScriptType}");
        }

        // token replacement
        var placeholder = template.Substring(startIndex, endIndex - startIndex + endMarker.Length);
        return template.Replace(placeholder, code);
    }

    private static string GetFunctionCode(string code)
    {
        if (!HasReturnStatement(code))
        {
            code = $"return {code}";
        }

        return code.EnsureEnd(";");
    }

    // ignore multi line statement
    private static bool HasReturnStatement(string code) =>
        code.Contains(';') || code.StartsWith(ReturnStatement);
}