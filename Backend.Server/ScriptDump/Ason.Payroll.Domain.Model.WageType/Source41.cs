using System.Collections.Generic;
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec wage type Suva</summary>
public class WageTypeSuva<TNational, TCompany, TEmployee> :
    WageTypeToolBase<TNational, TCompany, TEmployee>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
{
    /// <summary>Wage type Suva constructor</summary>
    public WageTypeSuva(WageTypeFunction function, TEmployee employee) :
        base(function, employee)
    {
    }

    /// <summary>Get wage type SUVA/UVG wage</summary>
    public virtual decimal GetWage()
    {
        // UVG base
        var uvgBaseCollector = Collector[CollectorName.UvgBase];
        if (uvgBaseCollector == 0)
        {
            // no calculation if basis is 0
            return 0;
        }

        // Employee UVG InstitutionId does not update value on case change 
        var insuranceName = Employee.Uvg.InstitutionId;
        if (string.IsNullOrWhiteSpace(insuranceName))
        {
            // no calculation if institution is not set
            return 0;
        }

        // Employee UVG code
        var uvgCode = Employee.Uvg.Code;
        if (string.IsNullOrWhiteSpace(uvgCode))
        {
            // no calculation if code is not set
            return 0;
        }

        // insurance
        var insurance = Employee.Company.ContractProvider.GetInsurance(InsuranceName.Uvg, insuranceName);
        if (insurance == null || string.IsNullOrWhiteSpace(insurance.DefaultCode))
        {
            // no calculation if company insurances are not set
            return 0;
        }

        // insurance contribution and fallback contribution
        var contribution = Employee.Company.ContractProviderContributions.GetContribution(
            InsuranceName.Uvg, uvgCode);
        var fallbackContribution = Employee.Company.ContractProviderContributions.GetContribution(
            InsuranceName.Uvg, insurance.DefaultCode);
        if (contribution == null && fallbackContribution == null)
        {
            // no calculation if no contribution data is found
            return 0;
        }

        // UVG insurance type
        var insuranceType = EmployeeUvgInsurance.GetInsuranceType(uvgCode);
        if (insuranceType is null or UvgInsuranceType.NotInsured)
        {
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

        // set Tags based on code
        var tags = insuranceType is UvgInsuranceType.InsuredShared or UvgInsuranceType.InsuredCompany ?
            // tags if employee code is x1 or x2 (accumulation based on both codes)
            new()
            {
                OrTag,
                EmployeeUvgInsurance.BuildCode(uvgCode[0], UvgInsuranceType.InsuredShared),
                EmployeeUvgInsurance.BuildCode(uvgCode[0], UvgInsuranceType.InsuredCompany) } :
            // tags in any other case
            new List<string> { uvgCode };
        var uvgBaseConsCollector = GetConsolidatedCollectorResults(
            NewCollectorQuery(CollectorName.UvgBase, accumulationPeriod.Start, tags)).Sum();
        var suvaConsWage = GetConsolidatedWageTypeResults(
            NewWageTypeQuery(WageTypeNumber.SuvaWage, accumulationPeriod.Start, tags));

        // get accumulatedSvDays from previous wage type result attributes + current
        var svDaysByCode = Employee.GetSvMonthDays();
        foreach (var suvaWage in suvaConsWage)
        {
            if (int.TryParse(suvaWage.Attributes["SvMonthDays"].ToString(), out var svDays))
            {
                svDaysByCode += svDays;
            }
        }

        // yearly minimum wage
        var yearlyMinimumWage = contribution != null
            // yearly minimum wage if contribution data exists
            ? contribution.SalaryMin.Safe()
            // yearly minimum wage if contribution data does not exist
            : fallbackContribution.SalaryMin.Safe();
        // yearly maximum wage
        var yearlyMaximumWage = contribution != null
            // yearly maximum wage if contribution data exists
            ? contribution.SalaryMax.Safe()
            // yearly maximum wage if contribution data does not exist
            : fallbackContribution.SalaryMax.Safe();
        var wage = CalculateInsuranceWage(
            yearMinimumWage: yearlyMinimumWage,
            yearMaximumWage: yearlyMaximumWage,
            collector: uvgBaseCollector + uvgBaseConsCollector,
            previousPeriodsWage: suvaConsWage.Sum(),
            accumulatedSvDays: svDaysByCode).RoundTwentieth();

        Function.SetResultAttribute(AttributeName.Subtotal, wage);

        return wage;
    }

    /// <summary>Get wage type SUVA/UVG contribution</summary>
    public virtual decimal GetContribution()
    {
        // SUVA/UVG insurance code
        var uvgCode = Employee.Uvg.Code;
        if (string.IsNullOrWhiteSpace(uvgCode))
        {
            return 0;
        }

        // contributions
        var contribution = Employee.Company.ContractProviderContributions.GetContribution(
            InsuranceName.Uvg, uvgCode);
        if (contribution == null)
        {
            return 0;
        }

        // insurance type
        var insuranceType = EmployeeUvgInsurance.GetInsuranceType(uvgCode);
        if (insuranceType is null or UvgInsuranceType.NotInsured or UvgInsuranceType.InsuredCompanyOnly)
        {
            return 0;
        }

        // gender
        var gender = Employee.GetGender();
        if (gender == null)
        {
            return 0;
        }
        var percent = contribution.GetAnPercent(gender.Value);
        var wage = WageType[WageTypeNumber.SuvaWage];
        var suvaContribution = (wage * percent).RoundTwentieth();
        Function.SetResultAttribute(AttributeName.Amount, wage);
        Function.SetResultAttribute(AttributeName.Code, uvgCode);
        Function.SetResultAttribute(AttributeName.Percentage, percent.ToPercentFormat());
        Function.SetResultAttribute(AttributeName.Report, new List<string> { ReportName.Payslip });
        Function.SetResultAttribute(AttributeName.Subtotal, suvaContribution);
        return suvaContribution;
    }

    /// <summary>Set employee SUVA/UVG result</summary>
    public void SetWageResult()
    {
        var employeeCode = Function.GetCaseValue<string>(CaseFieldName.EmployeeUvgInsuranceCode);
        if (string.IsNullOrWhiteSpace(employeeCode))
        {
            return;
        }

        Function.SetResultTags(new[] { employeeCode });
        SetSvMonthDaysToResult();
    }
}
