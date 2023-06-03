using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll user
/// </summary>
public class User : DomainObjectBase, IIdentifiableObject, IDomainAttributeObject, IEquatable<User>
{
    /// <summary>
    /// The user unique identifier
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    /// The user password
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// The password salt
    /// </summary>
    public byte[] StoredSalt { get; set; }

    /// <summary>
    /// The first name of the user
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// The last name of the user
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// The users culture
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// The users language
    /// </summary>
    public Language Language { get; set; }

    /// <summary>
    /// The user type
    /// </summary>
    public UserType UserType { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public User()
    {
    }

    /// <inheritdoc/>
    public User(User copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(User compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Identifier} {base.ToString()}";
}