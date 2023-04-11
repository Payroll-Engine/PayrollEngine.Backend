/* PayrunCollectors */
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec Payrun Collectors</summary>
public class PayrunCollectors : Collectors
{
    /// <summary>Function constructor</summary>
    public PayrunCollectors(PayrunFunction function) :
        base(function)
    {
    }
}
