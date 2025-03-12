using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extension methods for <see cref="string"/>
/// </summary>
public static class StringExtensions
{

    #region Modification

    /// <summary>Ensures a start prefix</summary>
    /// <param name="source">The source value</param>
    /// <param name="prefix">The prefix to add</param>
    /// <returns>The string with prefix</returns>
    public static string EnsureStart(this string source, string prefix)
    {
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                source = prefix;
            }
            else if (!source.StartsWith(prefix))
            {
                source = $"{prefix}{source}";
            }
        }
        return source;
    }

    /// <summary>Ensures a start prefix</summary>
    /// <param name="source">The source value</param>
    /// <param name="prefix">The prefix to add</param>
    /// <param name="comparison">The comparison culture</param>
    /// <returns>The string with prefix</returns>
    public static string EnsureStart(this string source, string prefix, StringComparison comparison)
    {
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                source = prefix;
            }
            else if (!source.StartsWith(prefix, comparison))
            {
                source = $"{prefix}{source}";
            }
        }
        return source;
    }

    /// <summary>Ensures an ending suffix</summary>
    /// <param name="source">The source value</param>
    /// <param name="suffix">The suffix to add</param>
    /// <returns>The string with suffix</returns>
    public static string EnsureEnd(this string source, string suffix)
    {
        if (!string.IsNullOrWhiteSpace(suffix))
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                source = suffix;
            }
            else if (!source.EndsWith(suffix))
            {
                source = $"{source}{suffix}";
            }
        }
        return source;
    }

    /// <summary>Ensures an ending suffix</summary>
    /// <param name="source">The source value</param>
    /// <param name="suffix">The suffix to add</param>
    /// <param name="comparison">The comparison culture</param>
    /// <returns>The string with suffix</returns>
    public static string EnsureEnd(this string source, string suffix, StringComparison comparison)
    {
        if (!string.IsNullOrWhiteSpace(suffix))
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                source = suffix;
            }
            else if (!source.EndsWith(suffix, comparison))
            {
                source = $"{source}{suffix}";
            }
        }
        return source;
    }

    /// <summary>Remove prefix from string</summary>
    /// <param name="source">The source value</param>
    /// <param name="prefix">The prefix to remove</param>
    /// <returns>The string without suffix</returns>
    public static string RemoveFromStart(this string source, string prefix)
    {
        if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(prefix) &&
            source.StartsWith(prefix))
        {
            source = source.Substring(prefix.Length);
        }
        return source;
    }

    /// <summary>Remove prefix from string</summary>
    /// <param name="source">The source value</param>
    /// <param name="prefix">The prefix to remove</param>
    /// <param name="comparison">The comparison culture</param>
    /// <returns>The string without the starting prefix</returns>
    public static string RemoveFromStart(this string source, string prefix, StringComparison comparison)
    {
        if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(prefix) &&
            source.StartsWith(prefix, comparison))
        {
            source = source.Substring(prefix.Length);
        }
        return source;
    }

    /// <summary>Remove suffix from string</summary>
    /// <param name="source">The source value</param>
    /// <param name="suffix">The suffix to remove</param>
    /// <returns>The string without the ending suffix</returns>
    public static string RemoveFromEnd(this string source, string suffix)
    {
        if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(suffix) &&
            source.EndsWith(suffix))
        {
            source = source.Substring(0, source.Length - suffix.Length);
        }
        return source;
    }

    /// <summary>Remove suffix from string</summary>
    /// <param name="source">The source value</param>
    /// <param name="suffix">The suffix to remove</param>
    /// <param name="comparison">The comparison culture</param>
    /// <returns>The string without the ending suffix</returns>
    public static string RemoveFromEnd(this string source, string suffix, StringComparison comparison)
    {
        if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(suffix) &&
            source.EndsWith(suffix, comparison))
        {
            source = source.Substring(0, source.Length - suffix.Length);
        }
        return source;
    }

    #endregion

    #region Csv

    /// <summary>Test for a cSV token</summary>
    /// <param name="source">The source value</param>
    /// <param name="token">The token to search</param>
    /// <param name="separator">The token separator</param>
    /// <returns>True if the token is available</returns>
    public static bool ContainsCsvToken(this string source, string token, char separator = ',')
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException(nameof(token));
        }
        if (string.IsNullOrWhiteSpace(source))
        {
            return false;
        }
        return source.Split(separator, StringSplitOptions.RemoveEmptyEntries).Contains(token);
    }

    #endregion

    #region Scripting

    /// <summary>
    /// Extract the code tags from script.
    /// Script tag example:
    ///   /* #sealed */ MyScriptExpression
    /// </summary>
    /// <param name="script">The script</param>
    /// <returns>A list of code tags</returns>
    private static IEnumerable<string> GetScriptTags(this string script)
    {
        // tags comment start
        var tagsStartIndex = script.IndexOf(ScriptingSpecification.TagsStartMarker, 0, StringComparison.InvariantCulture);
        if (tagsStartIndex < 0)
        {
            return null;
        }

        // tags commend end
        var tagsEndIndex = script.IndexOf(ScriptingSpecification.TagsEndMarker, tagsStartIndex, StringComparison.InvariantCulture);
        if (tagsEndIndex < 0)
        {
            return null;
        }

        // split tags into tokens
        tagsStartIndex += ScriptingSpecification.TagsStartMarker.Length;
        var tokens = script.Substring(tagsStartIndex, tagsEndIndex - tagsStartIndex - 1)
            .Split(ScriptingSpecification.TagsSeparator, StringSplitOptions.RemoveEmptyEntries);

        // validate and collect tags
        var tags = new List<string>();
        foreach (var token in tokens)
        {
            var tag = token.Trim();
            if (ScriptingSpecification.IsTag(tag))
            {
                tags.Add(tag);
            }
        }
        return tags;
    }

    /// <summary>
    /// Test if string is a sealed script
    /// </summary>
    /// <param name="script">The script</param>
    /// <returns>True if the script is sealed</returns>
    public static bool IsSealedScript(this string script) =>
        GetScriptTags(script)?.Contains(ScriptingSpecification.SealedTag) ?? false;

    #endregion

}