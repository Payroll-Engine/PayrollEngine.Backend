/* WageTypeValue */
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec wage type value</summary>
public class WageTypeValue : WageType<
    PayrollNational,
    PayrollCompany<PayrollNational>,
    PayrunEmployee<PayrollNational, PayrollCompany<PayrollNational>>,
    WageTypeCollectors,
    WageTypeWageTypes<PayrollNational, PayrollCompany<PayrollNational>,
        PayrunEmployee<PayrollNational, PayrollCompany<PayrollNational>>>>
{
    /// <summary>Function constructor</summary>
    public WageTypeValue(WageTypeValueFunction function) :
        base(function)
    {
    }
}
