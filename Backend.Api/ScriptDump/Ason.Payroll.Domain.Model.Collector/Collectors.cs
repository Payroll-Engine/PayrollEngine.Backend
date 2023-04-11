/* Collectors */
using System;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec collectors</summary>
public class Collectors
{
    /// <summary>The function</summary>
    public Function Function { get; }

    /// <summary>Function constructor</summary>
    public Collectors(Function function)
    {
        Function = function ?? throw new ArgumentNullException(nameof(function));
        Groups = new(function);
    }

    /// <summary>Swissdec collector groups</summary>
    public CollectorGroups Groups { get; }
}

/// <summary>Swissdec collector groups</summary>
public class CollectorGroups
{
    /// <summary>The function</summary>
    public Function Function { get; }

    /// <summary>Function constructor</summary>
    public CollectorGroups(Function function)
    {
        Function = function ?? throw new ArgumentNullException(nameof(function));
    }
}
