// ReSharper disable UnusedMemberInSuper.Global
namespace PayrollEngine.Domain.Model;

/// <summary>
/// Represents a named object
/// </summary>
public interface INamedObject
{
    /// <summary>
    /// The object name
    /// </summary>
    string Name { get; set; }
}