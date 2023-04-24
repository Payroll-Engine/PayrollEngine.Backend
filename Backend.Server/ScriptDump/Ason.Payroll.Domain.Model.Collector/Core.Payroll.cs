/* Core.Payroll */

namespace Ason.Regulation.Swissdec5;

using Payroll.Client.Scripting.Function;
using System.Runtime.CompilerServices;
using System;
using EmployeeType = Employee<National, Company<National>>;
using EmployeeStatisticsType = EmployeeStatistics;
using EmployeeActivityType = EmployeeActivity;
using EmployeeKtgInsuranceType = EmployeeKtgInsurance;
using EmployeeUvgInsuranceType = EmployeeUvgInsurance;
using EmployeeUvgzInsuranceType = EmployeeUvgzInsurance;
using EmployeeQstType = EmployeeQst;

#region Case field names

/// <summary>Swissdec case field name</summary>
public static class CaseFieldName
{
    private static string EmployeeTypeName => nameof(Employee<National, Company<National>>);
    private static string EmployeeStatisticsTypeName => nameof(EmployeeStatistics);
    private static string EmployeeActivityTypeName => nameof(EmployeeActivity);
    private static string EmployeeUvgInsuranceTypeName => nameof(EmployeeUvgInsurance);
    private static string EmployeeUvgzInsuranceTypeName => nameof(EmployeeUvgzInsurance);
    private static string EmployeeKtgInsuranceTypeName => nameof(EmployeeKtgInsurance);
    private static string EmployeeQstTypeName => nameof(EmployeeQst);

    /// <summary>Employee: entry date <see cref="EmployeeType.EntryDate"/></summary>
    public static readonly string EmployeeEntryDate =
        $"{EmployeeTypeName}{nameof(EmployeeType.EntryDate)}".ToNamespace();
    /// <summary>Employee: withdrawal date <see cref="EmployeeType.WithdrawalDate"/></summary>
    public static readonly string EmployeeWithdrawalDate =
        $"{EmployeeTypeName}{nameof(EmployeeType.WithdrawalDate)}".ToNamespace();
    /// <summary>Employee: activity rate percent <see cref="EmployeeType.ActivityRatePercent"/></summary>
    public static readonly string EmployeeActivityRatePercent =
        $"{EmployeeTypeName}{nameof(EmployeeType.ActivityRatePercent)}".ToNamespace();
    /// <summary>Employee: employment status <see cref="EmployeeType.EmploymentStatus"/></summary>
    public static readonly string EmployeeEmploymentStatus =
        $"{EmployeeTypeName}{nameof(EmployeeType.EmploymentStatus)}".ToNamespace();

    /// <summary>Employee statistics: contractual hourly wage <see cref="EmployeeStatisticsType.ContractualHourlyWage"/></summary>
    public static readonly string EmployeeStatisticsContractualHourlyWage =
        $"{EmployeeStatisticsTypeName}{nameof(EmployeeStatisticsType.ContractualHourlyWage)}".ToNamespace();

    /// <summary>Employee activity: worked hours <see cref="EmployeeActivityType.WorkedHours"/></summary>
    public static readonly string EmployeeActivityWorkedHours =
        $"{EmployeeActivityTypeName}{nameof(EmployeeActivityType.WorkedHours)}".ToNamespace();
    /// <summary>Employee activity: worked lessons <see cref="EmployeeActivityType.WorkedLessons"/></summary>
    public static readonly string EmployeeActivityWorkedLessons =
        $"{EmployeeActivityTypeName}{nameof(EmployeeActivityType.WorkedLessons)}".ToNamespace();

    /// <summary>Employee UVG insurance: code <see cref="EmployeeUvgInsuranceType.Code"/></summary>
    public static readonly string EmployeeUvgInsuranceCode =
        $"{EmployeeUvgInsuranceTypeName}{nameof(EmployeeUvgInsuranceType.Code)}".ToNamespace();
    /// <summary>Employee UVGZ insurance: code <see cref="EmployeeUvgInsuranceType.Code"/></summary>
    public static readonly string EmployeeUvgzInsuranceCode =
        $"{EmployeeUvgzInsuranceTypeName}{nameof(EmployeeUvgzInsuranceType.Code)}".ToNamespace();
    /// <summary>Employee KTG insurance: code <see cref="EmployeeKtgInsuranceType.Code"/></summary>
    public static readonly string EmployeeKtgInsuranceCode =
        $"{EmployeeKtgInsuranceTypeName}{nameof(EmployeeKtgInsuranceType.Code)}".ToNamespace();

    /// <summary>Employee QST: tax code <see cref="EmployeeQstType.TaxCode"/></summary>
    public static readonly string EmployeeQstTaxCode =
        $"{EmployeeQstTypeName}{nameof(EmployeeQstType.TaxCode)}".ToNamespace();
    /// <summary>Employee QST: effective work days <see cref="EmployeeQstType.EffectiveWorkDays"/></summary>
    public static readonly string EmployeeQstEffectiveWorkDays =
        $"{EmployeeQstTypeName}{nameof(EmployeeQstType.EffectiveWorkDays)}".ToNamespace();
    /// <summary>Employee QST: entry date <see cref="EmployeeQstType.WorkDaysSwitzerland"/></summary>
    public static readonly string EmployeeQstWorkDaysSwitzerland =
        $"{EmployeeQstTypeName}{nameof(EmployeeQstType.WorkDaysSwitzerland)}".ToNamespace();
    /// <summary>Employee QST: other activities <see cref="EmployeeQstType.OtherActivities"/></summary>
    public static readonly string EmployeeQstOtherActivities =
        $"{EmployeeQstTypeName}{nameof(EmployeeQstType.OtherActivities)}".ToNamespace();
    /// <summary>Employee QST: tax canton <see cref="EmployeeQstType.TaxCanton"/></summary>
    public static readonly string EmployeeQstTaxCanton =
        $"{EmployeeQstTypeName}{nameof(EmployeeQstType.TaxCanton)}".ToNamespace();
}

#endregion

/// <summary>Swissdec extensions</summary>
public static class PayrollFunctionExtensions
{
    /// <summary>Get case value with name prefix</summary>
    /// <param name="function">The function</param>
    /// <param name="caseFieldName">The case field name</param>
    public static TValue GetTypeCaseValue<TType, TValue>(this PayrollFunction function, [CallerMemberName] string caseFieldName = "") =>
        GetTypeCaseValue<TValue>(function, typeof(TType).GetTypeName(), caseFieldName);

    /// <summary>Get case value</summary>
    /// <param name="function">The function</param>
    /// <param name="typeName">The type name</param>
    /// <param name="caseFieldName">The case field name</param>
    public static T GetTypeCaseValue<T>(this PayrollFunction function, string typeName, [CallerMemberName] string caseFieldName = "")
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException("Missing type name", nameof(typeName));
        }
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException("Missing case field name", nameof(caseFieldName));
        }
        return GetCaseValue<T>(function, typeName + caseFieldName);
    }
    /// <summary>Get case value</summary>
    /// <remarks>this cast breaks the architectural layer access from function to payroll function
    /// benefits: single model for cases and access to the Swissdec XML tags through reporting
    /// </remarks>
    /// <param name="function">The function</param>
    /// <param name="caseFieldName">The case field name</param>
    public static T GetCaseValue<T>(this PayrollFunction function, [CallerMemberName] string caseFieldName = "")
    {
        return PayrollExtensions.GetCaseValue<T>(function, caseFieldName);
    }

    /// <summary>Get case slot value with name prefix</summary>
    /// <param name="function">The function</param>
    /// <param name="caseSlot">The case slot</param>
    /// <param name="caseFieldName">The case field name</param>
    public static TValue GetTypeCaseSlotValue<TType, TValue>(this PayrollFunction function, string caseSlot,
        [CallerMemberName] string caseFieldName = "") =>
        GetTypeCaseSlotValue<TValue>(function, caseSlot, typeof(TType).GetTypeName(), caseFieldName);

    /// <summary>Get case slot value</summary>
    /// <param name="function">The function</param>
    /// <param name="typeName">The type name</param>
    /// <param name="caseSlot">The case slot</param>
    /// <param name="caseFieldName">The case field name</param>
    public static T GetTypeCaseSlotValue<T>(this PayrollFunction function, string caseSlot, string typeName,
        [CallerMemberName] string caseFieldName = "")
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException("Missing type name", nameof(typeName));
        }
        if (string.IsNullOrWhiteSpace(caseSlot))
        {
            throw new ArgumentException("Missing case slot", nameof(caseSlot));
        }
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException("Missing case field name", nameof(caseFieldName));
        }
        return GetCaseSlotValue<T>(function, caseSlot, typeName + caseFieldName);
    }

    /// <summary>Get case slot value</summary>
    /// <param name="function">The function</param>
    /// <param name="caseSlot">The case slot</param>
    /// <param name="caseFieldName">The case field name</param>
    public static T GetCaseSlotValue<T>(this PayrollFunction function, string caseSlot, [CallerMemberName] string caseFieldName = "")
    {
        if (string.IsNullOrWhiteSpace(caseSlot))
        {
            throw new ArgumentException("Missing case slot", nameof(caseSlot));
        }
        return GetCaseValue<T>(function, function.CaseFieldSlot(caseFieldName, caseSlot));
    }
}
