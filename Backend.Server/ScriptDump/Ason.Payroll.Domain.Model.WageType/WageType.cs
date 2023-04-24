/* WageType */
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec wage type</summary>
public abstract class WageType<TNational, TCompany, TEmployee, TCollectors, TWageTypes> :
    Payrun<TNational, TCompany, TEmployee, TCollectors, TWageTypes>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
    where TCollectors : WageTypeCollectors
    where TWageTypes : WageTypeWageTypes<TNational, TCompany, TEmployee>
{
    /// <summary>Function constructor</summary>
    protected WageType(WageTypeFunction function) :
        base(function)
    {
    }

    /// <summary>The function</summary>
    public new WageTypeFunction Function => base.Function as WageTypeFunction;

    private WageTypeCollectors collectors;
    /// <summary>The Swissdec wage type collectors</summary>
    public override TCollectors Collectors => (TCollectors)(collectors ??= new(Function));

    private WageTypeWageTypes<TNational, TCompany, TEmployee> wageTypes;
    /// <summary>The Swissdec wage type wage types</summary>
    public override TWageTypes WageTypes => (TWageTypes)(wageTypes ??= new(Function, Employee));
}
