//#define COMPILER_PERFORMANCE
#if COMPILER_PERFORMANCE
#define LOG_STOPWATCH
#endif
//#define DUMP_COMPILER_SOURCES

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Build .net assembly from C# code
/// </summary>
internal sealed class CSharpCompiler
{
    private string AssemblyName { get; }
    private AssemblyInfo AssemblyInfo { get; }

    internal CSharpCompiler(string assemblyName) :
        this(assemblyName, null)
    {
    }

    private CSharpCompiler(string assemblyName, AssemblyInfo assemblyInfo)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            throw new ArgumentException(nameof(assemblyName));
        }

        AssemblyName = assemblyName;
        AssemblyInfo = assemblyInfo;
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
    {
        // any object
        typeof(object),
        // used for object references
        typeof(AssemblyTargetedPatchBandAttribute),
        // used for dynamic objects
        typeof(DynamicAttribute),
        // used for linq
        typeof(Enumerable)
    };

    private static readonly string[] DefaultReferenceAssemblies =
    {
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
    };

    /// <summary>
    /// List of additional assembly references that are added to the
    /// compiler parameters in order to execute the script code.
    /// </summary>
    private static readonly HashSet<MetadataReference> DefaultReferences = new();

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
                    throw new PayrollException($"Invalid c# language version: {ScriptingSpecification.CSharpLanguageVersion}");
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
    {
        "System",
        "System.Text"
    };

    /// <summary>
    /// List of additional namespaces to add to the script
    /// </summary>
    private static readonly HashSet<string> DefaultNamespaces = new();

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
    /// <param name="codes">The C# source codes</param>
    /// <returns>The compile result</returns>
    internal ScriptCompileResult CompileAssembly(IList<string> codes)
    {
        if (codes == null)
        {
            throw new ArgumentNullException(nameof(codes));
        }

#if DUMP_COMPILER_SOURCES
            // target folder: ScriptDump\AssemblyName\
            var dumpFolder = Path.Combine("ScriptDump", AssemblyName);
            if (!Directory.Exists(dumpFolder))
            {
                Directory.CreateDirectory(dumpFolder);
            }

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
                File.WriteAllText($"{dumpFolder}\\{dumpFileName}.cs", code);
            }
#endif

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

        // references
        var allReferences = new List<MetadataReference>(DefaultReferences);

        // create bits
        using var peStream = new MemoryStream();
        var compilation = CSharpCompilation.Create(AssemblyName,
                syntaxTrees,
                allReferences,
                new(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default))
            .Emit(peStream);

        // error handling
        if (!compilation.Success)
        {
            var failures = compilation.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                .Select(x => x.ToString())
                .ToList();
            throw new ScriptCompileException(failures);
        }

        // build the assembly
        var script = new StringBuilder();
        foreach (var code in codes)
        {
            script.AppendLine($"// {new('-', 80)}");
            script.AppendLine(code);
        }

        peStream.Seek(0, SeekOrigin.Begin);
        var result = new ScriptCompileResult(script.ToString(), peStream.ToArray());

        LogStopwatch.Stop(nameof(CompileAssembly));

        return result;
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