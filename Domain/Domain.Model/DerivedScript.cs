// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Domain.Model;

/// <summary>Derived script</summary>
public sealed class DerivedScript : Script
{
    /// <summary>Default constructor</summary>
    public DerivedScript()
    {
    }

    /// <summary>Base class constructor</summary>
    public DerivedScript(Script script) :
        base(script)
    {
    }

    /// <summary>The regulation id</summary>
    public int RegulationId { get; init; }

    /// <summary>The layer level</summary>
    public int Level { get; init; }

    /// <summary>The layer priority</summary>
    public int Priority { get; init; }
}