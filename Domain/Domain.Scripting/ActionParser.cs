using System;
using System.Collections.Generic;
using System.Linq;
using PayrollEngine.Domain.Model;
using ClientScripting = PayrollEngine.Client.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PayrollEngine.Domain.Scripting;

/// <summary>Parse c# source code for actions</summary>
public static class ActionParser
{
    /// <summary>Parse source for actions</summary>
    /// <param name="code">The source code </param>
    /// <param name="functionType">THe function types</param>
    /// <returns>List of action information</returns>
    public static List<ActionInfo> Parse(string code, FunctionType functionType = FunctionType.All)
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
            return new();
        }

        // result
        var actionInfos = new List<ActionInfo>();

        // types
        var classesSyntax = GetNamespaceClasses(root);
        foreach (var classSyntax in classesSyntax)
        {
            // provider attribute
            var actionProvider = ParseActionProvider(classSyntax);
            if (actionProvider == null)
            {
                continue;
            }

            // function type
            if (!functionType.HasFlag(actionProvider.Item2))
            {
                continue;
            }

            var typeActions = ParseActions(classSyntax, actionProvider.Item1, actionProvider.Item2);
            actionInfos.AddRange(typeActions);
        }

        return actionInfos;
    }

    private static List<ActionInfo> ParseActions(ClassDeclarationSyntax classSyntax, string namespaceName, FunctionType functionTypes)
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
                    throw new PayrollException($"Invalid action provider attribute {actionAttribute.GetText()}");
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
                List<string> categories = new();
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
                    Namespace = namespaceName,
                    Name = name,
                    Description = description,
                    Categories = categories.Any() ? categories : null,
                    Parameters = new(),
                    Issues = new()
                };

                // action parameters
                var parameterAttributes = FindAttributes(methodSyntax.AttributeLists, nameof(ClientScripting.ActionParameterAttribute));
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
                        throw new PayrollException($"Invalid action parameter attribute {parameterAttribute.GetText()}");
                    }

                    var parameterName = GetElementText(parameterArgumentNodes[0]);
                    var parameterDescription = GetElementText(parameterArgumentNodes[1]);
                    if (string.IsNullOrEmpty(parameterName) || string.IsNullOrWhiteSpace(parameterDescription))
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

                    // action info parameter
                    actionInfo.Parameters.Add(new()
                    {
                        Name = parameterName,
                        Description = parameterDescription,
                        ValueTypes = parameterValueTypes,
                        ValueSources = parameterValueSources,
                        ValueReferences = parameterValueReferences
                    });
                }

                // action issues
                var issuesAttributes = FindAttributes(methodSyntax.AttributeLists, nameof(ClientScripting.ActionIssueAttribute));
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
                        throw new PayrollException($"Invalid action issue attribute {issuesAttribute.GetText()}");
                    }

                    var issueName = GetElementText(issueArgumentNodes[0]);
                    var issueMessage = GetElementText(issueArgumentNodes[1]);
                    if (string.IsNullOrEmpty(issueName) || string.IsNullOrWhiteSpace(issueMessage))
                    {
                        continue;
                    }

                    if (!int.TryParse(GetElementText(issueArgumentNodes[2]), out var issueParameterCount))
                    {
                        throw new PayrollException($"Invalid action issue attribute {issuesAttribute.GetText()}: parameter count {issueArgumentNodes[2]}");
                    }

                    // action info issue
                    actionInfo.Issues.Add(new()
                    {
                        Name = issueName,
                        Message = issueMessage,
                        ParameterCount = issueParameterCount
                    });
                }

                actions.Add(actionInfo);
            }
        }

        return actions;
    }

    private static List<string> GetArrayParameters(SyntaxNode syntaxNode, string argumentName)
    {
        foreach (var childNode in syntaxNode.ChildNodes())
        {
            // name filter
            if (Equals(childNode.Kind(), SyntaxKind.NameColon))
            {
                var text = childNode.GetText().ToString();
                if (!text.Contains(argumentName))
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
                if (!string.IsNullOrEmpty(value))
                {
                    values.Add(value);
                }
            }
            return values;

        }
        return null;
    }

    private static string GetElementText(SyntaxNode syntaxNode)
    {
        if (syntaxNode == null)
        {
            return null;
        }
        var text = syntaxNode.GetText().ToString();
        text = text.Trim('"');
        return text.Trim();
    }

    private static string GetElementText(SyntaxToken syntaxToken)
    {
        var text = syntaxToken.Text;
        text = text.Trim('"');
        return text.Trim();
    }

    private static Tuple<string, FunctionType> ParseActionProvider(ClassDeclarationSyntax syntax)
    {
        // provider attribute
        AttributeSyntax attribute = FindAttribute(syntax.AttributeLists, nameof(ClientScripting.ActionProviderAttribute));
        if (attribute == null)
        {
            return null;
        }

        // attribute arguments
        if (attribute.ArgumentList == null || attribute.ArgumentList.ChildNodes().Count() != 2)
        {
            throw new PayrollException($"Invalid action provider attribute {attribute.GetText()}");
        }

        // namespace argument
        var namespaceArgument = attribute.ArgumentList.ChildNodes().First();
        var namespaceName = GetElementText(namespaceArgument);

        // function type
        var functionTypeArgument = attribute.ArgumentList.ChildNodes().Last();
        var typeofToken = functionTypeArgument.GetFirstToken();
        if (typeofToken.Value?.ToString() != "typeof")
        {
            throw new PayrollException($"Invalid action provider attribute {attribute.GetText()}: missing typeof()");
        }

        // skip (
        var midToken = typeofToken.GetNextToken();
        if (midToken.Value?.ToString() != "(")
        {
            throw new PayrollException($"Invalid action provider attribute {attribute.GetText()}: missing (");
        }

        // function type
        var functionToken = midToken.GetNextToken();
        if (functionToken.Value == null)
        {
            throw new PayrollException($"Invalid action provider attribute {attribute.GetText()}: missing function type");
        }
        var function = GetElementText(functionToken);
        function = function.RemoveFromEnd("Function");
        if (!Enum.TryParse<FunctionType>(function, out var functionType))
        {
            throw new PayrollException($"Invalid action provider attribute {attribute.GetText()}: unknown function type");
        }

        return new(namespaceName, functionType);
    }

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

    private static AttributeSyntax FindAttribute(SyntaxList<AttributeListSyntax> attributeLists, string attributeName) =>
        FindAttributes(attributeLists, attributeName).FirstOrDefault();

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
