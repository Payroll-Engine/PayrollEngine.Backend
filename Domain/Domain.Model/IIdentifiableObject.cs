
namespace PayrollEngine.Domain.Model;

/// <summary>
/// Represents an identifiable object
/// </summary>
public interface IIdentifiableObject
{
    /// <summary>
    /// The object identifier
    /// </summary>
    string Identifier { get; set; }
}