// ReSharper disable UnusedMemberInSuper.Global
namespace PayrollEngine.Domain.Model;

/// <summary>
/// Represents a derivable object
/// </summary>
public interface IDerivableObject
{
    /// <summary>
    /// The override type
    /// </summary>
    OverrideType OverrideType { get; set; }
}