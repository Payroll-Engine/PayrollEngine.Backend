// ReSharper disable UnusedMemberInSuper.Global
namespace PayrollEngine.Api.Model;

/// <summary>
/// Represents a culture query
/// </summary>
public interface ICultureQuery
{
    /// <summary>
    /// The culture name based on RFC 4646
    /// </summary>
    string Culture { get; set; }
}