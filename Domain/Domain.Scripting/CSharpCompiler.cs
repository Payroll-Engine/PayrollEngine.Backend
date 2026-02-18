//#define COMPILER_PERFORMANCE
#if COMPILER_PERFORMANCE
#define LOG_STOPWATCH
#endif

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using PayrollEngine.IO;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Build .net assembly from C# code
/// </summary>
internal sealed class CSharpCompiler
{
    private string AssemblyName { get; }
    private AssemblyInfo AssemblyInfo { get; }
    private bool DumpCompilerSource { get; }

    internal CSharpCompiler(string assemblyName, bool dumpCompilerSource) :
        this(assemblyName, null, dumpCompilerSource)
    {
    }

    private CSharpCompiler(string assemblyName, AssemblyInfo assemblyInfo, bool dumpCompilerSource)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            throw new ArgumentException(nameof(assemblyName));
        }

        AssemblyName = assemblyName;
        AssemblyInfo = assemblyInfo;
        DumpCompilerSource = dumpCompilerSource;
    }

    static CSharpCompiler()
    {
        // references
        foreach (var defaultReferenceType in DefaultReferenceTypes)
        {
            AddDefaultReference(defaultReferenceType);
        }
        foreach (var assemblyName in DefaultReferenceAssemblies)
        {
            AddDefaultReference(assemblyName);
        }

        // namespaces
        foreach (var defaultNamespaceName in DefaultNamespaceNames)
        {
            AddDefaultNamespace(defaultNamespaceName);
        }
    }

    #region References

    private static readonly Type[] DefaultReferenceTypes =
    [
        // any object
        typeof(object),
        // used for object references
        typeof(AssemblyTargetedPatchBandAttribute),
        // used for dynamic objects
        typeof(DynamicAttribute),
        // used for linq
        typeof(Enumerable)
    ];

    private static readonly string[] DefaultReferenceAssemblies =
    [
        // core
        "System",
        "System.Runtime",
        // dynamic objects
        "Microsoft.CSharp",
        "netstandard",
        // readonly collections
        "System.Collections",
        // json
        "System.Text.Json",
        // regular expressions
        "System.Text.RegularExpressions",

        // reports
        "System.Data.Common",
        "System.ComponentModel.TypeConverter",
        "System.ComponentModel.Primitives",
        "System.ComponentModel",
        "System.Xml.ReaderWriter",
        "System.Private.Xml"
    ];

    /// <summary>
    /// List of additional assembly references that are added to the
    /// compiler parameters in order to execute the script code.
    /// </summary>
    private static readonly HashSet<MetadataReference> DefaultReferences = [];

    /// <summary>
    /// Gets the c# language version
    /// </summary>
    /// <value>The c# version</value>
    private LanguageVersion? languageVersion;
    private LanguageVersion LanguageVersion
    {
        get
        {
            if (!languageVersion.HasValue)
            {
                if (!Enum.TryParse(ScriptingSpecification.CSharpLanguageVersion, out LanguageVersion version))
                {
                    throw new PayrollException($"Invalid c# language version: {ScriptingSpecification.CSharpLanguageVersion}.");
                }
                languageVersion = version;
            }
            return languageVersion.Value;
        }
    }

    /// <summary>
    /// Add a default assembly reference
    /// </summary>
    /// <param name="type">The type within the assembly</param>
    private static void AddDefaultReference(Type type) =>
        AddReference(type.Assembly, DefaultReferences);

    /// <summary>
    /// Add a default assembly reference
    /// </summary>
    /// <param name="assemblyName">The assembly name to refer</param>
    private static void AddDefaultReference(string assemblyName) =>
        AddReference(Assembly.Load(new AssemblyName(assemblyName)), DefaultReferences);


    private static void AddReference(Assembly assembly, HashSet<MetadataReference> references)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        var reference = MetadataReference.CreateFromFile(assembly.Location);
        references.Add(reference);
    }

    #endregion

    #region Namespaces

    private static readonly string[] DefaultNamespaceNames =
    [
        "System",
        "System.Text"
    ];

    /// <summary>
    /// List of additional namespaces to add to the script
    /// </summary>
    private static readonly HashSet<string> DefaultNamespaces = [];

    /// <summary>
    /// Adds a default assembly namespace
    /// </summary>
    /// <param name="name">The namespace name</param>
    private static void AddDefaultNamespace(string name) => AddNamespace(name, DefaultNamespaces);

    /// <summary>
    /// Adds a namespace to the referenced namespaces
    /// used at compile time.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="namespaces">The collection to insert the namespace</param>
    private static void AddNamespace(string name, HashSet<string> namespaces)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }
        namespaces.Add(name);
    }

    #endregion

    /// <summary>
    /// Compiles and runs the source code for a complete assembly.
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="codes">The C# source codes</param>
    /// <returns>The compile result</returns>
    internal ScriptCompileResult CompileAssembly(int tenantId, IList<string> codes)
    {
        if (codes == null)
        {
            throw new ArgumentNullException(nameof(codes));
        }

        if (DumpCompilerSource)
        {
            DumpScriptSourceFiles(codes);
        }

        LogStopwatch.Start(nameof(CompileAssembly));

        // parser options: supported c# language
        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion);

        // parse code
        var syntaxTrees = new List<SyntaxTree>();
        // parse assembly title, product and version
        if (AssemblyInfo != null)
        {
            syntaxTrees.Add(SyntaxFactory.ParseSyntaxTree(GetAssemblyAttributes()));
        }
        // parse source codes
        foreach (var code in codes)
        {
            syntaxTrees.Add(SyntaxFactory.ParseSyntaxTree(SourceText.From(code), options));
        }

        // collect script code
        var script = new StringBuilder();
        foreach (var code in codes)
        {
            script.AppendLine($"// {new('-', 80)}");
            script.AppendLine(code);
        }
        var scriptCode = script.ToString();

        // references
        var allReferences = new List<MetadataReference>(DefaultReferences);

        // create bits
        using var peStream = new MemoryStream();
        var assemblyName = $"{tenantId}_{scriptCode.ToPayrollHash()}_{AssemblyName}";
        var compilation = CSharpCompilation.Create(assemblyName,
                syntaxTrees,
                allReferences,
                new(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default))
            .Emit(peStream);

        // error handling
        if (!compilation.Success)
        {
            throw new ScriptCompileException(GetCompilerFailures(compilation));
        }

        // compile result
        peStream.Seek(0, SeekOrigin.Begin);
        var result = new ScriptCompileResult(scriptCode, peStream.ToArray());

        LogStopwatch.Stop(nameof(CompileAssembly));

        return result;
    }

    /// <summary>Dump scripts to a temporary folder</summary>
    /// <param name="codes">Source codes</param>
    /// <remarks>Debug only</remarks>
    private void DumpScriptSourceFiles(IList<string> codes)
    {
        // target folder: ScriptDump/AssemblyName/TimeStamp/
        var dumpFolder = EnsureDumpFolderName();

        // dump all code files
        for (var i = 0; i < codes.Count; i++)
        {
            // source code file
            var code = codes[i];

            // file name
            string dumpFileName = null;
            // support name comment on first line in source file, example:
            // /* MyScriptType */
            var startMarker = code.IndexOf("/*", StringComparison.InvariantCulture);
            var endMarker = code.IndexOf("*/", StringComparison.InvariantCulture);
            var length = endMarker - startMarker;
            if (length > 0 && length < 100)
            {
                dumpFileName = code.Substring(startMarker + 2, length - 2).Trim();
            }
            if (string.IsNullOrWhiteSpace(dumpFileName))
            {
                dumpFileName = $"Source{i + 1}";
            }

            // file storage
            var fileName = Path.Combine(dumpFolder, $"{dumpFileName}.cs");
            File.WriteAllText(fileName, code);
        }
    }

    private string EnsureDumpFolderName()
    {
        var dumpFolder = Path.Combine("ScriptDump", AssemblyName);
        if (!Directory.Exists(dumpFolder))
        {
            Directory.CreateDirectory(dumpFolder);
        }
        dumpFolder = Path.Combine(dumpFolder, FileTool.CurrentTimeStamp());
        if (!Directory.Exists(dumpFolder))
        {
            Directory.CreateDirectory(dumpFolder);
        }
        else
        {
            var index = 1;
            while (Directory.Exists(dumpFolder))
            {
                dumpFolder += $"-{index}";
                index++;
            }
            Directory.CreateDirectory(dumpFolder);
        }
        return dumpFolder;
    }

    private static List<string> GetCompilerFailures(EmitResult compilation)
    {
        var failures = new List<string>();
        foreach (var diagnostic in compilation.Diagnostics.Where(diagnostic =>
                     diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error))
        {
            var failure = $"{diagnostic.GetMessage()}";

            // line
            var spanStart = diagnostic.Location.GetLineSpan().Span.Start;
            failure += $" [{diagnostic.Id}: Line {spanStart.Line + 1}, Column {spanStart.Character + 1}";

            // file
            var comment = GetSourceFileComment(diagnostic.Location);
            if (comment != null)
            {
                failure += $", {comment}";
            }
            failure += "]";
            failures.Add(failure);
        }
        return failures;
    }

    private static string GetSourceFileComment(Location location)
    {
        var text = location.SourceTree?.ToString();
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }
        var commentStart = text.IndexOf("/*", StringComparison.InvariantCulture);
        var commentEnd = text.IndexOf("*/", StringComparison.InvariantCulture);
        if (commentStart < 0 || commentEnd <= commentStart)
        {
            return null;
        }
        var comment = text.Substring(commentStart, commentEnd - commentStart + 2);
        if (comment.Length > 100)
        {
            comment = comment.Substring(0, 100) + "...";
        }
        return comment;
    }

    private string GetAssemblyAttributes()
    {
        var asmInfo = new StringBuilder();

        asmInfo.AppendLine("using System.Reflection;");
        if (!string.IsNullOrWhiteSpace(AssemblyInfo.Title))
        {
            asmInfo.AppendLine($"[assembly: AssemblyTitle(\"{AssemblyInfo.Title}\")]");
        }
        if (AssemblyInfo.Version != null)
        {
            asmInfo.AppendLine($"[assembly: AssemblyVersion(\"{AssemblyInfo.Version}\")]");
        }
        if (!string.IsNullOrWhiteSpace(AssemblyInfo.Product))
        {
            asmInfo.AppendLine($"[assembly: AssemblyProduct(\"{AssemblyInfo.Product}\")]");
        }

        return asmInfo.ToString();
    }
}