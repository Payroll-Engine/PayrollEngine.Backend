
namespace PayrollEngine.Domain.Model;

/// <summary>
/// Represents an named object
/// </summary>
public interface INamedObject
{
    /// <summary>
    /// The object name
    /// </summary>
    string Name { get; set; }
}