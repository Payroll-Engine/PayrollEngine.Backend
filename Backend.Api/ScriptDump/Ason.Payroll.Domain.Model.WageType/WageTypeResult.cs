/* WageTypeResult */
using Ason.Payroll.Client.Scripting.Function;
using System.Collections.Generic;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec wage type result</summary>
public class WageTypeResult : WageType<
    PayrollNational,
    PayrollCompany<PayrollNational>,
    PayrunEmployee<PayrollNational, PayrollCompany<PayrollNational>>,
    WageTypeCollectors,
    WageTypeWageTypes<
        PayrollNational,
        PayrollCompany<
            PayrollNational>,
        PayrunEmployee<
            PayrollNational,
            PayrollCompany<PayrollNational>>>>
{
    /// <summary>Function constructor</summary>
    public WageTypeResult(WageTypeResultFunction function) :
        base(function)
    {
    }

    /// <summary></summary>
    protected new WageTypeResultFunction Function => base.Function as WageTypeResultFunction;

    /// <summary>Get wage value and set result attributes</summary>
    /// <remarks>Sets attribute "Report" value</remarks>
    public virtual void SetResultAttributes()
    {
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Subtotal, Function.WageTypeValue);
    }
}
