using System.Collections.Generic;
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec wage type Ui</summary>
public class WageTypeUi<TNational, TCompany, TEmployee> :
    WageTypeToolBase<TNational, TCompany, TEmployee>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
{

    /// <summary>Wage type Ui constructor</summary>
    public WageTypeUi(WageTypeFunction function, TEmployee employee) :
        base(function, employee)
    {
    }

    /// <summary>Get wage type UI/ALV wage</summary>
    public virtual decimal GetWage()
    {
        // pension contribution status
        var pensionStatus = Employee.GetPensionContributionStatus();
        if (pensionStatus != PensionContributionStatus.Obligatory)
        {
            // no calculation if not obligatory
            return 0;
        }

        var alvBaseCollector = Collector[CollectorName.AlvBase];
        if (alvBaseCollector == 0)
        {
            // no calculation if base is 0
            return 0;
        }

        // set accumulation period for calculation
        var accumulationPeriod = !Employee.IsBackPayment() ?
            // accumulation period is current cycle
            Employee.YearToDatePeriod :
            // accumulation period is cycle of last withdrawal date
            Employee.WithdrawalDate.Safe().IsBefore(Function.Cycle) ?
                // accumulation period if withdrawal date before cycle
                Function.GetCycle(Employee.WithdrawalDate.Safe()) :
                // accumulation period if withdrawal date within cycle
                Employee.YearToDatePeriod;

        // accumulated base
        var alvBaseConsCollector = GetConsolidatedCollectorResults(
            NewCollectorQuery(CollectorName.AlvBase, accumulationPeriod.Start)).Sum();
        // accumulated wage type
        var uiConsWage = GetConsolidatedWageTypeResults(
            NewWageTypeQuery(WageTypeNumber.UiWage, accumulationPeriod.Start)).Sum();

        var uiWage = CalculateInsuranceWage(
            yearMinimumWage: Specification.YearMinWage,
            yearMaximumWage: Employee.National.Alv.UpperLimit,
            collector: alvBaseCollector + alvBaseConsCollector,
            previousPeriodsWage: uiConsWage,
            accumulatedSvDays: Employee.GetSvAccumulatedDays(accumulationPeriod),
            allowNegative: false).RoundTwentieth();


        return uiWage;
    }

    /// <summary>Get wage type UI/ALV contribution</summary>
    public virtual decimal GetContribution()
    {
        var wage = WageType[WageTypeNumber.UiWage];
        var percent = Employee.National.Alv.ContributionEmployee;
        var uiContribution = (wage * percent).RoundTwentieth();
        Function.SetResultAttribute(AttributeName.Amount, wage);
        Function.SetResultAttribute(AttributeName.Percentage, percent.ToPercentFormat());
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Subtotal, uiContribution);
        return uiContribution;
    }

}
