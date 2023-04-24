/* Collector */
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec collector</summary>
public abstract class Collector<TNational, TCompany, TEmployee, TCollectors, TWageTypes> :
    Payrun<TNational, TCompany, TEmployee, TCollectors, TWageTypes>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
    where TCollectors : CollectorCollectors<TNational, TCompany, TEmployee>
    where TWageTypes : PayrunWageTypes
{
    /// <summary>Function constructor</summary>
    protected Collector(CollectorFunction function) :
        base(function)
    {
    }

    /// <summary>The function</summary>
    public new CollectorFunction Function => base.Function as CollectorFunction;

    private CollectorCollectors<TNational, TCompany, TEmployee> collectors;
    /// <summary>The Swissdec collectors</summary>
    public override TCollectors Collectors => (TCollectors)(collectors ??= new(Function, Employee));
}

/// <summary>Swissdec Collector Collectors</summary>
public class CollectorCollectors<TNational, TCompany, TEmployee> : PayrunCollectors
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
{
    /// <summary>The function</summary>
    public TEmployee Employee { get; }

    /// <summary>The function</summary>
    public new CollectorFunction Function => base.Function as CollectorFunction;

    /// <summary>Function constructor</summary>
    public CollectorCollectors(CollectorFunction function, TEmployee employee) :
        base(function)
    {
        Employee = employee;

        Oasi = new(Function, employee);
        Suva = new(Function, employee);
        Sai = new(Function, employee);
        Dsa = new(Function, employee);
        Qst = new(Function, employee);
    }

    /// <summary>Swissdec collector Oasi</summary>
    public CollectorOasi<TNational, TCompany, TEmployee> Oasi { get; }

    /// <summary>Swissdec collector Oasi</summary>
    public CollectorSuva<TNational, TCompany, TEmployee> Suva { get; }

    /// <summary>Swissdec collector Sai</summary>
    public CollectorSai<TNational, TCompany, TEmployee> Sai { get; }

    /// <summary>Swissdec collector Dsa</summary>
    public CollectorDsa<TNational, TCompany, TEmployee> Dsa { get; }

    /// <summary>Swissdec collector Qst</summary>
    public CollectorQst<TNational, TCompany, TEmployee> Qst { get; }
}
