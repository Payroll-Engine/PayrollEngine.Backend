/* Swissdec */
using System;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec core class</summary>
public abstract class Swissdec<TNational, TCompany, TEmployee, TCollectors, TWageTypes> :
    SwissdecBase<PayrollFunction>
    where TNational : National
    where TCompany : Company<TNational>
    where TEmployee : Employee<TNational, TCompany>
    where TCollectors : Collectors
    where TWageTypes : WageTypes
{

    /// <summary>Function constructor</summary>
    protected Swissdec(PayrollFunction function) :
        base(function)
    {
    }

    #region Cases

    private National national;
    /// <summary>Swissdec national</summary>
    public virtual TNational National => (TNational)(national ??= new(Function));

    private Company<TNational> company;
    /// <summary>Swissdec company</summary>
    public virtual TCompany Company => (TCompany)(company ??= new(Function, National));

    private Employee<TNational, TCompany> employee;
    /// <summary>Swissdec employee</summary>
    public virtual TEmployee Employee => (TEmployee)(employee ??= new(Function, National, Company));

    #endregion

    #region Regulation

    private Collectors collectors;
    /// <summary>Swissdec Collectors</summary>
    public virtual TCollectors Collectors => (TCollectors)(collectors ??= new(Function));

    private WageTypes wageTypes;
    /// <summary>Swissdec Wage Types</summary>
    public virtual TWageTypes WageTypes => (TWageTypes)(wageTypes ??= new(Function));

    #endregion

}

/// <summary>Swissdec specification</summary>
public static class Specification
{
    /// <summary>The Swissdec interface name</summary>
    public static readonly string Name = "ELM";

    /// <summary>The Swiss country code</summary>
    public static readonly string SwissCountryCode = "CH";

    /// <summary>The Swissdec residence category</summary>
    public static readonly string ResidenceCategory = "settled-C";

    /// <summary>The Swissdec interface version</summary>
    public static Version Version => new(5, 0);

    /// <summary>The days in a year</summary>
    public static readonly int DaysInYear = 360;

    /// <summary>The count of cantons</summary>
    public static readonly int CantonCount = 32;

    /// <summary>The yearly minimum wage</summary>
    public static readonly decimal YearMinWage = 0;

    /// <summary>The yearly maximum wage</summary>
    public static readonly decimal YearMaxWage = 99999999m;

    /// <summary>SV min days in month</summary>
    public static readonly int SvMinMonthDays = 1;

    /// <summary>SV month days</summary>
    public static readonly int SvMonthDays = 30;

    /// <summary>AHV retirement month days</summary>
    public static readonly int AhvRetirementMonthDays = 30;

    /// <summary>Maximum employee insurance codes</summary>
    public static readonly int MaxEmployeeInsuranceCodes = 5;
}
