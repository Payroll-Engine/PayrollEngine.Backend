/* WageTypeCollectors */
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec Wage Type Collector</summary>
public class WageTypeCollectors : PayrunCollectors
{
    /// <summary>Function constructor</summary>
    public WageTypeCollectors(WageTypeFunction function) :
        base(function)
    {
    }
}
