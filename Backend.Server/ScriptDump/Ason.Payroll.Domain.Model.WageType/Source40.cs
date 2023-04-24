using System.Collections.Generic;
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec wage type Sui</summary>
public class WageTypeSui<TNational, TCompany, TEmployee> :
    WageTypeToolBase<TNational, TCompany, TEmployee>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
{
    /// <summary>Wage type Sui constructor</summary>
    public WageTypeSui(WageTypeFunction function, TEmployee employee) :
        base(function, employee)
    {
    }

    /// <summary> Get wage type SUI/ALVZ wage</summary>
    public virtual decimal GetWage()
    {
        // ALV base
        var alvBaseCollector = Collector[CollectorName.AlvBase];
        if (alvBaseCollector == 0)
        {
            // no calculation if base is 0
            return 0;
        }

        // pension contribution status
        var pensionStatus = Employee.GetPensionContributionStatus();
        if (pensionStatus != PensionContributionStatus.Obligatory)
        {
            // no calculation if not obligatory
            return 0;
        }

        // set accumulation period for calculation
        // check if back payment
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
        var suiConsWage = GetConsolidatedWageTypeResults(
            NewWageTypeQuery(WageTypeNumber.SuiWage, accumulationPeriod.Start)).Sum();

        var suiWage = CalculateInsuranceWage(
            yearMinimumWage: Employee.National.Alv.UpperLimit,
            yearMaximumWage: Employee.National.Alvz.UpperLimit,
            collector: alvBaseCollector + alvBaseConsCollector,
            previousPeriodsWage: suiConsWage,
            accumulatedSvDays: Employee.GetSvAccumulatedDays(accumulationPeriod),
            allowNegative: false).RoundTwentieth();

        Function.SetResultAttribute(AttributeName.Subtotal, suiWage);

        return suiWage;
    }

    /// <summary>Get wage type SUI/ALVZ contribution</summary>
    public virtual decimal GetContribution()
    {
        var wage = WageType[WageTypeNumber.SuiWage];
        var percent = Employee.National.Alvz.ContributionEmployee;
        var suiContribution = (wage * percent).RoundTwentieth();
        Function.SetResultAttribute(AttributeName.Amount, wage);
        Function.SetResultAttribute(AttributeName.Percentage, percent.ToPercentFormat());
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Subtotal, suiContribution);
        return suiContribution;
    }

}
