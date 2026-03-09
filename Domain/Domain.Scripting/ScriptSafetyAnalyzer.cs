using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Analyzes compiled script code for usage of banned .NET APIs.
/// Operates on the Roslyn semantic model to catch aliased and fully-qualified references.
/// </summary>
/// <remarks>
/// <para>
/// This is a defense-in-depth measure. It raises the bar significantly but cannot
/// prevent all bypass techniques (e.g. <c>Type.GetType</c> with string literals,
/// or <c>dynamic</c> dispatch). A process-level sandbox is needed for full isolation.
/// </para>
/// <para>
/// Performance: uses a two-stage filter to minimize expensive <c>GetSymbolInfo</c> calls.
/// Stage 1 skips entire syntax trees whose source text contains no banned namespace fragments.
/// Stage 2 only resolves symbols on node types that can reference banned APIs
/// (member access, object creation, typeof, using directives).
/// Typical overhead: &lt;10 ms per compilation for clean scripts.
/// </para>
/// </remarks>
internal static class ScriptSafetyAnalyzer
{
    /// <summary>
    /// Namespace prefixes that are blocked in user scripts.
    /// Any symbol whose containing namespace starts with one of these is rejected.
    /// </summary>
    private static readonly ImmutableArray<string> BannedNamespacePrefixes =
    [
        "System.IO",
        "System.Net",
        "System.Diagnostics",
        // do not ban the System.Reflection namespace, required for attributes
        //"System.Reflection",
        "System.Runtime.InteropServices",
        "System.Runtime.Loader",
        "System.Security.Cryptography",
        "System.Threading.Tasks.Dataflow",
        "Microsoft.Win32"
    ];

    /// <summary>
    /// Specific types that are banned even if their namespace is not fully blocked.
    /// </summary>
    private static readonly ImmutableHashSet<string> BannedTypeNames = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "System.Environment",
        "System.AppDomain",
        "System.Activator",
        "System.GC",
        "System.Runtime.CompilerServices.Unsafe"
    );

    /// <summary>
    /// Specific method names on <c>System.Type</c> that enable reflection-based bypass.
    /// </summary>
    private static readonly ImmutableHashSet<string> BannedTypeMethods = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "GetType",       // Type.GetType("System.IO.File")
        "InvokeMember"
    );

    /// <summary>
    /// Substring fragments used for the fast tree-level pre-filter (stage 1).
    /// If a syntax tree's source text does not contain any of these substrings,
    /// the entire tree is skipped without semantic analysis.
    /// </summary>
    /// <remarks>
    /// Includes namespace fragments, banned type short names, and banned method names.
    /// False positives (e.g. substring in a comment or variable name) only cause
    /// the tree to proceed to the more precise semantic check — no correctness impact.
    /// </remarks>
    private static readonly ImmutableArray<string> SuspiciousTextFragments =
    [
        // namespace fragments
        "System.IO",
        "System.Net",
        "System.Diagnostics",
        "System.Reflection",
        "InteropServices",
        "System.Runtime.Loader",
        "Cryptography",
        "Dataflow",
        "Microsoft.Win32",
        // banned type short names (catch using-aliased access)
        "Environment",
        "AppDomain",
        "Activator",
        // banned methods
        "GetType",
        "InvokeMember"
    ];

    /// <summary>
    /// Validates all syntax trees in a compilation against the banned API list.
    /// </summary>
    /// <param name="compilation">The Roslyn compilation (before or after Emit).</param>
    /// <param name="userTreeCount">
    /// Number of syntax trees at the end of <paramref name="compilation"/>.Trees
    /// that originate from user code. System/embedded trees at the beginning are skipped.
    /// Pass 0 to check all trees.
    /// </param>
    /// <returns>List of human-readable violation messages. Empty if clean.</returns>
    internal static List<string> Analyze(CSharpCompilation compilation, int userTreeCount = 0)
    {
        var violations = new List<string>();
        var trees = compilation.SyntaxTrees;

        // determine which trees to scan
        var treesToScan = userTreeCount > 0 && userTreeCount < trees.Length
            ? trees.Skip(trees.Length - userTreeCount)
            : trees.AsEnumerable();

        foreach (var tree in treesToScan)
        {
            // --- stage 1: fast text pre-filter ---
            // skip entire tree if source text contains no suspicious fragments
            var sourceText = tree.ToString();
            if (!ContainsSuspiciousText(sourceText))
            {
                continue;
            }

            // --- stage 2: targeted semantic analysis ---
            var model = compilation.GetSemanticModel(tree);
            var walker = new BannedApiWalker(model, violations);
            walker.Visit(tree.GetRoot());
        }

        // deduplicate (same symbol may be visited via different node types)
        return violations.Distinct(StringComparer.Ordinal).ToList();
    }

    /// <summary>
    /// Fast check whether source text contains any substring that could indicate
    /// usage of a banned API. O(n) string scan, no allocations.
    /// </summary>
    private static bool ContainsSuspiciousText(string source)
    {
        foreach (var fragment in SuspiciousTextFragments)
        {
            if (source.Contains(fragment, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Syntax walker that only visits node types relevant for banned API detection.
    /// Avoids visiting the majority of syntax nodes (literals, local declarations,
    /// control flow, etc.) which cannot reference banned APIs.
    /// </summary>
    private sealed class BannedApiWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel model;
        private readonly List<string> violations;

        internal BannedApiWalker(SemanticModel model, List<string> violations)
        {
            this.model = model;
            this.violations = violations;
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            // using directives: syntax-only check, no semantic model needed
            var nameText = node.Name?.ToString();
            if (nameText != null && IsBannedNamespace(nameText))
            {
                var line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                violations.Add(
                    $"Banned namespace '{nameText}' in using directive (line {line}). " +
                    $"Scripts may not reference {nameText} for security reasons.");
            }
            // don't recurse into using children
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            CheckSymbol(node);
            // continue walking into nested member access (e.g. System.IO.File.ReadAllText)
            base.VisitMemberAccessExpression(node);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            CheckSymbol(node);
            base.VisitObjectCreationExpression(node);
        }

        public override void VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            // typeof(System.IO.File) — check the type argument
            var typeInfo = model.GetTypeInfo(node.Type);
            if (typeInfo.Type is INamedTypeSymbol namedType)
            {
                CheckNamedType(namedType, node);
            }
            base.VisitTypeOfExpression(node);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            // skip identifiers that are part of member access or object creation
            // (parent node handler already checks them)
            if (node.Parent is MemberAccessExpressionSyntax ||
                node.Parent is ObjectCreationExpressionSyntax ||
                node.Parent is UsingDirectiveSyntax ||
                node.Parent is QualifiedNameSyntax)
            {
                return;
            }

            // standalone identifier — could be a using-imported banned type
            // (e.g. "Environment" after "using System;")
            CheckSymbol(node);
        }

        private void CheckSymbol(SyntaxNode node)
        {
            var symbolInfo = model.GetSymbolInfo(node);
            var symbol = symbolInfo.Symbol;
            if (symbol == null)
            {
                return;
            }

            // resolve the containing type
            var containingType = symbol as INamedTypeSymbol ?? symbol.ContainingType;
            if (containingType != null)
            {
                CheckNamedType(containingType, node);
            }

            // check banned methods on System.Type (reflection bypass)
            if (symbol is IMethodSymbol method &&
                method.ContainingType?.ToDisplayString() == "System.Type" &&
                BannedTypeMethods.Contains(method.Name))
            {
                var line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                violations.Add(
                    $"Banned method 'Type.{method.Name}' at line {line}. " +
                    $"Scripts may not use reflection methods for security reasons.");
            }
        }

        private void CheckNamedType(INamedTypeSymbol type, SyntaxNode node)
        {
            var fullTypeName = type.ToDisplayString();

            // check banned types
            if (BannedTypeNames.Contains(fullTypeName))
            {
                var line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                violations.Add(
                    $"Banned type '{fullTypeName}' at line {line}. " +
                    $"Scripts may not use {fullTypeName} for security reasons.");
                return;
            }

            // check banned namespaces
            var ns = type.ContainingNamespace?.ToDisplayString();
            if (ns != null && IsBannedNamespace(ns))
            {
                var line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                violations.Add(
                    $"Banned API from namespace '{ns}' at line {line}. " +
                    $"Scripts may not access {ns} for security reasons.");
            }
        }
    }

    private static bool IsBannedNamespace(string ns) =>
        BannedNamespacePrefixes.Any(banned =>
            ns.Equals(banned, StringComparison.Ordinal) ||
            ns.StartsWith(banned + ".", StringComparison.Ordinal));
}
