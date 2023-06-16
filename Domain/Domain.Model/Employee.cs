using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll service employee.
/// </summary>
public class Employee : DomainObjectBase, IIdentifiableObject, IDomainAttributeObject, IEquatable<Employee>
{
    /// <summary>
    /// The employee unique identifier
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    /// The first name of the user
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// The last name of the user
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// The employees language
    /// </summary>
    public Language Language { get; set; }

    /// <summary>
    /// Employee division names
    /// </summary>
    public List<string> Divisions { get; set; }

    /// <summary>
    /// The employee culture, fallback is the division culture
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// The employee calendar, fallback is the division calendar
    /// </summary>
    public string Calendar { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public Employee()
    {
    }

    /// <inheritdoc/>
    public Employee(Employee copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(Employee compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Identifier} {base.ToString()}";
}