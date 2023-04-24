/* Payrun */
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec payrun</summary>
public abstract class Payrun<TNational, TCompany, TEmployee, TCollectors, TWageTypes> :
    Payroll<TNational, TCompany, TEmployee, TCollectors, TWageTypes>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
    where TCollectors : PayrunCollectors
    where TWageTypes : PayrunWageTypes
{
    /// <summary>Function constructor</summary>
    protected Payrun(PayrunFunction function) :
        base(function)
    {
    }

    /// <summary>The function</summary>
    public new PayrunFunction Function => base.Function as PayrunFunction;

    private PayrunEmployee<TNational, TCompany> employee;
    /// <summary>The Swissdec payroll employee</summary>
    public override TEmployee Employee => (TEmployee)(employee ??= new(Function, National, Company));

    private PayrunCollectors collectors;
    /// <summary>The Swissdec collectors</summary>
    public override TCollectors Collectors => (TCollectors)(collectors ??= new(Function));

    private PayrunWageTypes wageTypes;
    /// <summary>The Swissdec wage type</summary>
    public override TWageTypes WageTypes => (TWageTypes)(wageTypes ??= new(Function));
}
