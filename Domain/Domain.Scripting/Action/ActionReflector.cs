using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PayrollEngine.Domain.Model;
using PayrollEngine.Client.Scripting;

namespace PayrollEngine.Domain.Scripting.Action;

/// <summary>Reflect c# source code for actions</summary>
public static class ActionReflector
{
    /// <summary>Reflect action info</summary>
    /// <param name="code">The source code</param>
    /// <param name="functionType">The function types</param>
    /// <returns>List of action information</returns>
    public static List<ActionInfo> GetActionInfo(string code, FunctionType functionType = FunctionType.All)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(nameof(code));
        }

        // c# parser
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = syntaxTree.GetRoot() as CompilationUnitSyntax;
        if (root == null)
        {
            return [];
        }

        // result
        var actionInfos = new List<ActionInfo>();

        // types
        var classesSyntax = GetNamespaceClasses(root);
        foreach (var classSyntax in classesSyntax)
        {
            var typeActions = ReflectActions(classSyntax, functionType);
            actionInfos.AddRange(typeActions);
        }

        return actionInfos;
    }

    /// <summary>
    /// Reflect actions
    /// </summary>
    /// <param name="classSyntax">C# class syntax</param>
    /// <param name="functionTypes">Function types</param>
    /// <returns>List of action info</returns>
    private static List<ActionInfo> ReflectActions(ClassDeclarationSyntax classSyntax, FunctionType functionTypes)
    {
        var actions = new List<ActionInfo>();

        // methods
        var methodsSyntax = classSyntax.Members.OfType<MethodDeclarationSyntax>();
        foreach (var methodSyntax in methodsSyntax)
        {
            // attribute
            foreach (var functionType in functionTypes.GetFlags())
            {
                var actionName = $"{functionType}Action";
                var actionAttribute = FindAttribute(methodSyntax.AttributeLists, actionName);
                if (actionAttribute == null || actionAttribute.ArgumentList == null)
                {
                    continue;
                }

                // attribute arguments
                var actionArgumentNodes = actionAttribute.ArgumentList.ChildNodes().ToList();
                if (!actionArgumentNodes.Any())
                {
                    throw new PayrollException($"Invalid action provider attribute {actionAttribute.GetText()}.");
                }

                // name
                var name = GetElementText(actionArgumentNodes[0]);

                // description
                string description = null;
                if (actionArgumentNodes.Count > 1)
                {
                    description = GetElementText(actionArgumentNodes[1]);
                }

                // categories
                List<string> categories = [];
                var categoryIndex = 2;
                while (categoryIndex < actionArgumentNodes.Count)
                {
                    var category = GetElementText(actionArgumentNodes[categoryIndex]);
                    if (!string.IsNullOrWhiteSpace(category))
                    {
                        categories.Add(category);
                    }
                    categoryIndex++;
                }

                // action
                var actionInfo = new ActionInfo
                {
                    FunctionType = functionType,
                    Name = name,
                    Description = description,
                    Categories = categories.Any() ? categories : null,
                    Parameters = [],
                    Issues = []
                };

                // action parameters
                actionInfo.Parameters.AddRange(ReflectParameters(methodSyntax));

                // action issues
                actionInfo.Issues.AddRange(ReflectIssues(methodSyntax));

                actions.Add(actionInfo);
            }
        }

        return actions;
    }

    /// <summary>
    /// Reflect action parameters
    /// </summary>
    /// <param name="methodSyntax">C# method syntax</param>
    /// <returns>List of action parameter infos</returns>
    private static List<ActionParameterInfo> ReflectParameters(MethodDeclarationSyntax methodSyntax)
    {
        var parameters = new List<ActionParameterInfo>();
        var parameterAttributes = FindAttributes(methodSyntax.AttributeLists, nameof(ActionParameterAttribute));
        foreach (var parameterAttribute in parameterAttributes)
        {
            if (parameterAttribute.ArgumentList == null)
            {
                continue;
            }

            // parameter arguments
            var parameterArgumentNodes = parameterAttribute.ArgumentList.ChildNodes().ToList();
            if (parameterArgumentNodes.Count < 2)
            {
                throw new PayrollException($"Invalid action parameter attribute {parameterAttribute.GetText()}.");
            }

            var parameterName = GetElementText(parameterArgumentNodes[0]);
            var parameterDescription = GetElementText(parameterArgumentNodes[1]);
            if (string.IsNullOrWhiteSpace(parameterName) || string.IsNullOrWhiteSpace(parameterDescription))
            {
                continue;
            }
            List<string> parameterValueTypes = null;
            List<string> parameterValueSources = null;
            List<string> parameterValueReferences = null;
            for (var i = 2; i < parameterArgumentNodes.Count; i++)
            {
                parameterValueTypes ??= GetArrayParameters(parameterArgumentNodes[i], "valueTypes");
                parameterValueSources ??= GetArrayParameters(parameterArgumentNodes[i], "valueSources");
                parameterValueReferences ??= GetArrayParameters(parameterArgumentNodes[i], "valueReferences");
            }

            // action parameter info
            parameters.Add(new()
            {
                Name = parameterName,
                Description = parameterDescription,
                ValueTypes = parameterValueTypes,
                ValueSources = parameterValueSources,
                ValueReferences = parameterValueReferences
            });
        }

        return parameters;
    }

    /// <summary>
    /// Reflect action issues
    /// </summary>
    /// <param name="methodSyntax">C# method syntax</param>
    /// <returns>List of action issue infos</returns>
    private static List<ActionIssueInfo> ReflectIssues(MethodDeclarationSyntax methodSyntax)
    {
        var issues = new List<ActionIssueInfo>();
        var issuesAttributes = FindAttributes(methodSyntax.AttributeLists, nameof(ActionIssueAttribute));
        foreach (var issuesAttribute in issuesAttributes)
        {
            if (issuesAttribute.ArgumentList == null)
            {
                continue;
            }

            // issue arguments
            var issueArgumentNodes = issuesAttribute.ArgumentList.ChildNodes().ToList();
            if (issueArgumentNodes.Count != 3)
            {
                throw new PayrollException($"Invalid action issue attribute {issuesAttribute.GetText()}.");
            }

            var issueName = GetElementText(issueArgumentNodes[0]);
            var issueMessage = GetElementText(issueArgumentNodes[1]);
            if (string.IsNullOrWhiteSpace(issueName) || string.IsNullOrWhiteSpace(issueMessage))
            {
                continue;
            }

            if (!int.TryParse(GetElementText(issueArgumentNodes[2]), out var issueParameterCount))
            {
                throw new PayrollException($"Invalid action issue attribute {issuesAttribute.GetText()}: parameter count {issueArgumentNodes[2]}.");
            }

            // action info issue
            issues.Add(new()
            {
                Name = issueName,
                Message = issueMessage,
                ParameterCount = issueParameterCount
            });
        }
        return issues;
    }

    /// <summary>
    /// Get method array parameter
    /// </summary>
    /// <param name="syntaxNode">C# syntax node</param>
    /// <param name="argumentName">Method argument name</param>
    private static List<string> GetArrayParameters(SyntaxNode syntaxNode, string argumentName)
    {
        foreach (var childNode in syntaxNode.ChildNodes())
        {
            // name filter
            if (Equals(childNode.Kind(), SyntaxKind.NameColon))
            {
                var text = childNode.GetText().ToString();
                if (string.IsNullOrWhiteSpace(text) || !text.Contains(argumentName))
                {
                    return null;
                }
                continue;
            }

            // array filter
            var arrayNode = childNode.ChildNodes().FirstOrDefault();
            if (arrayNode == null || !Equals(arrayNode.Kind(), SyntaxKind.ArrayInitializerExpression))
            {
                return null;
            }

            // array values
            var arrayValueNodes = arrayNode.ChildNodes().ToList();
            var values = new List<string>();
            foreach (var arrayValueNode in arrayValueNodes)
            {
                var value = arrayValueNode.GetText().ToString().Trim();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add(value);
                }
            }
            return values;

        }
        return null;
    }

    /// <summary>
    /// Get C# element text
    /// </summary>
    /// <param name="syntaxNode">C# syntax node</param>
    private static string GetElementText(SyntaxNode syntaxNode)
    {
        if (syntaxNode == null)
        {
            return null;
        }
        var text = syntaxNode.GetText().ToString().Trim('"').Trim();
        return text;
    }

    /// <summary>
    /// Get namespace classes
    /// </summary>
    /// <param name="syntax">C# compilation syntax</param>
    private static List<ClassDeclarationSyntax> GetNamespaceClasses(CompilationUnitSyntax syntax)
    {
        var types = new List<ClassDeclarationSyntax>();
        foreach (var member in syntax.Members)
        {
            // namespace with braces
            if (member is NamespaceDeclarationSyntax namespaceSyntax)
            {
                types.AddRange(namespaceSyntax.Members.OfType<ClassDeclarationSyntax>());
            }
            // file namespace
            else if (member is FileScopedNamespaceDeclarationSyntax fileNamespaceSyntax)
            {
                types.AddRange(fileNamespaceSyntax.Members.OfType<ClassDeclarationSyntax>());
            }
            // class
            else if (member is ClassDeclarationSyntax classSyntax)
            {
                types.Add(classSyntax);
            }
        }
        return types;
    }

    /// <summary>
    /// Find action attribute
    /// </summary>
    /// <param name="attributeLists">Available attributes</param>
    /// <param name="attributeName">Attribute to find</param>
    private static AttributeSyntax FindAttribute(SyntaxList<AttributeListSyntax> attributeLists, string attributeName) =>
        FindAttributes(attributeLists, attributeName).FirstOrDefault();

    /// <summary>
    /// Find action attributes
    /// </summary>
    /// <param name="attributeLists">Available attributes</param>
    /// <param name="attributeName">Attribute to find</param>
    private static List<AttributeSyntax> FindAttributes(SyntaxList<AttributeListSyntax> attributeLists, string attributeName)
    {
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }

        var attributes = new List<AttributeSyntax>();

        // attribute
        var attributeFullName = attributeName.EnsureEnd(nameof(Attribute));
        var attributeCompactName = attributeName.RemoveFromEnd(nameof(Attribute));
        foreach (var attributeList in attributeLists)
        {
            var attribute = attributeList.Attributes.FirstOrDefault(x =>
                string.Equals(x.Name.ToString(), attributeFullName) ||
                string.Equals(x.Name.ToString(), attributeCompactName));
            if (attribute != null)
            {
                attributes.Add(attribute);
            }
        }
        return attributes;
    }
}
