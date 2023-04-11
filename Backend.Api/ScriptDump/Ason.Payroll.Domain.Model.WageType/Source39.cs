using System.Collections.Generic;
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec wage type Sai</summary>
public class WageTypeSai<TNational, TCompany, TEmployee> :
    WageTypeToolBase<TNational, TCompany, TEmployee>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
{
    /// <summary>Wage type Sui constructor</summary>
    public WageTypeSai(WageTypeFunction function, TEmployee employee) :
        base(function, employee)
    {
    }

    /// <summary>Get wage type SAI/UVGZ wage</summary>
    public virtual decimal GetWage(int employeeCaseSlot)
    {
        // slot
        if (!IsValidEmployeeInsuranceSlot(employeeCaseSlot))
        {
            return 0;
        }
        var caseSlot = employeeCaseSlot.ToString();

        // UVGZ base
        var uvgzBaseCollector = Collector[CollectorName.UvgzBase];
        if (uvgzBaseCollector == 0)
        {
            return 0;
        }

        // insurance
        var insurance = Employee.Uvgz.GetInsurance(caseSlot);
        if (insurance == null)
        {
            return 0;
        }

        // contribution
        var contribution = Employee.Company.ContractProviderContributions.GetContribution(
            InsuranceName.Uvgz, insurance.Code);
        if (contribution == null)
        {
            return 0;
        }

        // get code raw case value start date
        var uvgzInsuranceCode = Function.GetRawCaseValue(
            Function.CaseFieldSlot(CaseFieldName.EmployeeUvgzInsuranceCode, caseSlot), Function.PeriodEnd);
        if (uvgzInsuranceCode?.Start == null)
        {
            return 0;
        }
        var employeeCodeStart = uvgzInsuranceCode.Start.Value;
        // accumulation cannot start before cycle
        var beginAccumulationDate = employeeCodeStart.IsBefore(Function.Cycle) ? Function.CycleStart : employeeCodeStart;

        var withdrawalDate = Employee.WithdrawalDate.Safe();
        // set accumulation period for calculation
        var accumulationPeriod = !Employee.IsBackPayment() ?
            // accumulation period if no back payment
            new(beginAccumulationDate, Function.PeriodEnd) :
            // accumulation period is cycle of last withdrawal date
            withdrawalDate.IsBefore(Function.Cycle) ?
                // accumulation period if withdrawal date is before cycle
                Function.GetCycle(withdrawalDate) :
                // accumulation period if withdrawal date is within cycle
                new(beginAccumulationDate, Function.PeriodEnd);

        // slot wage type
        var wageType = WageTypes.GetSlotWageTypeNumber(WageTypeNumber.SaiWage, employeeCaseSlot);
        // set tags for result filtering
        var tags = new List<string> { insurance.Code };
        // get accumulated wage basis
        var uvgzBaseConsCollector = GetConsolidatedCollectorResults(
            NewCollectorQuery(CollectorName.UvgzBase, accumulationPeriod.Start, tags)).Sum();
        // get accumulated wage type result
        var uvgzConsWage = GetConsolidatedWageTypeResults(
            NewWageTypeQuery(wageType, accumulationPeriod.Start, tags)).Sum();

        return CalculateInsuranceWage(
            yearMinimumWage: contribution.SalaryMin.Safe(),
            yearMaximumWage: contribution.SalaryMax.Safe(),
            collector: uvgzBaseCollector + uvgzBaseConsCollector,
            previousPeriodsWage: uvgzConsWage,
            accumulatedSvDays: Employee.GetSvAccumulatedDays(accumulationPeriod),
            allowNegative: false).RoundTwentieth();
    }

    /// <summary>Get wage type SAI/UVGZ contribution (uvgz contribution)</summary>
    public virtual decimal GetContribution()
    {
        var gender = Employee.GetGender();
        if (gender == null)
        {
            return 0;
        }

        // calculate each & total contribution
        decimal total = 0;
        foreach (var insurance in Employee.Uvgz.Insurances.Values)
        {
            // company contribution
            var contribution = Employee.Company.ContractProviderContributions.GetContribution(
                InsuranceName.Uvgz, insurance.Code);
            if (contribution == null)
            {
                continue;
            }

            // get percentage
            var percent = contribution.GetAnPercent(gender.Value);
            // slot wage type
            var wageType = WageTypes.GetSlotWageTypeNumber(WageTypeNumber.SaiWage, insurance.CaseSlot);
            var result = (WageType[wageType] * percent).RoundTwentieth();


            // add custom result
            var attributes = new Dictionary<string, object>
            {
                { AttributeName.Amount, WageType[wageType] },
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

    /// <summary>Set employee SAI/UVGZ result</summary>
    public void SetWageResult(int employeeCaseSlot)
    {
        if (!IsValidEmployeeInsuranceSlot(employeeCaseSlot))
        {
            return;
        }
        var employeeCode = Function.GetCaseValue<string>(Function.CaseFieldSlot(
            CaseFieldName.EmployeeUvgzInsuranceCode, employeeCaseSlot.ToString()));
        if (string.IsNullOrWhiteSpace(employeeCode))
        {
            return;
        }
        Function.SetResultTags(new[] { employeeCode });
    }
}
