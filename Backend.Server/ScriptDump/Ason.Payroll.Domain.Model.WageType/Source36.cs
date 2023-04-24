using System;
using System.Collections.Generic;
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec wage type Oasi</summary>
public class WageTypeOasi<TNational, TCompany, TEmployee> :
    WageTypeToolBase<TNational, TCompany, TEmployee>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
{
    /// <summary>Wage type Oasi constructor</summary>
    public WageTypeOasi(WageTypeFunction function, TEmployee employee) :
        base(function, employee)
    {
    }

    /// <summary>Get wage type OASI/AHV wage</summary>
    public virtual decimal GetWage()
    {
        // contribution status
        var pensionStatus = Employee.GetPensionContributionStatus();
        if (pensionStatus is PensionContributionStatus.SpecialCase or PensionContributionStatus.NonObligatory)
        {
            // no calculation if special case or nonobligatory
            return 0;
        }

        var ahvBaseCollector = Collector[CollectorName.AhvBase];
        if (ahvBaseCollector == 0)
        {
            // no calculation if basis is 0
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
        // check for pensioner
        var isPensioner = pensionStatus == PensionContributionStatus.Pensioner;
        // set SV days for calculation
        var accumulatedSvDays = isPensioner ?
            // accumulated sv days if pensioner (pensioner days)
            Employee.GetPensionAccumulatedDays(accumulationPeriod) :
            // accumulated sv days if obligatory
            Employee.GetSvAccumulatedDays(accumulationPeriod);

        // set result tags for calculation
        var tags = new List<string> { pensionStatus.ToString() };
        var ahvBaseConsCollector = GetConsolidatedCollectorResults(
            NewCollectorQuery(CollectorName.AhvBase, accumulationPeriod.Start, tags)).Sum();
        var oasiConsWage = GetConsolidatedWageTypeResults(
            NewWageTypeQuery(WageTypeNumber.OasiWage, accumulationPeriod.Start, tags)).Sum();

        // set minimum yearly wage for calculation
        var yearlyMinimumWage = isPensioner ?
            // minimum yearly wage if pensioner
            Employee.National.Ahv.RetirementExemption :
            // minimum yearly wage if obligatory
            Specification.YearMinWage;
        var oasiWage = CalculateInsuranceWage(
            yearMinimumWage: yearlyMinimumWage,
            yearMaximumWage: Specification.YearMaxWage,
            collector: ahvBaseCollector + ahvBaseConsCollector,
            previousPeriodsWage: oasiConsWage,
            accumulatedSvDays: accumulatedSvDays).RoundTwentieth();

        Function.SetResultAttribute(AttributeName.Subtotal, oasiWage);

        return oasiWage;

    }

    /// <summary>Get wage type OASI/AHV contribution</summary>
    public virtual decimal GetContribution()
    {
        var wage = WageType[WageTypeNumber.OasiWage];
        var percent = Employee.National.Ahv.ContributionEmployee;
        var oasiContribution = (wage * percent).RoundTwentieth();
        Function.SetResultAttribute(AttributeName.Amount, wage);
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Percentage, percent.ToPercentFormat());
        Function.SetResultAttribute(AttributeName.Subtotal, oasiContribution);
        return oasiContribution;
    }


    /// <summary>Set employee OASI/AHV result</summary>
    public virtual void SetWageResult()
    {
        var oasiCode = Enum.GetName(typeof(PensionContributionStatus), Employee.GetPensionContributionStatus());
        Function.SetResultTags(new[] { oasiCode });
    }
}
