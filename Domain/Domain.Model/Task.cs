using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A task
/// </summary>
public class Task : DomainObjectBase, IDomainAttributeObject, IEquatable<Task>
{
    /// <summary>
    /// The task name (immutable)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized task names (immutable)
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The task category
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// The task instruction
    /// </summary>
    public string Instruction { get; set; }

    /// <summary>
    /// The scheduled user id
    /// </summary>
    public int ScheduledUserId { get; set; }

    /// <summary>
    /// The task schedule date
    /// </summary>
    public DateTime Scheduled { get; set; }

    /// <summary>
    /// The completed user id
    /// </summary>
    public int? CompletedUserId { get; set; }

    /// <summary>
    /// The task completed date
    /// </summary>
    public DateTime? Completed { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public Task()
    {
    }

    /// <inheritdoc/>
    public Task(Task copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(Task compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name}: {Scheduled} {base.ToString()}";
}