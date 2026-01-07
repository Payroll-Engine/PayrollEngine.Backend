using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using PayrollEngine.Domain.Model;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Domain.Scripting.Action;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Script compiler
/// </summary>
public class ScriptCompiler
{
    private const string ReturnStatement = "return";

    private ActionParser ActionParser { get; }

    private IScriptObject ScriptObject { get; }
    private IDictionary<FunctionType, string> FunctionScripts { get; }
    private IList<Script> Scripts { get; }
    private IEnumerable<string> EmbeddedScriptNames { get; }

    /// <summary>
    /// Script object type
    /// </summary>
    private Type ScriptObjectType => ScriptObject.GetType();

    /// <summary>
    /// Owner name
    /// </summary>
    private string ScriptOwnerName { get; }

    /// <summary>
    /// Dump compiler source files (default: false)
    /// </summary>
    public static bool DumpCompilerSources { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptCompiler"/> class
    /// </summary>
    /// <param name="scriptObject">The script object</param>
    /// <param name="functionScripts">The function scripts</param>
    /// <param name="scripts">The scripts</param>
    /// <param name="embeddedScriptNames">The embedded scripts</param>
    /// <param name="namespace">The function namespace</param>
    public ScriptCompiler(IScriptObject scriptObject, IDictionary<FunctionType, string> functionScripts,
        IEnumerable<Script> scripts = null, IEnumerable<string> embeddedScriptNames = null, string @namespace = null)
    {
        // script owner
        ScriptObject = scriptObject ?? throw new ArgumentNullException(nameof(scriptObject));
        FunctionScripts = functionScripts ?? throw new ArgumentNullException(nameof(functionScripts));
        EmbeddedScriptNames = embeddedScriptNames;

        // parser
        ActionParser = new(@namespace);

        // script
        if (!typeof(IScriptObject).IsAssignableFrom(ScriptObjectType))
        {
            throw new ArgumentException(nameof(ScriptObjectType));
        }
        ScriptOwnerName = scriptObject is INamedObject namedObject ?
            namedObject.Name :
            ScriptObjectType.Name;
        Scripts = scripts?.ToList();
    }

    /// <summary>
    /// Compile the assembly
    /// </summary>
    /// <returns>The compile result</returns>
    public ScriptCompileResult Compile()
    {
        // object codes
        var objectCodes = BuildObjectCodes();
        if (objectCodes.Count == 0)
        {
            throw new PayrollException("Missing function code.");
        }

        // action codes
        var actionCodes = BuildActionResults();

        // function codes (run after action codes)
        var functionCodes = BuildFunctionCodes(actionCodes);
        if (functionCodes.Count == 0)
        {
            throw new PayrollException("Missing function code.");
        }

        // collect code to compile
        var codes = new List<string>();

        // add object codes
        foreach (var objectCode in objectCodes)
        {
            var functionCode = functionCodes.Any(x => objectCode.Key.EndsWith($"{x.Key}Function.cs"));
            if (functionCode)
            {
                continue;
            }
            codes.AddRange(objectCode.Value);
        }

        // add function codes
        codes.AddRange(functionCodes.Values);

        // scripts
        if (Scripts != null)
        {
            var scripts = Scripts.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => x.Value);
            codes.AddRange(scripts);
        }

        // compile code
        var compiler = new CSharpCompiler(
            assemblyName: ScriptObjectType.FullName,
            dumpCompilerSource: DumpCompilerSources);
        return compiler.CompileAssembly(codes);
    }

    /// <summary>
    /// Build function codes
    /// </summary>
    /// <param name="actions">Function action codes</param>
    private Dictionary<FunctionType, string> BuildFunctionCodes(Dictionary<FunctionType,
        List<ActionResult>> actions)
    {
        var functionCodes = new Dictionary<FunctionType, string>();
        foreach (var functionScript in FunctionScripts)
        {
            var functionType = functionScript.Key;
            actions.TryGetValue(functionType, out var actionCodes);
            var functionCode = ApplyFunctionCode(
                functionType: functionType,
                function: FunctionScripts[functionType],
                actionResults: actionCodes);
            functionCodes[functionType] = functionCode;
        }
        return functionCodes;
    }

    /// <summary>
    /// Build action results
    /// </summary>
    private Dictionary<FunctionType, List<ActionResult>> BuildActionResults()
    {
        var actionCodes = new Dictionary<FunctionType, List<ActionResult>>();
        foreach (var functionScript in FunctionScripts)
        {
            var functionType = functionScript.Key;
            var actions = ScriptObject.GetFunctionActions(functionType);
            if (actions != null && actions.Any())
            {
                actionCodes[functionType] = ActionParser.Parse(functionType, actions);
            }
        }
        return actionCodes;
    }

    /// <summary>
    /// Build object codes
    /// </summary>
    private Dictionary<string, string> BuildObjectCodes()
    {
        var objectCodes = new Dictionary<string, string>();
        // system embedded scripts
        foreach (var scriptName in ScriptProvider.GetEmbeddedScriptNames())
        {
            objectCodes.Add(scriptName, CodeFactory.GetEmbeddedCodeFile(scriptName));
        }
        // function embedded scripts
        if (EmbeddedScriptNames != null)
        {
            foreach (var script in EmbeddedScriptNames)
            {
                var codeName = script.EnsureEnd(".cs");
                objectCodes.Add(codeName, CodeFactory.GetEmbeddedCodeFile(codeName));
            }
        }
        return objectCodes;
    }

    /// <summary>
    /// Apply template code to a source code region
    /// </summary>
    /// <param name="functionType">The function type</param>
    /// <param name="function">The function code</param>
    /// <param name="actionResults">The action results</param>
    private string ApplyFunctionCode(FunctionType functionType, string function,
        List<ActionResult> actionResults)
    {
        var functionCode = CodeFactory.GetEmbeddedCodeFile($"Function\\{functionType}Function");
        if (string.IsNullOrWhiteSpace(functionCode))
        {
            throw new PayrollException($"Missing embedded code for function {functionType} in script {ScriptOwnerName} [{ScriptObjectType}].");
        }

        // function action
        if (actionResults != null && actionResults.Any())
        {
            // action code
            var actionCode = string.Join(Environment.NewLine, actionResults.Select(x => x.Code));
            functionCode = SetupRegion(functionCode, ScriptingSpecification.ActionRegion, actionCode);

            // action invoke
            var invokeCode = string.Join(Environment.NewLine, actionResults.Select(x => x.InvokeCode));
            functionCode = SetupRegion(functionCode, ScriptingSpecification.ActionInvokeRegion, invokeCode);
        }

        // function expression
        if (!string.IsNullOrWhiteSpace(function))
        {
            functionCode = SetupRegion(
                template: functionCode,
                regionName: ScriptingSpecification.FunctionRegion,
                code: EnsureCodeReturn(function));
        }

        return functionCode;
    }

    /// <summary>
    /// Setup code region
    /// </summary>
    /// <param name="template">Code template</param>
    /// <param name="regionName">Code region name</param>
    /// <param name="code">Code to insert</param>
    private string SetupRegion(string template, string regionName, string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return template;
        }

        // start
        var startMarker = $"#region {regionName}";
        var startIndex = template.IndexOf(startMarker, StringComparison.InvariantCulture);
        if (startIndex < 0)
        {
            throw new PayrollException($"Missing start region with name {regionName} in script {ScriptOwnerName} [{ScriptObjectType}].");
        }

        // end
        var endMarker = "#endregion";
        var endIndex = template.IndexOf(endMarker, startIndex + startMarker.Length, StringComparison.InvariantCulture);
        if (endIndex < 0)
        {
            throw new PayrollException($"Missing end region with name {regionName} in script {ScriptOwnerName} [{ScriptObjectType}].");
        }

        // code
        var builder = new StringBuilder();
        var indent = RegionIndent(template, endIndex);
        var indentText = indent > 0 ? new string(' ', indent) : string.Empty;
        builder.AppendLine(startMarker);
        builder.AppendLine();
        builder.AppendLine(code);
        builder.Append($"{indentText}{endMarker}");

        // token replacement
        var placeholder = template.Substring(startIndex, endIndex - startIndex + endMarker.Length);
        return template.Replace(placeholder, builder.ToString());
    }

    /// <summary>
    /// Count the region indent
    /// </summary>
    /// <param name="template">Script template</param>
    /// <param name="index">Region start index</param>
    private static int RegionIndent(string template, int index)
    {
        var indent = 0;
        index--;
        while (index > 0)
        {
            if (template[index] == '\n')
            {
                break;
            }
            indent++;
            index--;
        }
        return indent;
    }

    /// <summary>
    /// Ensure return statement in code
    /// </summary>
    private static string EnsureCodeReturn(string code)
    {
        if (!HasReturnStatement(code))
        {
            code = $"{ReturnStatement} {code}";
        }
        return code.EnsureEnd(";");
    }

    /// <summary>
    /// Ignore multi line statement
    /// </summary>
    private static bool HasReturnStatement(string code) =>
        code.Contains(';') || code.StartsWith(ReturnStatement);
}