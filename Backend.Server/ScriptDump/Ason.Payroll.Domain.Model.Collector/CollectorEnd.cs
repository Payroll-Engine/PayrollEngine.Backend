/* CollectorEnd */
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec collector end</summary>
public class CollectorEnd : Collector<
    PayrollNational,
    PayrollCompany<PayrollNational>,
    PayrunEmployee<PayrollNational, PayrollCompany<PayrollNational>>,
    CollectorCollectors<PayrollNational, PayrollCompany<PayrollNational>,
        PayrunEmployee<PayrollNational, PayrollCompany<PayrollNational>>>,
    PayrunWageTypes>
{
    /// <summary>Function constructor</summary>
    public CollectorEnd(CollectorEndFunction function) :
        base(function)
    {
    }
}
