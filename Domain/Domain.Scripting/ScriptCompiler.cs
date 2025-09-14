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
    /// Script owner
    /// </summary>
    private object ScriptOwner { get; }

    /// <summary>
    /// Script owner type
    /// </summary>
    private Type ScriptOwnerType => ScriptOwner.GetType();

    /// <summary>
    /// Owner name
    /// </summary>
    private string ScriptOwnerName { get; }

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

    /// <summary>Dump compiler source files (default: false)</summary>
    public static bool DumpCompilerSources { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptCompiler"/> class
    /// </summary>
    /// <param name="scriptOwner">The script owner</param>
    /// <param name="functionScripts">The function scripts</param>
    /// <param name="scripts">The scripts</param>
    /// <param name="embeddedScripts">The embedded scripts</param>
    public ScriptCompiler(object scriptOwner, IDictionary<FunctionType, string> functionScripts,
        IEnumerable<Script> scripts = null, IEnumerable<string> embeddedScripts = null)
    {
        // script owner
        ScriptOwner = scriptOwner ?? throw new ArgumentNullException(nameof(scriptOwner));
        if (!typeof(IScriptObject).IsAssignableFrom(ScriptOwnerType))
        {
            throw new ArgumentException(nameof(ScriptOwnerType));
        }
        ScriptOwnerName = scriptOwner is INamedObject namedObject ?
            namedObject.Name :
            ScriptOwnerType.Name;

        // function scripts
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
            throw new PayrollException("Missing function code.");
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
        var compiler = new CSharpCompiler(
            assemblyName: ScriptOwnerType.FullName,
            dumpCompilerSource: DumpCompilerSources);
        return compiler.CompileAssembly(codes);
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
            throw new PayrollException($"Missing embedded code for function {functionType} in script {ScriptOwnerName} [{ScriptOwnerType}].");
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
            throw new PayrollException($"Missing start region with name {regionName} in script {ScriptOwnerName} [{ScriptOwnerType}].");
        }

        // end
        var endMarker = "#endregion";
        var endIndex = template.IndexOf(endMarker, startIndex + startMarker.Length, StringComparison.InvariantCulture);
        if (endIndex < 0)
        {
            throw new PayrollException($"Missing end region with name {regionName} in script {ScriptOwnerName} [{ScriptOwnerType}].");
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