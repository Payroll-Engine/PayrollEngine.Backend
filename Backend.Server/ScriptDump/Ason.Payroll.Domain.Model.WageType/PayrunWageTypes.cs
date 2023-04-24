/* PayrunWageTypes */
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec Payrun Wage Types</summary>
public class PayrunWageTypes : WageTypes
{
    /// <summary>Function constructor</summary>
    public PayrunWageTypes(PayrunFunction function) :
        base(function)
    {
    }
}
