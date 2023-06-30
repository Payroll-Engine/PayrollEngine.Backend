using System;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A log
/// </summary>
public class Log : DomainObjectBase, IEquatable<Log>
{
    /// <summary>
    /// The log level (immutable)
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    /// The log message name (immutable)
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// The log user (immutable)
    /// </summary>
    public string User { get; set; }

    /// <summary>
    /// The log error (immutable)
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// The log comment (immutable)
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// The log owner (immutable)
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    /// The log owner type (immutable)
    /// </summary>
    public string OwnerType { get; set; }

    /// <inheritdoc/>
    public Log()
    {
    }

    /// <inheritdoc/>
    public Log(Log copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(Log compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Level}: {Message} {base.ToString()}";
}