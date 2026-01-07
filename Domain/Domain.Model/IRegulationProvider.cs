using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Regulation provider
/// </summary>
public interface IRegulationProvider
{
    /// <summary>
    /// Derived regulation collectors
    /// </summary>
    ILookup<string, DerivedCollector> DerivedCollectors { get; }

    /// <summary>
    /// Derived regulation wage types
    /// </summary>
    ILookup<decimal, DerivedWageType> DerivedWageTypes { get; }
}
