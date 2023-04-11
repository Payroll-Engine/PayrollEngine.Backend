/* PayrollEmployee */
using System;
using System.Collections.Generic;
using System.Globalization;
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec payroll employee</summary>
public class PayrunEmployee<TNational, TCompany> :
    Employee<TNational, TCompany>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
{
    /// <summary>Function constructor</summary>
    public PayrunEmployee(PayrunFunction function, TNational national, TCompany company) :
        base(function, national, company)
    {
        YearToDatePeriod = new(function.CycleStart, function.PeriodEnd);
        WithdrawalPeriod = function.GetPeriod(WithdrawalDate.Safe());
        WithdrawalCycle = function.GetCycle(WithdrawalPeriod.Start);
    }

    /// <summary>The function</summary>
    public new PayrunFunction Function => base.Function as PayrunFunction;

    /// <summary>Period from year start to the end of the current period</summary>
    public DatePeriod YearToDatePeriod { get; }
    /// <summary>Period in witch the withdrawal date is</summary>
    public DatePeriod WithdrawalPeriod { get; }
    /// <summary>Cycle in witch the withdrawal date is</summary>
    public DatePeriod WithdrawalCycle { get; }

    #region SV

    /// <summary>Test for SV month days</summary>
    public bool HasSvMonthDays() =>
        GetSvMonthDays(Function.Period) != 0;

    /// <summary>Test for SV month days</summary>
    public bool HasSvMonthDays(DatePeriod period) =>
        GetSvMonthDays(period) != 0;

    /// <summary>Get the SV days for the current period</summary>
    /// <returns>
    /// 0: for unemployed
    /// 1: minimum SV days <see cref="Specification.SvMinMonthDays"/>
    /// 30: SV months days <see cref="Specification.SvMonthDays"/>
    /// </returns>
    public int GetSvMonthDays() =>
        GetSvMonthDays(Function.Period);

    /// <summary>Get the SV days for a period</summary>
    /// <param name="period">The period</param>
    /// <returns>
    /// 0: for unemployed
    /// 1: minimum SV days <see cref="Specification.SvMinMonthDays"/>
    /// 30: SV months days <see cref="Specification.SvMonthDays"/>
    /// </returns>
    public int GetSvMonthDays(DatePeriod period)
    {
        // entry date value should be safe
        var entryDateCaseValue = Function.GetPeriodCaseValue(period, CaseFieldName.EmployeeEntryDate);
        if (!entryDateCaseValue.HasValue)
        {
            return 0;
        }
        var entryDate = entryDateCaseValue.ValueAs<DateTime>();
        var isEntryMonth = entryDate.IsSameMonth(period.Start);

        // check for unemployed, period range limits
        var withdrawalDate = Function.GetPeriodCaseValue<DateTime?>(period, CaseFieldName.EmployeeWithdrawalDate);
        if (entryDate > period.End || (withdrawalDate.HasValue && withdrawalDate.Value < period.Start))
        {
            return 0;
        }

        // entry and withdrawal date outside of the period
        var isWithdrawalMonth = withdrawalDate.HasValue && withdrawalDate.Value.IsSameMonth(period.Start);
        if (!isEntryMonth && !isWithdrawalMonth)
        {
            // all month days
            return Specification.SvMonthDays;
        }

        // entry month day
        var entryDay = !isEntryMonth
            // entry not in period
            ? Specification.SvMinMonthDays
            : period.IsLastDay(entryDate) ||
              // last day of february
              entryDate.IsLastDayOfMonth(Month.February, true)
                // round to unified month days
                ? Specification.SvMonthDays
                // calendar day
                : entryDate.Day;

        // withdrawal month day
        var withdrawalDay = !isWithdrawalMonth ||
                            withdrawalDate.Value.IsLastDayOfMonth(true)
            // entry not in period or round to unified month days
            ? Specification.SvMonthDays
            // calendar day
            : withdrawalDate.Value.Day;

        // multiple withdrawals and entries within month
        var isReentry = entryDay - withdrawalDay > 0;

        // withdrawal and reentry in same month
        if (isWithdrawalMonth && isEntryMonth && isReentry)
        {
            var employmentStatus = Function.GetPeriodCaseValue(period, CaseFieldName.EmployeeEmploymentStatus);
            var employmentDays = (int)employmentStatus.TotalDaysByValue(false);
            // TODO always months with 31 days?
            if (Function.Period.End.Day == 31 && employmentStatus == 1m)
            {
                employmentDays -= 1;
            }
            return employmentDays;
        }

        // SV days in month
        return withdrawalDay - entryDay + 1;
    }

    /// <summary>Get the total SV days between cycle start and period end</summary>
    /// <returns>Total SV days between cycle start and period end</returns>
    public int GetSvAccumulatedDaysYear2DateDays() =>
        GetSvAccumulatedDays(YearToDatePeriod);

    /// <summary>Get the total SV days in between two periods</summary>
    /// <param name="period">The date period</param>
    /// <returns>total SV days in between two periods</returns>
    public int GetSvAccumulatedDays(DatePeriod period)
    {
        var accumulatedSvDays = 0;
        var year = period.Start.Year;
        var month = period.Start.Month;
        var numPeriods = (period.End.Year - period.Start.Year) * Date.MonthsInYear + period.End.Month - period.Start.Month + 1;
        for (var periods = 1; periods <= numPeriods; periods++)
        {
            var periodStartDate = Date.MonthStart(year, month);
            var periodEndDate = Date.MonthEnd(year, month);
            var checkSvPeriod = new DatePeriod(periodStartDate, periodEndDate);
            accumulatedSvDays += GetSvMonthDays(checkSvPeriod);

            month += 1;
            if (month <= Date.MonthsInYear)
            {
                continue;
            }
            month = 1;
            year += 1;
        }
        return accumulatedSvDays;
    }

    /// <summary>Check if calculation is for back payment</summary>
    public virtual bool IsBackPayment()
    {
        var withdrawalDate = WithdrawalDate;
        return withdrawalDate.HasValue && withdrawalDate < Function.PeriodStart && !HasSvMonthDays();
    }

    /// <summary>Get employee child and education allowance wage type value</summary>
    public virtual decimal GetChildAllowance()
    {
        // no child allowance when employee is inactive
        if (!HasSvMonthDays())
        {
            return 0;
        }

        decimal result = 0;
        //calculate total child allowance & education allowance for all children
        foreach (var child in Children.Values)
        {
            var fakPeriod = child.GetFakPeriod();
            // current period must be within the FAK period
            if (fakPeriod == null || !fakPeriod.IsWithin(Function.Period))
            {
                continue;
            }
            // total child allowances
            result += child.ChildAllowance.Safe() + child.EducationAllowance.Safe();
        }
        return result.RoundTwentieth();
    }

    #endregion

    #region QST

    /// <summary>Get employee QST days</summary>
    /// <returns>The employee QST days for current period</returns>
    public int GetQstDays() =>
        GetQstDays(Function.Period);

    /// <summary>Get employee QST days</summary>
    /// <param name="period"></param>
    /// <returns>The employee QST days for given period</returns>
    public int GetQstDays(DatePeriod period)
    {
        // get qst canton by period
        var employeeQstTaxCanton = Function.GetPeriodCaseValue(period, CaseFieldName.EmployeeQstTaxCanton);
        if (!employeeQstTaxCanton.HasValue)
        {
            return 0;
        }
        // get qst cycle
        var qstCycle = GetQstCycle(employeeQstTaxCanton.Value.ToString());
        // return qst days based on cycle
        return qstCycle switch
        {
            //Qst yearly cycle
            QstCalculationCycle.Yearly => 360,
            //Qst monthly cycle
            QstCalculationCycle.Monthly => 30,
            _ => 0
        };
    }

    /// <summary>Check if employee has QST days</summary>
    /// <returns>True if days more than 0</returns>
    public bool HasQstDays() =>
        HasQstDays(Function.Period);

    /// <summary>Check if employee has QST days for given period</summary>
    /// <returns>True if days more than 0 for given period</returns>
    public bool HasQstDays(DatePeriod period)
    {
        return GetQstDays(period) > 0;
    }

    /// <summary>Get the employee effective workdays for current period</summary>
    /// <returns>The employee effective workdays for current period</returns>
    public int GetAccumulatedQstEffectiveWorkdays() =>
        GetAccumulatedQstEffectiveWorkdays(Function.Period);

    /// <summary>Get the employee effective workdays between given start date and end date</summary>
    /// <returns>The employee effective workdays between given start date and end date</returns>
    public int GetAccumulatedQstEffectiveWorkdays(DatePeriod period)
    {
        var numPeriods = (period.End.Year - period.Start.Year) * Date.MonthsInYear + period.End.Month - period.Start.Month + 1;
        var accumulatedQstEffectiveWorkDays = 0;
        var month = period.Start.Month;
        var year = period.Start.Year;
        for (var periods = 1; periods <= numPeriods; periods++)
        {
            var periodStartDate = Date.MonthStart(year, month);
            var periodEndDate = Date.MonthEnd(year, month);
            var checkPeriod = new DatePeriod(periodStartDate, periodEndDate);

            var qstEffectiveWorkDaysCaseValue = Function.GetPeriodCaseValue(checkPeriod, CaseFieldName.EmployeeQstEffectiveWorkDays);
            var qstEffectiveWorkDays = qstEffectiveWorkDaysCaseValue.HasValue ? (int)qstEffectiveWorkDaysCaseValue.Value : 0;

            accumulatedQstEffectiveWorkDays += qstEffectiveWorkDays;

            month += 1;
            if (month > Date.MonthsInYear)
            {
                month = 1;
                year += 1;
            }
        }
        return accumulatedQstEffectiveWorkDays;
    }

    /// <summary>Get the employee workdays in switzerland for current period</summary>
    /// <returns>The employee workdays in switzerland for current period</returns>
    public int GetAccumulatedQstWorkDaysSwitzerland() =>
        GetAccumulatedQstWorkDaysSwitzerland(Function.Period);

    /// <summary>Get the employee workdays in switzerland between given start date and end date</summary>
    /// <returns>The employee workdays in switzerland between given start date and end date</returns>
    public int GetAccumulatedQstWorkDaysSwitzerland(DatePeriod period)
    {
        var numPeriods = (period.End.Year - period.Start.Year) * Date.MonthsInYear + period.End.Month - period.Start.Month + 1;
        var accumulatedQstWorkDaysSwitzerland = 0;
        var month = period.Start.Month;
        var year = period.Start.Year;
        for (var periods = 1; periods <= numPeriods; periods++)
        {
            var periodStartDate = Date.MonthStart(year, month);
            var periodEndDate = Date.MonthEnd(year, month);
            var checkSvPeriod = new DatePeriod(periodStartDate, periodEndDate);

            var qstWorkDaysSwitzerlandCaseValue = Function.GetPeriodCaseValue(checkSvPeriod, CaseFieldName.EmployeeQstWorkDaysSwitzerland);
            var qstWorkDaysSwitzerland = qstWorkDaysSwitzerlandCaseValue.HasValue ? (int)qstWorkDaysSwitzerlandCaseValue.Value : 0;

            accumulatedQstWorkDaysSwitzerland += qstWorkDaysSwitzerland;

            month += 1;
            if (month > Date.MonthsInYear)
            {
                month = 1;
                year += 1;
            }
        }
        return accumulatedQstWorkDaysSwitzerland;
    }

    /// <summary>Get the employee withholding tax obligation status</summary>
    /// <returns>True if obligated, false if not obligated</returns>
    public bool IsQstObligated()
    {
        // not obligated if nationality is switzerland
        var nationality = Nationality;
        if (string.IsNullOrWhiteSpace(nationality) ||
            Specification.SwissCountryCode.Equals(nationality))
        {
            return false;
        }

        // not obligated if settled in switzerland
        var residenceCategory = ResidenceCategory;
        if (string.IsNullOrWhiteSpace(residenceCategory) ||
            Specification.ResidenceCategory.Equals(residenceCategory))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Get the QST cycle from the QST canton
    /// </summary>
    public QstCalculationCycle? GetQstCycle()
    {
        var canton = Qst.TaxCanton;
        return string.IsNullOrWhiteSpace(canton) ? null : GetQstCycle(canton);
    }

    /// <summary>
    /// Get the QST cycle
    /// </summary>
    public QstCalculationCycle? GetQstCycle(string canton)
    {
        if (string.IsNullOrWhiteSpace(canton))
        {
            throw new ArgumentException("Missing QST canton", nameof(canton));
        }

        // get canton lookup
        var cantonLookup = Function.GetLookup<Dictionary<string, object>>(LookupName.QstCantons, canton);
        if (cantonLookup == null || !cantonLookup.TryGetValue("CalculationCycle", out var cycle))
        {
            return null;
        }

        return cycle.ToString().ToQstCycle();
    }

    // wage type resultExpression (QST codes)
    /// <summary>Get the employee QST lookup item (code, canton and calculation type)</summary>
    public QstLookupItem GetQstLookupItem() =>
        GetQstLookupItem(Qst.TaxCode, Qst.TaxCanton);

    // wage type resultExpression (QST codes)
    /// <summary>Get the employee QST code, canton and calculation type</summary>
    /// <returns>A string array of QST code, canton and calculation type</returns>
    public QstLookupItem GetQstLookupItem(string qstCode, string qstCanton)
    {
        if (string.IsNullOrWhiteSpace(qstCanton) || string.IsNullOrWhiteSpace(qstCanton))
        {
            return null;
        }

        var cantonLookup = Function.GetLookup<Dictionary<string, object>>(LookupName.QstCantons, qstCanton);
        var qstCycle = cantonLookup["CalculationCycle"].ToString().ToQstCycle();

        return new()
        {
            QstCode = qstCode,
            QstCanton = qstCanton,
            QstCycle = qstCycle
        };
    }

    /// <summary>Get the QST tax percent</summary>
    /// <param name="qstPercentWage"></param>
    /// <param name="qstTaxCanton"></param>
    /// <param name="qstTaxCode"></param>
    /// <returns></returns>
    public virtual decimal GetQstTaxPercent(decimal qstPercentWage, string qstTaxCanton, string qstTaxCode)
    {
        // get qst canton lookup name
        var qstLookupName = Qst.GetQstLookupName(qstTaxCanton);

        // get qst lookup data from qst table by qstLookupName, qstPercentDefiningWage and currentQstCode
        var qstPercentLookup = Function.GetRangeLookup<Dictionary<string, object>>(
            qstLookupName, qstPercentWage, qstTaxCode);
        if (qstPercentLookup == null || !qstPercentLookup.ContainsKey(Qst.QstLookupTaxRate) ||
            !decimal.TryParse(qstPercentLookup[Qst.QstLookupTaxRate].ToString(),
                NumberStyles.Any, CultureInfo.InvariantCulture, out var qstPercent))
        {
            return 0;
        }
        return qstPercent;
    }

    #endregion

    #region Pension (AHV)

    /// <summary>Get the employee pension contribution status</summary>
    public PensionContributionStatus GetPensionContributionStatus() =>
        GetPensionContributionStatus(IsBackPayment() ? WithdrawalDate.Safe() : Function.PeriodEnd);

    /// <summary>Get the employee pension contribution status</summary>
    public PensionContributionStatus GetPensionContributionStatus(DateTime moment)
    {
        // pension status not insured (special case), employee value
        var specialCase = Ahv.SpecialCase;
        if (specialCase.HasValue && specialCase.Value)
        {
            return PensionContributionStatus.SpecialCase;
        }

        // pension status non obligatory (children), based on the age
        var dateOfBirth = BirthDate;
        if (!dateOfBirth.HasValue)
        {
            // missing base data
            return PensionContributionStatus.SpecialCase;
        }

        var contributionAge = National.Ahv.ContributionObligationAge;
        // always check if ContributionObligationAge is met at end of cycle, either at cycle end of moment or current cycle end
        var ageAtYearEnd = dateOfBirth.Value.Age(Function.GetCycle(moment).End);
        if (ageAtYearEnd < contributionAge)
        {
            return PensionContributionStatus.NonObligatory;
        }

        // pension status pensioner
        if (IsRetired(Function.GetPeriod(moment).Start).Safe())
        {
            return PensionContributionStatus.Pensioner;
        }

        // default is obligatory
        return PensionContributionStatus.Obligatory;
    }

    /// <summary>Get the employee pension month days <see cref="Specification.AhvRetirementMonthDays"/> or zero</summary>
    public int GetPensionMonthDays(DateTime? moment = null) =>
        IsRetired(moment).Safe() ? Specification.AhvRetirementMonthDays : 0;

    /// <summary>Get the employee pension accumulated days from cycle start until period end</summary>
    public int GetPensionAccumulatedDays() =>
        GetPensionAccumulatedDays(YearToDatePeriod);

    /// <summary>Get the employee pension accumulated days from cycle start until period end</summary>
    public int GetPensionAccumulatedDays(DatePeriod period)
    {
        var numPeriods = (period.End.Year - period.Start.Year) * Date.MonthsInYear + period.End.Month - period.Start.Month + 1;
        var accumulatedPensionDays = 0;
        var month = period.Start.Month;
        var year = period.Start.Year;
        for (var periods = 1; periods <= numPeriods; periods++)
        {
            var periodStartDate = Date.MonthStart(year, month);
            accumulatedPensionDays += GetPensionMonthDays(periodStartDate);

            month += 1;
            if (month > Date.MonthsInYear)
            {
                month = 1;
                year += 1;
            }
        }
        return accumulatedPensionDays;
    }

    /// <summary>Test if employee is retired</summary>
    /// <returns>True if retired, null on missing gender, age or retirement data</returns>
    public bool? IsRetired(DateTime? moment = null)
    {
        var age = GetAge(moment);
        return age.HasValue ? IsRetired(age.Value) : false;
    }

    /// <summary>Test if employee is retired</summary>
    /// <returns>True if retired, null on missing gender, age or retirement data</returns>
    public bool? IsRetired(int age)
    {
        if (age <= 0)
        {
            return null;
        }

        // gender
        var gender = GetGender();
        if (!gender.HasValue)
        {
            return null;
        }

        // retirement age
        int retirementAge;
        switch (gender.Value)
        {
            case Gender.Female:
                retirementAge = National.Ahv.RetirementAgeFemale;
                break;
            case Gender.Male:
                retirementAge = National.Ahv.RetirementAgeMale;
                break;
            default:
                return null;
        }

        // retired check
        var retired = age >= retirementAge;
        return retired;
    }

    #endregion

    #region Entry and Exit

    /// <summary>The employee status off</summary>
    public const decimal EmployedStatusOff = 0;

    /// <summary>The employee status on</summary>
    public const decimal EmployedStatusOn = 1;

    /// <summary>Test if employee is currently employed</summary>
    public bool IsEmployed =>
        EmploymentStatus.Safe() == EmployedStatusOn;

    /// <summary>Test if employee can entry</summary>
    public bool EmployeeEntryAvailable()
    {
        // initial entry
        var entryDate = EntryDate;
        Function.LogInformation($"entry date: {entryDate}");
        if (!entryDate.HasValue)
        {
            return true;
        }

        // withdrawal re-entry
        var withdrawalDate = WithdrawalDate;
        Function.LogInformation($"withdrawal date: {withdrawalDate}");
        return withdrawalDate.HasValue && entryDate < withdrawalDate;
    }

    /// <summary>Test if employee can exit</summary>
    public bool EmployeeExitAvailable()
    {
        var entryDate = EntryDate;
        // not employed
        if (!entryDate.HasValue)
        {
            return false;
        }

        var withdrawalDate = WithdrawalDate;
        // no withdrawal
        if (!withdrawalDate.HasValue)
        {
            return true;
        }
        // active employee
        return entryDate > withdrawalDate;
    }

    #endregion

}
