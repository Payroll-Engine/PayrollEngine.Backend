using System;

namespace PayrollEngine.Domain.Model;

/// <summary>Reference to regulation</summary>
public class RegulationReference
{
    /// <summary>the regulation name</summary>
    /// <value>The name.</value>
    private string Name { get; }

    /// <summary>Gets the regulation version</summary>
    /// <value>The version.</value>
    private int Version { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegulationReference"/> class.
    /// </summary>
    /// <param name="reference">The reference string</param>
    public RegulationReference(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new ArgumentException(nameof(reference));
        }

        var versionToken = reference.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);
        if (versionToken > 0)
        {
            var versionText = reference.Substring(versionToken + 1);
            if (!int.TryParse(versionText, out var version))
            {
                throw new ArgumentException($"Invalid regulation reference {reference}");
            }
            Name = reference.Substring(0, versionToken);
            Version = version;
        }
        else
        {
            // reference without version restriction
            Name = reference;
            Version = 0;
        }
    }

    /// <summary>Initializes a new instance of the <see cref="RegulationReference"/> class</summary>
    /// <param name="name">The regulation name</param>
    /// <param name="version">The regulation version</param>
    public RegulationReference(string name, int version = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }
        Name = name;
        Version = version;
    }

    /// <summary>Determines whether the specified regulation is matching this reference</summary>
    /// <param name="name">The regulation name</param>
    /// <param name="version">The regulation version</param>
    /// <returns>True if the regulation reference matches the test regulation</returns>
    public bool IsMatching(string name, int version)
    {
        // name
        if (!string.Equals(Name, name))
        {
            return false;
        }

        // version
        if (Version <= 0)
        {
            // no version restriction
            return true;
        }
        // equal or newer version
        return version >= Version;
    }
}