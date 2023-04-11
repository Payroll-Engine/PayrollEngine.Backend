
namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Generic repository
/// </summary>
public interface IRepository
{
    /// <summary>
    /// The object type name
    /// </summary>
    string TypeName { get; }
}