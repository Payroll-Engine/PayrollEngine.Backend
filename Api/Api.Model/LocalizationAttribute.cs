using System;

namespace PayrollEngine.Api.Model;

/// <summary>Localization attribute</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class LocalizationAttribute : Attribute
{
    /// <summary>Property name of the translation source</summary>
    public string Source { get; }

    /// <summary>Initializes a new instance of the <see cref="LocalizationAttribute"/> class</summary>
    /// <param name="source">The source property name</param>
    public LocalizationAttribute(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException(nameof(source));
        }

        Source = source;
    }
}