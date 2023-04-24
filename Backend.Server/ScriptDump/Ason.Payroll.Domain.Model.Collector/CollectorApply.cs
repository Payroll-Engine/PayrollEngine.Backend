/* CollectorApply */
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec collector apply</summary>
public class CollectorApply : Collector<
    PayrollNational,
    PayrollCompany<PayrollNational>,
    PayrunEmployee<PayrollNational, PayrollCompany<PayrollNational>>,
    CollectorCollectors<PayrollNational,
        PayrollCompany<PayrollNational>,
        PayrunEmployee<PayrollNational, PayrollCompany<PayrollNational>>>,
    PayrunWageTypes>
{
    /// <summary>Function constructor</summary>
    public CollectorApply(CollectorApplyFunction function) :
        base(function)
    {
    }
}
