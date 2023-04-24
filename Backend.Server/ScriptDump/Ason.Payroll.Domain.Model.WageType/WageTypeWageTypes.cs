/* WageTypeWageTypes */
using System.Linq;
using System.Collections.Generic;
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Cache;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec wage types for wage type value</summary>
public class WageTypeWageTypes<TNational, TCompany, TEmployee> : PayrunWageTypes
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
{
    /// <summary>Function constructor value</summary>
    public WageTypeWageTypes(WageTypeFunction function, TEmployee employee) :
        base(function)
    {
        Employee = employee;

        Oasi = new(function, employee);
        Ui = new(function, employee);
        Sui = new(function, employee);
        Suva = new(function, employee);
        Sai = new(function, employee);
        Dsa = new(function, employee);
        Qst = new(function, employee);

        // lookups
        WageType = new(wageTypeNumber => Function.WageType[wageTypeNumber]);
        Collector = new(collectorName => Function.Collector[collectorName]);
    }

    /// <summary>The function value</summary>
    public new WageTypeFunction Function => base.Function as WageTypeFunction;

    /// <summary>Swissdec wage type Oasi value</summary>
    public WageTypeOasi<TNational, TCompany, TEmployee> Oasi { get; }

    /// <summary>Swissdec wage type Ui value</summary>
    public WageTypeUi<TNational, TCompany, TEmployee> Ui { get; }

    /// <summary>Swissdec wage type Sui value</summary>
    public WageTypeSui<TNational, TCompany, TEmployee> Sui { get; }

    /// <summary>Swissdec wage type Suva value</summary>
    public WageTypeSuva<TNational, TCompany, TEmployee> Suva { get; }

    /// <summary>Swissdec wage type Sai value</summary>
    public WageTypeSai<TNational, TCompany, TEmployee> Sai { get; }

    /// <summary>Swissdec wage type Dsa value</summary>
    public WageTypeDsa<TNational, TCompany, TEmployee> Dsa { get; }

    /// <summary>Swissdec wage type Qst value</summary>
    public WageTypeQst<TNational, TCompany, TEmployee> Qst { get; }

    #region Internal

    /// <summary>The employee value</summary>
    private TEmployee Employee { get; }

    /// <summary>Get a wage type value by wage type number value</summary>
    private ScriptDictionary<decimal, decimal> WageType { get; }

    /// <summary>Get a collector value by collector name value</summary>
    private ScriptDictionary<string, decimal> Collector { get; }

    /// <summary>Create new collector result cycle query value</summary>
    private CollectorConsolidatedResultQuery NewCollectorCycleQuery(string collectorName, List<string> tags = null)
    {
        return new(collectorName, Function.CycleStart, PayrunJobStatus.Legal) { Tags = tags };
    }

    private CollectorConsolidatedResultCache collectorConsolidatedResultCache;
    /// <summary>Get the collector results value</summary>
    protected IList<CollectorResult> GetConsolidatedCollectorResults(CollectorConsolidatedResultQuery query)
    {
        collectorConsolidatedResultCache ??= new(Function.CycleStart, PayrunJobStatus.Legal);
        return collectorConsolidatedResultCache.GetConsolidatedResults(Function, query);
    }

    /// <summary>Create wage type result cycle query value</summary>
    private WageTypeConsolidatedResultQuery NewWageTypeCycleQuery(decimal wageTypeNumber, List<string> tags = null)
    {
        return new(wageTypeNumber, Function.CycleStart, PayrunJobStatus.Legal) { Tags = tags };
    }

    private WageTypeConsolidatedResultCache wageTypeConsolidatedResultCache;
    /// <summary>Get the collector results value</summary>
    protected IList<Payroll.Client.Scripting.WageTypeResult> GetConsolidatedWageTypeResults(WageTypeConsolidatedResultQuery query)
    {
        wageTypeConsolidatedResultCache ??= new(Function.CycleStart, PayrunJobStatus.Legal);
        return wageTypeConsolidatedResultCache.GetConsolidatedResults(Function, query);
    }

    #endregion

    #region Child

    /// <summary>Get employee child and education allowance wage type value</summary>
    /// <remarks>wage type 3000</remarks>
    public virtual decimal GetChildAllowanceWage()
    {
        var childAllowance = Employee.GetChildAllowance().RoundTwentieth();
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Subtotal, childAllowance);
        return childAllowance;
    }

    /// <summary>Get employee child allowance and education allowance back payment wage type value</summary>
    /// <remarks>wage type 3001</remarks>
    public virtual decimal GetChildAllowanceBackPaymentWage()
    {
        var childAllowanceBackPayment = Employee.Children.Values.Sum(child =>
                child.ChildAllowanceBackPayment.Safe() + child.EducationAllowanceBackPayment.Safe())
            .RoundTwentieth();
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Subtotal, childAllowanceBackPayment);
        return childAllowanceBackPayment;
    }

    #endregion

    #region Wages

    /// <summary>Get employee month salary value</summary>
    /// <remarks>wage type 1000</remarks>
    public virtual decimal? GetMonthSalaryWage()
    {
        // TODO month salary null vs 0 (result test)
        var monthSalary = Employee.HasSvMonthDays() ? Employee.Statistics.ContractualMonthlyWage?.RoundTwentieth() : null;
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Subtotal, monthSalary);
        return monthSalary;
    }
    
    /// <summary>Get employee hourly wage value</summary>
    /// <remarks>wage type 1005</remarks>
    public virtual decimal GetHourlyWage()
    {
        decimal result = 0;
        var caseValues = Function.GetCaseValues(
            CaseFieldName.EmployeeStatisticsContractualHourlyWage,
            CaseFieldName.EmployeeActivityWorkedHours);
        if (caseValues.HasAllValues)
        {
            result = caseValues[CaseFieldName.EmployeeStatisticsContractualHourlyWage] *
                     caseValues[CaseFieldName.EmployeeActivityWorkedHours];
        }

        Function.SetResultAttribute(AttributeName.Amount, caseValues[CaseFieldName.EmployeeActivityWorkedHours].Value);
        Function.SetResultAttribute(AttributeName.Percentage, caseValues[CaseFieldName.EmployeeStatisticsContractualHourlyWage].Value);
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Subtotal, result);

        return result.RoundTwentieth();
    }

    /// <summary>Get wage type daily wage value</summary>
    /// <remarks>wage type 1006</remarks>
    public virtual decimal GetDailyWage()
    {
        decimal result = 0;
        var caseValues = Function.GetCaseValues(
            CaseFieldName.EmployeeStatisticsContractualHourlyWage,
            CaseFieldName.EmployeeActivityWorkedLessons);
        if (caseValues.HasAllValues)
        {
            result = caseValues[CaseFieldName.EmployeeStatisticsContractualHourlyWage] *
                     caseValues[CaseFieldName.EmployeeActivityWorkedLessons];
        }

        Function.SetResultAttribute(AttributeName.Amount, caseValues[CaseFieldName.EmployeeActivityWorkedLessons].Value);
        Function.SetResultAttribute(AttributeName.Percentage, caseValues[CaseFieldName.EmployeeStatisticsContractualHourlyWage].Value);
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Subtotal, result);

        return result.RoundTwentieth();
    }
    
    /// <summary>Get employee holiday compensation value</summary>
    /// <remarks>wage type 1160</remarks>
    public virtual decimal GetHolidayCompensationWage()
    {
        var baseWage = WageType[WageTypeNumber.HourlyWage] + WageType[WageTypeNumber.DailyWage];
        var compensationPercent = Employee.Wage.HolidayCompensation.Safe();
        var wage = (baseWage * compensationPercent).RoundTwentieth();

        Function.SetResultAttribute(AttributeName.Amount, baseWage);
        Function.SetResultAttribute(AttributeName.Percentage, compensationPercent.ToPercentFormat());
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Subtotal, wage);

        return wage;
    }

    /// <summary>Get employee public holiday compensation value</summary>
    /// <remarks>wage type 1161</remarks>
    public virtual decimal GetPublicHolidayCompensationWage()
    {
        var baseWage = WageType[WageTypeNumber.HourlyWage] + WageType[WageTypeNumber.DailyWage];
        var compensationPercent = Employee.Wage.PublicHolidayCompensation.Safe();
        var wage = (baseWage * compensationPercent).RoundTwentieth();

        Function.SetResultAttribute(AttributeName.Amount, baseWage);
        Function.SetResultAttribute(AttributeName.Percentage, compensationPercent.ToPercentFormat());
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Subtotal, wage);

        return wage;
    }
    
    /// <summary>Get employee 13th month wage supplementary wages value</summary>
    private decimal GetSupplementaryWagesWage()
    {
        return (WageType[WageTypeNumber.DailyWage] +
                WageType[WageTypeNumber.AdditionalWork] +
                WageType[WageTypeNumber.Overtime125Percent] +
                WageType[WageTypeNumber.HolidayCompensation] +
                WageType[WageTypeNumber.PublicHolidayCompensation]).RoundTwentieth();
    }

    /// <summary>Get employee 13th month wage value</summary>
    /// <remarks>wage type 1200</remarks>
    public virtual decimal GetMonthly13thWage()
    {
        decimal wage = 0;
        // TODO: check if implemented solution for wage in special case is correct
        if (Function.IsRetroPayrun != Function.IsCycleRetroPayrun)
        {
            //Function.DisableCollector(CollectorName.QstpBase);
            var wage13th = Function.GetWageTypeResults(new WageTypeRangeResultQuery(
                new[] { WageTypeNumber.MonthlyWage13th }, Function.PeriodStart, Function.PeriodEnd,
                PayrunJobStatus.Legal)).Sum();
            Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
            Function.SetResultAttribute(AttributeName.Subtotal, wage13th.RoundTwentieth());
            return wage13th.RoundTwentieth();
        }

        // percentage for 13th wage is set
        var wage13thPercent = Employee.Statistics.Contractual13thPercent.Safe();
        if (wage13thPercent == 0)
        {
            wage = Employee.Wage.MonthlyWage13th.Safe();
        }
        else
        {
            // monthly salary
            var monthlySalary = WageType[WageTypeNumber.MonthlySalary];
            if (monthlySalary > 0)
            {
                var consCollector = Function.Collector[CollectorName.ThirteenthMonthWageBase] +
                                    GetConsolidatedCollectorResults(
                                        NewCollectorCycleQuery(CollectorName.ThirteenthMonthWageBase)).Sum();
                var consWage = GetConsolidatedWageTypeResults(
                    NewWageTypeCycleQuery(WageTypeNumber.MonthlyWage13th)).Sum();
                wage = consCollector / Date.MonthsInYear - consWage;
            }
            else
            {
                // hourly salary
                var hourlyWage = WageType[WageTypeNumber.HourlyWage];
                if (hourlyWage > 0)
                {
                    var supplementaryWages = GetSupplementaryWagesWage();
                    var baseWage = hourlyWage + supplementaryWages;
                    wage = baseWage * wage13thPercent;
                    Function.SetResultAttribute(AttributeName.Amount, baseWage);
                    Function.SetResultAttribute(AttributeName.Percentage, wage13thPercent.ToPercentFormat());
                }
            }
        }
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Subtotal, wage.RoundTwentieth());
        return wage.RoundTwentieth();
    }
    
    /// <summary>Get wage value and set result attributes</summary>
    /// <param name="wage"></param>
    /// <remarks>Sets attribute "Report" value</remarks>
    public virtual decimal? GetWage(decimal? wage)
    {
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Subtotal, wage);
        return wage;
    }
    
    /// <summary>Get employee payment in kind adjustment value</summary>
    public virtual decimal GetPaymentInKindAdjustmentWage()
    {
        return (WageType[WageTypeNumber.MealsFreeCharge] +
                WageType[WageTypeNumber.AccommodationFreeCharge] +
                WageType[WageTypeNumber.RentedFlatPriceReduction]).RoundTwentieth();
    }

    /// <summary>Get employee payment in kind adjustment value</summary>
    public virtual decimal GetNonCashBenefitsAdjustmentWage()
    {
        return (WageType[WageTypeNumber.FringeBenefitsCar] +
                WageType[WageTypeNumber.EmployeeShares] +
                WageType[WageTypeNumber.EmployeeOptions] +
                WageType[WageTypeNumber.EmployerFacultativeDsa] +
                WageType[WageTypeNumber.EmployerFacultativePillar3a] +
                WageType[WageTypeNumber.FacultativeWithholdingTax]).RoundTwentieth();
    }

    /// <summary>Get employee payment in kind adjustment value</summary>
    public virtual decimal GetPfLobEmployerAdjustmentWage()
    {
        return (WageType[WageTypeNumber.EmployerFacultativePfLob] +
                WageType[WageTypeNumber.EmployerFacultativeRedemptionPfLob]).RoundTwentieth();
    }

    /// <summary>Get employee payment in kind adjustment value</summary>
    /// <remarks>wage type 5000</remarks>
    public virtual decimal GetGrossWage()
    {
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Total, Collector[CollectorName.GrossSalary].RoundTwentieth());
        return Collector[CollectorName.GrossSalary].RoundTwentieth();
    }
    
    /// <summary>Get employee net wage retro wage value</summary>
    /// <remarks>wage type 6499</remarks>
    public virtual decimal GetNetWageRetroWage()
    {
        var netWageSum = Function.GetWageTypeRetroResults(new(WageTypeNumber.NetWage)).Sum();
        var newtWageRetroResults = Function.GetPeriodWageTypeResults(new(WageTypeNumber.NetWageRetro, 0, PayrunJobStatus.Legal));
        var netWageRetroWage = newtWageRetroResults.Any() ? newtWageRetroResults.Last().Value : 0;
        return (netWageSum + netWageRetroWage).RoundTwentieth();
    }

    /// <summary>Get employee net wage value</summary>
    /// <remarks>Used by wage type 6500</remarks>
    public virtual decimal GetNetWage()
    {
        var netWage = (Collector[CollectorName.GrossSalary] -
                       Collector[CollectorName.EmployeeContributions] +
                       Collector[CollectorName.Expenses] +
                       GetNetWageRetroWage()).RoundTwentieth();
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Total, netWage);

        return netWage;
    }
    #endregion

}
