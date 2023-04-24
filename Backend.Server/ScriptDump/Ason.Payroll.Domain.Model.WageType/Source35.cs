using System.Collections.Generic;
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec wage type Dsa</summary>
public class WageTypeDsa<TNational, TCompany, TEmployee> :
    WageTypeToolBase<TNational, TCompany, TEmployee>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
{
    /// <summary>Wage type Dsa constructor</summary>
    public WageTypeDsa(WageTypeFunction function, TEmployee employee) :
        base(function, employee)
    {
    }

    /// <summary>Get wage type DSA/KTG wage</summary>
    public virtual decimal GetWage(int employeeCaseSlot)
    {
        if (!IsValidEmployeeInsuranceSlot(employeeCaseSlot))
        {
            return 0;
        }
        var slot = employeeCaseSlot.ToString();

        // DSA/KTG collector
        var ktgBaseCollector = Collector[CollectorName.KtgBase];
        if (ktgBaseCollector == 0)
        {
            return 0;
        }

        // insurance
        var insurance = Employee.Ktg.GetInsurance(slot);
        if (insurance == null)
        {
            return 0;
        }

        // contribution
        var contribution = Employee.Company.ContractProviderContributions.GetContribution(
            InsuranceName.Ktg, insurance.Code);
        if (contribution == null)
        {
            return 0;
        }

        var withdrawalDate = Employee.WithdrawalDate.Safe();
        // set accumulation period for calculation
        var accumulationPeriod = !Employee.IsBackPayment() ?
            // accumulation period if no back payment
            Employee.YearToDatePeriod :
            // accumulation period is cycle of last withdrawal date
            withdrawalDate.IsBefore(Function.Cycle) ?
                // accumulation period if withdrawal date is before cycle
                Employee.WithdrawalCycle :
                // accumulation period if withdrawal date is within cycle
                Employee.YearToDatePeriod;

        // slot wage type
        var wageType = WageTypes.GetSlotWageTypeNumber(WageTypeNumber.DsaWage, employeeCaseSlot);
        // set tags for result filtering
        var tags = new List<string> { insurance.Code };
        // get accumulated wage basis
        var ktgBaseConsCollector = GetConsolidatedCollectorResults(
            NewCollectorQuery(CollectorName.KtgBase, accumulationPeriod.Start, tags)).Sum();
        // get accumulated wage type result
        var ktgConsWage = GetConsolidatedWageTypeResults(
            NewWageTypeQuery(wageType, accumulationPeriod.Start, tags));
        // get accumulatedSvDays from previous wage type result attributes
        var svDaysByCode = Employee.GetSvMonthDays();
        foreach (var ktgWage in ktgConsWage)
        {
            if (int.TryParse(ktgWage.Attributes["SvMonthDays"].ToString(), out var svDays))
            {
                svDaysByCode += svDays;
            }
        }

        return CalculateInsuranceWage(
            yearMinimumWage: contribution.SalaryMin.Safe(),
            yearMaximumWage: contribution.SalaryMax.Safe(),
            collector: ktgBaseCollector + ktgBaseConsCollector,
            previousPeriodsWage: ktgConsWage.Sum(),
            accumulatedSvDays: svDaysByCode,
            allowNegative: false).RoundTwentieth();
    }

    /// <summary>Get wage type DSA/KTG contribution</summary>
    public virtual decimal GetContribution()
    {
        // gender
        var gender = Employee.GetGender();
        if (gender == null)
        {
            return 0;
        }

        // calculate each & total contribution
        decimal total = 0;
        foreach (var insurance in Employee.Ktg.Insurances.Values)
        {
            // company contribution
            var contributionInsurance = Employee.Company.ContractProviderContributions.GetContribution(
                InsuranceName.Ktg, insurance.Code);
            if (contributionInsurance == null)
            {
                continue;
            }

            // insurance percentage
            var percent = contributionInsurance.GetAnPercent(gender.Value);

            // slot wage type
            var dsaWageNumber = WageTypes.GetSlotWageTypeNumber(WageTypeNumber.DsaWage, insurance.CaseSlot);
            var result = (WageType[dsaWageNumber] * percent).RoundTwentieth();

            // add custom result
            var attributes = new Dictionary<string, object>
            {
                { AttributeName.Amount, WageType[dsaWageNumber] },
                { AttributeName.Code, insurance.Code },
                { AttributeName.Percentage, percent.ToPercentFormat() },
                { AttributeName.Report, new List<string> { ReportName.Payslip } },
                { AttributeName.Subtotal, result }
            };
            Function.AddCustomResult(
                source: Function.WageTypeName,
                value: result,
                // set tags for custom result
                tags: new List<string> { insurance.Code, insurance.CaseSlot },
                attributes: attributes);
            // add to total
            total += result;
        }
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        return total.RoundTwentieth();
    }

    /// <summary>Set employee DSA/KTG result</summary>
    public void SetWageResult(int employeeCaseSlot)
    {
        if (!IsValidEmployeeInsuranceSlot(employeeCaseSlot))
        {
            return;
        }
        var employeeCode = Function.GetCaseValue<string>(Function.CaseFieldSlot(
            CaseFieldName.EmployeeKtgInsuranceCode, employeeCaseSlot.ToString()));
        if (string.IsNullOrWhiteSpace(employeeCode))
        {
            return;
        }
        SetSvMonthDaysToResult();
        Function.SetResultTags(new[] { employeeCode });
    }
}
