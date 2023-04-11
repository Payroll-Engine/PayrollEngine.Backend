using System.Linq;
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec wage type Qst</summary>
public class WageTypeQst<TNational, TCompany, TEmployee> :
    WageTypeToolBase<TNational, TCompany, TEmployee>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
{
    private const string QstPercentDefiningWageAttribute = "QstPercentDefiningWage";

    /// <summary>Wage type Qst constructor</summary>
    public WageTypeQst(WageTypeFunction function, TEmployee employee) :
        base(function, employee)
    {
    }

    #region Wage

    /// <summary>Wage type QST wage</summary>
    /// <remarks>Used by wage type 5007.3</remarks>
    public virtual decimal GetWage()
    {
        // check if qst collector is 0
        if (Collector[CollectorName.QstBase] == 0)
        {
            return 0;
        }

        // check if obligated to pay tax
        if (!Employee.IsQstObligated())
        {
            return 0;
        }

        // qst cycle
        return Employee.GetQstCycle() switch
        {
            QstCalculationCycle.Yearly => GetYearWage(),
            QstCalculationCycle.Monthly => GetMonthWage(),
            _ => 0
        };
    }

    /// <summary>Calculate the year wage, respecting the 13th monthly wage</summary>
    /// <returns>The year wage</returns>
    protected virtual decimal GetYearWage()
    {
        var qstpCollector = Collector[CollectorName.QstpBase];
        var qstaCollector = Collector[CollectorName.QstaBase];

        var effectiveWorkdays = Function.GetPeriodCaseValue<int>(Function.Period, CaseFieldName.EmployeeQstEffectiveWorkDays);
        var workdaysSwitzerland = Function.GetPeriodCaseValue<int>(Function.Period, CaseFieldName.EmployeeQstWorkDaysSwitzerland);

        var isBackPayment = Employee.IsBackPayment();
        var withdrawalCycle = Employee.WithdrawalCycle;

        var effectiveWorkDaysY2D = isBackPayment ?
            // total effective workdays when back payment
            Employee.GetAccumulatedQstEffectiveWorkdays(withdrawalCycle) :
            // total effective workdays
            Employee.GetAccumulatedQstEffectiveWorkdays(Employee.YearToDatePeriod);

        var workdaysSwitzerlandY2D = isBackPayment ?
            // total workdays switzerland when back payment
            Employee.GetAccumulatedQstWorkDaysSwitzerland(withdrawalCycle) :
            // total workdays switzerland
            Employee.GetAccumulatedQstWorkDaysSwitzerland(Employee.YearToDatePeriod);

        // special retro case (retro into previous cycle)
        if (Function.IsRetroPayrun != Function.IsCycleRetroPayrun)
        {
            // 13th month wage
            var periodMonth13thWage = Function.GetWageTypeResults(
                new WageTypeRangeResultQuery(WageTypeNumber.MonthlyWage13th, Employee.YearToDatePeriod.Start,
                    Employee.YearToDatePeriod.End)).Sum();
            var month13thWage = WageType[WageTypeNumber.MonthlyWage13th];
            // deduct 13th month wage from qst periodical collector
            if (month13thWage > 0)
            {
                qstpCollector -= periodMonth13thWage;
            }
            // extrapolate periodical collector (collector must be 0 when workdaysSwitzerland 0)
            if (effectiveWorkdays > 0)
            {
                qstpCollector = qstpCollector / effectiveWorkdays * workdaysSwitzerland;
            }
            // add 13th month wage to non periodical collector
            qstaCollector += periodMonth13thWage;
            // extrapolate non periodical collector
            if (effectiveWorkDaysY2D > 0 && workdaysSwitzerlandY2D > 0)
            {
                qstaCollector = qstaCollector / effectiveWorkDaysY2D * workdaysSwitzerlandY2D;
            }
        }
        // regular calculation and retro within cycle
        else
        {
            // 13th month wage
            var month13thWage = WageType[WageTypeNumber.MonthlyWage13th];
            // deduct 13th month wage from periodical collector
            if (month13thWage > 0 && effectiveWorkdays > 0)
            {
                qstpCollector -= month13thWage;
            }
            // extrapolate periodical collector (collector must be 0 when workdaysSwitzerland 0)
            if (effectiveWorkdays > 0)
            {
                qstpCollector = qstpCollector / effectiveWorkdays * workdaysSwitzerland;
            }
            // extrapolate non periodical collector
            if (effectiveWorkDaysY2D > 0 && workdaysSwitzerlandY2D > 0)
            {
                // extrapolate and add 13th month wage to non periodical collector
                month13thWage = month13thWage / effectiveWorkDaysY2D * workdaysSwitzerlandY2D;
                qstaCollector = qstaCollector / effectiveWorkDaysY2D * workdaysSwitzerlandY2D + month13thWage;
            }
        }

        return (qstpCollector + qstaCollector).RoundTwentieth();
    }

    /// <summary>Calculate the month wage, respecting the 13th monthly wage</summary>
    /// <returns>The month wage</returns>
    protected virtual decimal GetMonthWage()
    {
        // QST non periodical wage
        var monthlyWage13thWage = WageType[WageTypeNumber.MonthlyWage13th];
        var effectiveWorkDaysY2D = Employee.GetAccumulatedQstEffectiveWorkdays(Employee.YearToDatePeriod);
        var workdaysSwitzerlandY2D = Employee.GetAccumulatedQstWorkDaysSwitzerland(Employee.YearToDatePeriod);
        var qstaBaseCollector = Collector[CollectorName.QstaBase];
        var qstpBaseCollector = Collector[CollectorName.QstpBase];
        var qstNonPeriodicalWage = qstaBaseCollector;
        if (effectiveWorkDaysY2D > 0 && workdaysSwitzerlandY2D > 0)
        {
            qstNonPeriodicalWage = qstaBaseCollector / effectiveWorkDaysY2D * workdaysSwitzerlandY2D;
        }

        // QST periodical wage
        var qstWorkdays = Employee.GetAccumulatedQstEffectiveWorkdays();
        var qstSwitzerlandWorkdays = Employee.GetAccumulatedQstWorkDaysSwitzerland();
        if (qstWorkdays > 0 && qstSwitzerlandWorkdays > 0 && monthlyWage13thWage > 0)
        {
            var qstPeriodicalWage = qstpBaseCollector - monthlyWage13thWage;
            var qstPeriodicalWageWithout13th = qstPeriodicalWage / qstWorkdays * qstSwitzerlandWorkdays;
            var qstPeriodicalWageMonthly13th = monthlyWage13thWage / effectiveWorkDaysY2D * workdaysSwitzerlandY2D;
            return (qstPeriodicalWageWithout13th + qstPeriodicalWageMonthly13th + qstNonPeriodicalWage).RoundTwentieth();
        }

        // scale wage to workdays
        if (qstWorkdays > 0)
        {
            return (qstpBaseCollector / qstWorkdays * qstSwitzerlandWorkdays + qstNonPeriodicalWage).RoundTwentieth();
        }

        // withdrawal wage
        var qstBaseCollector = Collector[CollectorName.QstBase];
        var withdrawalCycle = Employee.WithdrawalCycle;
        var qstWithdrawalWorkdays = Employee.GetAccumulatedQstEffectiveWorkdays(withdrawalCycle);
        var qstWithdrawalSwitzerlandWorkdays = Employee.GetAccumulatedQstWorkDaysSwitzerland(withdrawalCycle);
        // scale withdrawal wage to workdays
        if (Employee.IsBackPayment() && qstWithdrawalWorkdays > 0)
        {
            return (qstBaseCollector / qstWithdrawalWorkdays * qstWithdrawalSwitzerlandWorkdays).RoundTwentieth();
        }
        return (qstpBaseCollector + qstNonPeriodicalWage).RoundTwentieth();
    }

    /// <summary>Set employee QST result</summary>
    public void SetWageResult()
    {
        var qstLookupItem = Employee.GetQstLookupItem();
        if (qstLookupItem != null)
        {
            Function.SetResultTags(qstLookupItem.GetResultTags());
        }
    }

    #endregion

    #region Periodical wage

    /// <summary>Wage type QST periodical wage</summary>
    /// <remarks>Used by wage type 5007.1</remarks>
    public virtual decimal GetPeriodicalWage()
    {
        // check if qst periodical base is 0
        if (Collector[CollectorName.QstpBase] == 0)
        {
            return 0;
        }

        // check if obligated to pay tax
        if (!Employee.IsQstObligated())
        {
            return 0;
        }

        // qst cycle
        return Employee.GetQstCycle() switch
        {
            QstCalculationCycle.Yearly => GetPeriodicalYearWage(),
            QstCalculationCycle.Monthly => GetPeriodicalMonthWage(),
            _ => 0
        };
    }

    /// <summary>Wage type QST periodical year wage</summary>
    protected virtual decimal GetPeriodicalYearWage()
    {
        decimal result = 0;
        var qstpBaseCollector = Collector[CollectorName.QstpBase];

        if (Function.IsRetroPayrun)
        {
            var withdrawalPeriod = Employee.WithdrawalPeriod;
            var workdays = Function.GetPeriodCaseValue<int>(
                Function.Period, CaseFieldName.EmployeeQstEffectiveWorkDays);
            // no 13th wage in periodical when retro
            var monthlyWage13th = WageType[WageTypeNumber.MonthlyWage13th];
            if (monthlyWage13th > 0)
            {
                qstpBaseCollector -= monthlyWage13th;
            }

            if (workdays > 0)
            {
                var workdaysSwitzerland = Function.GetPeriodCaseValue<int>(withdrawalPeriod,
                    CaseFieldName.EmployeeQstWorkDaysSwitzerland);
                if (workdaysSwitzerland > 0)
                {
                    result = qstpBaseCollector / workdays * workdaysSwitzerland;
                }
            }
            if (workdays == 0)
            {
                result = qstpBaseCollector;
            }
        }
        else
        {
            var tags = Employee.GetQstLookupItem()?.GetResultTags();
            var qstpBaseConsCollector = qstpBaseCollector + GetConsolidatedCollectorResults(
                NewCollectorCycleQuery(CollectorName.QstpBase, tags)).Sum();
            var qstpConsWage = GetConsolidatedWageTypeResults(
                NewWageTypeCycleQuery(WageTypeNumber.QstPeriodicalWage, tags)).Sum();
            result = qstpBaseConsCollector - qstpConsWage;
        }

        return result.RoundTwentieth();
    }

    /// <summary>Wage type QST periodical month wage</summary>
    protected virtual decimal GetPeriodicalMonthWage() =>
        Collector[CollectorName.QstpBase].RoundTwentieth();

    #endregion

    #region Non periodical wage

    /// <summary>Wage type QST non periodical wage</summary>
    /// <remarks>Used by wage type 5007.2</remarks>
    public virtual decimal GetNonPeriodicalWage()
    {
        // withdrawal tax for non periodical base must be greater than 0
        var qstaBaseCollector = Collector[CollectorName.QstaBase];
        if (qstaBaseCollector == 0)
        {
            return 0;
        }

        // check if obligated to pay tax
        if (!Employee.IsQstObligated())
        {
            return 0;
        }

        // tax code is defined
        var qstTaxCode = Function.GetPeriodCaseValue(Function.Period, CaseFieldName.EmployeeQstTaxCode);
        if (!qstTaxCode.HasValue)
        {
            return 0;
        }

        // qst cycle
        return Employee.GetQstCycle() switch
        {
            null => 0,
            QstCalculationCycle.Yearly => GetNonPeriodicalYearWage(),
            QstCalculationCycle.Monthly => GetNonPeriodicalMonthWage(),
            _ => 0
        };
    }

    /// <summary>Wage type QST non periodical year wage</summary>
    protected virtual decimal GetNonPeriodicalYearWage()
    {
        var qstaBaseCollector = Collector[CollectorName.QstaBase];
        // in retro payrun non periodical wage is equal to non periodical base
        if (!Function.IsRetroPayrun)
        {
            var withdrawalCycle = Employee.WithdrawalCycle;
            var workdays = Employee.GetAccumulatedQstEffectiveWorkdays(withdrawalCycle);
            if (workdays > 0)
            {
                var workdaysSwitzerland = Employee.GetAccumulatedQstWorkDaysSwitzerland(withdrawalCycle);
                if (workdaysSwitzerland > 0)
                {
                    qstaBaseCollector = qstaBaseCollector / workdays * workdaysSwitzerland;
                }
            }
        }

        // retro payrun into previous cycle
        if (Function.IsRetroPayrun && !Function.IsCycleRetroPayrun)
        {
            var withdrawalCycle = Employee.WithdrawalCycle;
            var withdrawalPeriod = Employee.WithdrawalPeriod;
            var monthlyWage13th = Function.GetWageTypeResults(new[] { WageTypeNumber.MonthlyWage13th },
                withdrawalPeriod.Start, withdrawalPeriod.End).Take(1).Sum();
            var workdays = Employee.GetAccumulatedQstEffectiveWorkdays(withdrawalCycle);
            if (workdays > 0)
            {
                var workdaysSwitzerland = Employee.GetAccumulatedQstWorkDaysSwitzerland(withdrawalCycle);
                if (workdaysSwitzerland > 0)
                {
                    qstaBaseCollector = (qstaBaseCollector + monthlyWage13th) / workdays * workdaysSwitzerland;
                }
            }
        }

        return qstaBaseCollector.RoundTwentieth();
    }

    /// <summary>Wage type QST non periodical month wage</summary>
    protected virtual decimal GetNonPeriodicalMonthWage()
    {
        var qstaBaseCollector = Collector[CollectorName.QstaBase];
        // in retro payrun non periodical wage is equal to non periodical base
        if (!Function.IsRetroPayrun && !Employee.IsBackPayment())
        {
            var withdrawalCycle = Employee.WithdrawalCycle;
            var workdays = Employee.GetAccumulatedQstEffectiveWorkdays(withdrawalCycle);
            if (workdays > 0)
            {
                var workdaysSwitzerland = Employee.GetAccumulatedQstWorkDaysSwitzerland(withdrawalCycle);
                if (workdaysSwitzerland > 0)
                {
                    qstaBaseCollector = qstaBaseCollector / workdays * workdaysSwitzerland;
                }
            }
        }

        return qstaBaseCollector.RoundTwentieth();
    }

    #endregion

    #region Percent definining wage

    /// <summary>Wage type QST non periodical percent defining wage</summary>
    /// <remarks>Used by wage type 5008.2</remarks>
    public virtual decimal GetNonPeriodicalPercentDefiningWage()
    {
        // check if non periodical qst base is 0
        var qstaBaseCollector = Collector[CollectorName.QstaBase];
        if (qstaBaseCollector == 0)
        {
            return 0;
        }
        // check if obligated to pay tax
        if (!Employee.IsQstObligated())
        {
            return 0;
        }
        return qstaBaseCollector;
    }

    /// <summary>Wage type QST periodical percent defining wage</summary>
    /// <remarks>Used by wage type 5008.1</remarks>
    public virtual decimal GetPeriodicalPercentDefiningWage()
    {
        // check if qst periodical collector is 0
        if (Collector[CollectorName.QstpBase] == 0)
        {
            return 0;
        }

        // check if obligated to pay tax
        if (!Employee.IsQstObligated())
        {
            return 0;
        }

        // qst cycle
        return Employee.GetQstCycle() switch
        {
            QstCalculationCycle.Yearly => GetPeriodicalPercentDefiningYearWage(),
            QstCalculationCycle.Monthly => GetPeriodicalPercentDefiningMonthWage(),
            _ => 0
        };
    }

    /// <summary>Wage type QST periodical percent defining year wage</summary>
    protected virtual decimal GetPeriodicalPercentDefiningYearWage()
    {
        // yearly calculation - median is set
        if (Employee.Qst.Median.Safe())
        {
            // TODO: median is set logic - no swissdec relevance
            return 0;
        }

        // yearly calculation - payment after withdrawal
        if (Employee.IsBackPayment())
        {
            // TODO - build logic for yearly payment after withdrawal
            return 0;
        }

        var qstpCollector = Collector[CollectorName.QstpBase];

        var wageData = GetPeriodicalWageData();

        // active whole month and activity rate 100%
        if ((wageData.IsFullSvMonth && wageData.IsFullActivityRate) ||
            // no effective days, no switzerland days and no activity rates defined
            (!wageData.IsFullActivityRate && !wageData.HasActivityRate && !wageData.HasOtherActivityRate) ||
            // entry or withdrawal within month and activity rate 100%
            (wageData.IsFullActivityRate && (wageData.IsWithdrawalMonth || wageData.IsEntryMonth)))
        {
            var result = qstpCollector.RoundTwentieth();
            return result;
        }

        // entry or withdrawal within month
        if (wageData.HasActivityRate && wageData.HasOtherActivityRate &&
            (wageData.IsEntryMonth || wageData.IsWithdrawalMonth))
        {
            var result = (qstpCollector / wageData.ActivityRatePercent *
                           (wageData.ActivityRatePercent + wageData.OtherActivityPercent)).RoundTwentieth();
            return result;
        }

        // activity rate < 100% and other activity rate = 0%
        if (wageData.HasActivityRate && !wageData.HasOtherActivityRate)
        {
            var result = qstpCollector.RoundTwentieth();
            return result;
        }

        // activity rate < 100% and other activity rate > 0%
        if (wageData.HasActivityRate)
        {
            var result = (qstpCollector / wageData.ActivityRatePercent *
                           (wageData.ActivityRatePercent + wageData.OtherActivityPercent)).RoundTwentieth();
            return result;
        }

        // activity rate = 100%
        if (wageData.IsFullActivityRate)
        {
            var result = qstpCollector.RoundTwentieth();
            if (!Function.IsRetroPayrun)
            {
                Function.SetPayrunJobAttribute(QstPercentDefiningWageAttribute, result);
            }
            return result;
        }

        return 0;
    }

    /// <summary>Wage type QST periodical percent defining month wage</summary>
    protected virtual decimal GetPeriodicalPercentDefiningMonthWage()
    {
        // check if 0 is done in GetPeriodicalPercentDefiningWage()
        var qstpBaseCollector = Collector[CollectorName.QstpBase];

        // median is set
        if (Employee.Qst.Median.Safe())
        {
            return 0;
        }

        // payment after withdrawal
        if (Employee.IsBackPayment())
        {
            // TODO - build logic for monthly payment after withdrawal
            return 0;
        }

        var wageData = GetPeriodicalWageData();

        // withdrawal or entry within month, activity rate < 100% and other activity rate > 0%
        if (wageData.HasActivityRate && wageData.HasOtherActivityRate && wageData.IsWithdrawalMonth)
        {
            if (wageData.QstDays == 0 || wageData.SvMonthDays == 0)
            {
                return 0;
            }
            return (qstpBaseCollector / wageData.ActivityRatePercent *
                    (wageData.ActivityRatePercent + wageData.OtherActivityPercent) *
                    ((decimal)wageData.QstDays / wageData.SvMonthDays)).RoundTwentieth();
        }

        // withdrawal or entry within month and activity rate 100%
        if (wageData.IsFullActivityRate && (wageData.IsEntryMonth || wageData.IsWithdrawalMonth))
        {
            return (qstpBaseCollector * wageData.QstDays / wageData.SvMonthDays).RoundTwentieth();
        }

        // activity rate > 0%, activity rate < 100% and other activity rate > 0%
        if (wageData.HasActivityRate && wageData.HasOtherActivityRate)
        {
            return (qstpBaseCollector / wageData.ActivityRatePercent *
                    (wageData.ActivityRatePercent + wageData.OtherActivityPercent)).RoundTwentieth();
        }

        // activity rate 100%
        if ((wageData.IsFullActivityRate) ||
            // ???
            (wageData.QstDays > 0 &&
             Employee.GetAccumulatedQstEffectiveWorkdays() == 0 &&
             Employee.GetAccumulatedQstWorkDaysSwitzerland() == 0))
        {
            return qstpBaseCollector.RoundTwentieth();
        }

        return 0;
    }

    private PeriodicalWageData GetPeriodicalWageData()
    {
        var wageData = new PeriodicalWageData
        {
            // TODO: check qstDays > 0?
            QstDays = Employee.GetQstDays(),
            SvMonthDays = Employee.GetSvMonthDays(),
            EntryDate = Function.GetPeriodCaseValue(Function.Period, CaseFieldName.EmployeeEntryDate),
            WithdrawalDate = Function.GetPeriodCaseValue(Function.Period, CaseFieldName.EmployeeWithdrawalDate),
            ActivityRatePercent = Function.GetPeriodCaseValue(Function.Period, CaseFieldName.EmployeeActivityRatePercent),
            OtherActivityPercent = Function.GetPeriodCaseValue(Function.Period, CaseFieldName.EmployeeQstOtherActivities)
        };

        wageData.IsFullSvMonth = wageData.SvMonthDays == 30;
        wageData.IsEntryMonth = wageData.EntryDate.HasValue && wageData.EntryDate >
            Function.PeriodStart && wageData.EntryDate < Function.PeriodEnd;
        wageData.IsWithdrawalMonth = wageData.WithdrawalDate.HasValue && wageData.WithdrawalDate >
            Function.PeriodStart && wageData.WithdrawalDate < Function.PeriodEnd;
        wageData.IsFullActivityRate = wageData.ActivityRatePercent == 1;
        wageData.HasActivityRate = wageData.ActivityRatePercent > 0 && wageData.ActivityRatePercent < 1;
        wageData.HasOtherActivityRate = wageData.OtherActivityPercent > 0;

        return wageData;
    }

    /// <exclude />
    private sealed class PeriodicalWageData
    {
        internal int QstDays { get; init; }
        internal int SvMonthDays { get; init; }
        internal bool IsFullSvMonth { get; set; }

        internal CasePayrollValue EntryDate { get; init; }
        internal bool IsEntryMonth { get; set; }

        internal CasePayrollValue WithdrawalDate { get; init; }
        internal bool IsWithdrawalMonth { get; set; }

        internal decimal ActivityRatePercent { get; init; }
        internal bool IsFullActivityRate { get; set; }
        internal bool HasActivityRate { get; set; }

        internal decimal OtherActivityPercent { get; init; }
        internal bool HasOtherActivityRate { get; set; }
    }

    #endregion

    #region Percent definining wage

    /// <summary>Wage type QST percent defining wage</summary>
    /// <remarks>Used by wage type 5008.3</remarks>
    public virtual decimal GetPercentDefiningWage()
    {
        // check if qst base is 0
        if (Collector[CollectorName.QstBase] == 0)
        {
            return 0;
        }
        // check if obligated to pay tax
        if (!Employee.IsQstObligated())
        {
            return 0;
        }

        // qst cycle
        return Employee.GetQstCycle() switch
        {
            QstCalculationCycle.Yearly => GetPercentDefiningYearWage(),
            QstCalculationCycle.Monthly => GetPercentDefiningMonthWage(),
            _ => 0
        };
    }

    /// <summary>Wage type QST percent defining year wage</summary>
    protected virtual decimal GetPercentDefiningYearWage()
    {
        // filter tags
        var tags = Employee.GetQstLookupItem()?.GetResultFilterTags();
        // current tax canton
        var qstTaxCanton = Employee.Qst.TaxCanton;
        // tax canton period start
        var taxCanton = Function.GetPeriodCaseValue(Employee.YearToDatePeriod, CaseFieldName.EmployeeQstTaxCanton).Where(
                            x => x.HasValue && x.ValueAs<string>() == qstTaxCanton).GetPeriodStart();

        var withdrawalCycle = Employee.WithdrawalCycle;
        var isBackPayment = Employee.IsBackPayment();
        // consolidated wages accumulation period
        var accumulationPeriod = isBackPayment ? withdrawalCycle : Function.Cycle;
        // consolidated qst percent defining wages
        var qstConsWages = GetConsolidatedWageTypeResults(
            NewWageTypeQuery(
                new[]
                {
                    WageTypeNumber.QstPeriodicalPercentDefiningWage,
                    WageTypeNumber.QstNonPeriodicalPercentDefiningWage
                }, accumulationPeriod.Start, tags));
        // qst periodical percent defining wage
        var qstpWage = qstConsWages.Where(x => x.WageTypeNumber == WageTypeNumber.QstPeriodicalPercentDefiningWage).Sum() +
                       WageType[WageTypeNumber.QstPeriodicalPercentDefiningWage];
        // qst non periodical percent defining wage
        var qstaWage = isBackPayment ?
            // qst non periodical percent defining wage when back payment
            WageType[WageTypeNumber.QstNonPeriodicalPercentDefiningWage] :
            // qst non periodical percent defining regular wage
            qstConsWages.Where(x => x.WageTypeNumber == WageTypeNumber.QstNonPeriodicalPercentDefiningWage).Sum() +
            WageType[WageTypeNumber.QstNonPeriodicalPercentDefiningWage];

        // initial sv days accumulation period
        var svPeriod = Employee.YearToDatePeriod;
        // tax canton has start sv days
        if (taxCanton.HasValue)
        {
            svPeriod = new(taxCanton.Value, Function.PeriodEnd);
        }
        // sv days accumulation period when withdrawal cycle before calculation cycle
        if (isBackPayment && withdrawalCycle.End.IsBefore(Function.Cycle))
        {
            svPeriod = withdrawalCycle;
        }
        // sv days accumulation period when withdrawal in same cycle and tax canton has start date
        if (isBackPayment && withdrawalCycle == Function.Cycle && taxCanton.HasValue)
        {
            svPeriod = new(taxCanton.Value, Employee.WithdrawalPeriod.End);
        }
        // total sv days from accumulation cycle
        var svYearToDateDays = Employee.GetSvAccumulatedDays(svPeriod);

        var wageData = GetPeriodicalWageData();

        var result = ((qstpWage / svYearToDateDays * wageData.QstDays + qstaWage) / Date.MonthsInYear).RoundTwentieth();
        // set percent defining wage attribute
        if (!Function.IsRetroPayrun)
        {
            Function.SetPayrunJobAttribute(QstPercentDefiningWageAttribute, result);
        }

        return result;
    }

    /// <summary>Wage type QST percent defining month wage</summary>
    protected virtual decimal GetPercentDefiningMonthWage()
    {
        if (Employee.IsBackPayment())
        {
            var withdrawalPeriod = Function.GetPeriod(Employee.WithdrawalDate.Safe());
            var withdrawalWage = GetConsolidatedWageTypeResults(
                NewWageTypeQuery(WageTypeNumber.QstPeriodicalPercentDefiningWage, withdrawalPeriod.Start,
                    tags: new() { QstCalculationCycle.Monthly.ToQstCycleCode() })).Sum();

            return (withdrawalWage +
                    WageType[WageTypeNumber.QstPeriodicalWage] +
                    WageType[WageTypeNumber.QstNonPeriodicalWage]).RoundTwentieth();
        }

        return (WageType[WageTypeNumber.QstPeriodicalPercentDefiningWage] +
                WageType[WageTypeNumber.QstNonPeriodicalPercentDefiningWage]).RoundTwentieth();
    }

    #endregion

    #region Contribution wage

    /// <summary>Get wage type QST contribution wage</summary>
    /// <remarks>Used by wage type 5060</remarks>
    public virtual decimal GetContribution()
    {
        // check if qst base is 0
        if (Collector[CollectorName.QstBase] == 0)
        {
            return 0;
        }
        // check if obligated to pay tax
        if (!Employee.IsQstObligated())
        {
            return 0;
        }

        // qst code
        var qstCode = Function.GetPeriodCaseValue<string>(Function.Period, CaseFieldName.EmployeeQstTaxCode) ??
                      Function.GetPeriodCaseValue(Function.PreviousCycle, CaseFieldName.EmployeeQstTaxCode);
        if (qstCode == null)
        {
            return 0;
        }

        // qst cycle
        return Employee.GetQstCycle() switch
        {
            QstCalculationCycle.Yearly => GetContributionYearWage(qstCode),
            QstCalculationCycle.Monthly => GetContributionMonthWage(qstCode),
            _ => 0
        };
    }

    /// <summary>Get wage type QST contribution year wage</summary>
    protected virtual decimal GetContributionYearWage(string qstCode)
    {
        // percent defining wage
        var qstDefiningWage = Function.IsRetroPayrun && Function.IsCycleRetroPayrun ?
            Function.GetPayrunJobAttribute<decimal>(QstPercentDefiningWageAttribute) :
            WageType[WageTypeNumber.QstPercentDefiningWage];
        if (qstDefiningWage == 0)
        {
            Function.LogWarning("QST get contribution year wage: missing percent defining wage");
            return 0;
        }

        // period qst wage
        var currentPeriodTaxWage = WageType[WageTypeNumber.QstWage];
        // period tax canton
        var qstTaxCanton = Employee.Qst.TaxCanton;
        // all qst codes from cycle start to period end
        var qstTaxCodes = Function.GetPeriodCaseValue(Employee.YearToDatePeriod, CaseFieldName.EmployeeQstTaxCode).
            OrderBy(x => x.Period.Start);
        // tags without qst code
        var tags = Employee.GetQstLookupItem()?.GetResultFilterTags();
        // wage type result query
        var wageTypeResults = GetConsolidatedWageTypeResults(
            NewWageTypeCycleQuery(new[]
            {
                WageTypeNumber.QstWage,
                WageTypeNumber.WithholdingTaxDeduction
            }, tags));

        // check if back payment
        var isBackPayment = Employee.IsBackPayment();
        // total tax deduction until period start
        var yearToDateTaxDeduction = isBackPayment ?
                // year to date tax deduction will be recalculated when back payment
                0 :
                // regular year to date tax deduction
                wageTypeResults.Where(x => x.WageTypeNumber == WageTypeNumber.WithholdingTaxDeduction).Sum();

        // start period tax deduction at 0
        decimal totalTaxDeduction = 0;
        // calculate deduction with compensation from earlier periods
        foreach (var qstTaxCode in qstTaxCodes)
        {
            // qst percent
            var qstPercent = Employee.GetQstTaxPercent(qstDefiningWage, qstTaxCanton, qstTaxCode.ValueAs<string>());
            if (qstPercent == 0)
            {
                continue;
            }
            // qstTaxCode valid period
            var qstCodePeriod = qstTaxCode.Period;
            // result consolidation periods count
            var takePeriodsMin = qstCodePeriod.Start.Month - 1;
            var takePeriodsCount = qstCodePeriod.End.Year > Function.CycleEnd.Year ?
                // consolidated results count when qst code changes within cycle
                Date.MonthsInYear - takePeriodsMin :
                // consolidated results count when no qst code change within cycle
                qstCodePeriod.End.Month - 1;
            // tags with current code
            var codeTags = new QstLookupItem
            {
                QstCode = qstTaxCode.Value.ToString(),
                QstCanton = qstTaxCanton,
                QstCycle = QstCalculationCycle.Yearly
            }.GetResultTags();

            // current tax wage when current code equals valid code (qstTaxCode == qstCode)
            var taxWage = string.Equals(qstCode, qstTaxCode.Value.ToString()) ? currentPeriodTaxWage : 0;

            // consolidated wage results
            var consResultWage = wageTypeResults.Where(x =>
                                     x.WageTypeNumber == WageTypeNumber.QstWage &&
                                     codeTags.ContainsAll(x.Tags)).Take(takePeriodsCount).Sum();

            // when back payment recalculate year to date tax deduction for each code with new qst percent
            if (isBackPayment)
            {
                yearToDateTaxDeduction += (consResultWage * qstPercent).RoundTwentieth();
            }

            // qst code tax deduction
            var taxDeduction = (consResultWage + taxWage) * qstPercent;
            // add qst code tax deduction to total deduction
            totalTaxDeduction += taxDeduction.RoundTwentieth();
        }
        // tax deduction for current period
        var result = (totalTaxDeduction - yearToDateTaxDeduction).RoundTwentieth();
        return result;
    }

    /// <summary>Get wage type QST contribution month wage</summary>
    protected virtual decimal GetContributionMonthWage(string qstCode)
    {
        // percent defining wage
        var qstPercentWage = WageType[WageTypeNumber.QstPercentDefiningWage];
        if (qstPercentWage == 0)
        {
            return 0;
        }
        var qstPercent = Employee.GetQstTaxPercent(qstPercentWage, Employee.Qst.TaxCanton, qstCode);
        if (qstPercent == 0)
        {
            return 0;
        }
        var result = (WageType[WageTypeNumber.QstWage] * qstPercent).RoundTwentieth();
        return result;
    }

    #endregion

}
