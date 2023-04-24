/* Core */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

#region Enums

/// <summary>The employee gender</summary>
public enum Gender
{
    /// <summary>Female</summary>
    Female,
    /// <summary>Male</summary>
    Male
}

/// <summary>The employee pension contribution status</summary>
public enum PensionContributionStatus
{
    /// <summary>Non obligatory (children)</summary>
    NonObligatory,
    /// <summary>Normal adult</summary>
    Obligatory,
    /// <summary>Pensioner</summary>
    Pensioner,
    /// <summary>Special case</summary>
    SpecialCase
}

/// <summary>The swiss cantons</summary>
[SuppressMessage("ReSharper", "CommentTypo")]
public enum Canton
{
    /// <summary>Aargau</summary>
    Ag,
    /// <summary>Appenzell Innerrhoden</summary>
    Ai,
    /// <summary>Appenzell Ausserrhoden</summary>
    Ar,
    /// <summary>Bern</summary>
    Be,
    /// <summary>Basel-Landschaft</summary>
    Bl,
    /// <summary>Basel-Stadt</summary>
    Bs,
    /// <summary>Freiburg</summary>
    Fr,
    /// <summary>Genf</summary>
    Ge,
    /// <summary>Glarus</summary>
    Gl,
    /// <summary>Graubünden</summary>
    Gr,
    /// <summary>Luzern</summary>
    Lu,
    /// <summary>Neuenburg</summary>
    Ne,
    /// <summary>Nidwalden</summary>
    Nw,
    /// <summary>Obwalden</summary>
    Ow,
    /// <summary>St. Gallen</summary>
    Sg,
    /// <summary>Schaffhausen</summary>
    Sh,
    /// <summary>Schwyz</summary>
    Sz,
    /// <summary>Solothurn</summary>
    So,
    /// <summary>Thurgau</summary>
    Tg,
    /// <summary>Tessin</summary>
    Ti,
    /// <summary>Uri</summary>
    Ur,
    /// <summary>Waadt</summary>
    Vd,
    /// <summary>Wallis</summary>
    Vs,
    /// <summary>Zug</summary>
    Zg,
    /// <summary>Zürich</summary>
    Zh
}

/// <summary>Swissdec QST calculation cycle type</summary>
public enum QstCalculationCycle
{
    /// <summary>Monthly cycle</summary>
    Monthly,
    /// <summary>Yearly cycle</summary>
    Yearly
}

/// <summary>Swissdec UVG insurance type</summary>
public enum UvgInsuranceType
{
    /// <summary>Not insured</summary>
    NotInsured = 0,
    /// <summary>Insured shared by employee and company</summary>
    InsuredShared = 1,
    /// <summary>Insured by company</summary>
    InsuredCompany = 2,
    /// <summary>Insured by company</summary>
    InsuredCompanyOnly = 3
}

#endregion

#region Collections

/// <summary>Swissdec slot collection with start index</summary>
public class SlotCollection<TSlot> : Dictionary<int, TSlot>
    where TSlot : class
{

    /// <summary>Constructor</summary>
    /// <param name="function">The function</param>
    /// <param name="count">The slot count</param>
    /// <param name="startIndex">The slot start index, default is 1</param>
    public SlotCollection(Function function, int count, int startIndex = 1)
    {
        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }
        for (var index = startIndex; index <= startIndex + count; index++)
        {
            var slotValue = (TSlot)Activator.CreateInstance(typeof(TSlot), function, index.ToString());
            Add(index, slotValue);
        }
    }

}

/// <summary>Swissdec by enum named slot collection</summary>
public class NamedSlotCollection<TFunction, TEnum, TSlot> : Dictionary<TEnum, TSlot>
    where TFunction : Function
    where TEnum : struct
    where TSlot : class
{
    /// <summary>Constructor with selected enum names</summary>
    /// <param name="function">The function</param>
    /// <param name="selection">The slot selection (CSV)</param>
    public NamedSlotCollection(TFunction function, string selection)
    {
        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }
        if (string.IsNullOrWhiteSpace(selection))
        {
            throw new ArgumentException("Missing selection", nameof(selection));
        }
        foreach (var slot in selection.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (Enum.TryParse(typeof(TEnum), slot, out var enumValue) && enumValue != null)
            {
                var slotValue = (TSlot)Activator.CreateInstance(typeof(TSlot), function, slot);
                Add((TEnum)enumValue, slotValue);
            }
        }
    }
}

#endregion

#region Lookup names

/// <summary>Swissdec lookup name</summary>
public static class LookupName
{
    /// <summary>Qst cantons lookup name</summary>
    public static readonly string QstCantons = "QstCantons".ToNamespace();

    /// <summary>Workplaces lookup name</summary>
    public static readonly string Workplaces = "Workplaces".ToNamespace();
}

#endregion

#region Collector names

/// <summary>Swissdec collector name</summary>
public static class CollectorName
{
    /// <summary>AHV base collector name</summary>
    public static readonly string AhvBase = "AhvBase".ToNamespace();
    /// <summary>ALV base collector name</summary>
    public static readonly string AlvBase = "AlvBase".ToNamespace();
    /// <summary>QST base collector name</summary>
    public static readonly string QstBase = "QstBase".ToNamespace();
    /// <summary>QSTA base collector name</summary>
    public static readonly string QstaBase = "QstaBase".ToNamespace();
    /// <summary>QSTP base collector name</summary>
    public static readonly string QstpBase = "QstpBase".ToNamespace();
    /// <summary>UVG base collector name</summary>
    public static readonly string UvgBase = "UvgBase".ToNamespace();
    /// <summary>UVGZ base collector name</summary>
    public static readonly string UvgzBase = "UvgzBase".ToNamespace();
    /// <summary>KTG base collector name</summary>
    public static readonly string KtgBase = "KtgBase".ToNamespace();
    /// <summary>BVG base collector name</summary>
    public static readonly string BvgBase = "BvgBase".ToNamespace();

    /// <summary>Thirteenth month wage base collector name</summary>
    public static readonly string ThirteenthMonthWageBase = "ThirteenthMonthWageBase".ToNamespace();

    /// <summary>Gross salary collector name</summary>
    public static readonly string GrossSalary = "GrossSalary".ToNamespace();
    /// <summary>Employee contributions collector name</summary>
    public static readonly string EmployeeContributions = "EmployeeContributions".ToNamespace();
    /// <summary>Expenses collector name</summary>
    public static readonly string Expenses = "Expenses".ToNamespace();
}

/// <summary>Swissdec collector group name</summary>
public static class CollectorGroupName
{
    /// <summary>AHV/ALV/ALVZ collector group name</summary>
    public static readonly string AhvAlvAlvzBase = "AhvAlvAlvzBase".ToNamespace();
    /// <summary>UVG/UVGZ/KTG/BVG collector group name</summary>
    public static readonly string UvgUvgzKtgBvgBase = "UvgUvgzKtgBvgBase".ToNamespace();
}

#endregion

#region Wage type numbers

/// <summary>Swissdec wage type number</summary>
public static class WageTypeNumber
{
    /// <summary>Monthly salary</summary>
    public static readonly decimal MonthlySalary = 1000;
    /// <summary>Hourly wage</summary>
    public static readonly decimal HourlyWage = 1005;
    /// <summary>Daily wage</summary>
    public static readonly decimal DailyWage = 1006;
    /// <summary>Weekly wage</summary>
    public static readonly decimal WeeklyWage = 1007;
    /// <summary>Remuneration</summary>
    public static readonly decimal Remuneration = 1010;
    /// <summary>Temporary staff salaries</summary>
    public static readonly decimal TemporaryStaffSalaries = 1015;
    /// <summary>Homebased work allowance</summary>
    public static readonly decimal HomebasedWorkAllowance = 1016;
    /// <summary>Cleaning salary</summary>
    public static readonly decimal CleaningSalary = 1017;
    /// <summary>Piecework wage</summary>
    public static readonly decimal PieceworkWage = 1018;
    /// <summary>Absence compensation</summary>
    public static readonly decimal AbsenceCompensation = 1020;
    /// <summary>Authorities and committee members</summary>
    public static readonly decimal AuthoritiesAndCommitteeMembers = 1021;
    /// <summary>Seniority allowance</summary>
    public static readonly decimal SeniorityAllowance = 1030;
    /// <summary>Function allowance</summary>
    public static readonly decimal FunctionAllowance = 1031;
    /// <summary>Representation allowance</summary>
    public static readonly decimal RepresentationAllowance = 1032;
    /// <summary>Residential allowance</summary>
    public static readonly decimal ResidentialAllowance = 1033;
    /// <summary>Cost of living allowance</summary>
    public static readonly decimal CostOfLivingAllowance = 1034;
    /// <summary>Family cost of living allowance</summary>
    public static readonly decimal FamilyCostOfLivingAllowance = 1040;
    /// <summary>Accommodation allowance</summary>
    public static readonly decimal AccommodationAllowance = 1050;
    /// <summary>Travel expenses reimbursement</summary>
    public static readonly decimal TravelExpensesReimbursement = 1055;
    /// <summary>Displacement allowance</summary>
    public static readonly decimal DisplacementAllowance = 1056;
    /// <summary>Additional work</summary>
    public static readonly decimal AdditionalWork = 1060;
    /// <summary>Overtime 125%</summary>
    public static readonly decimal Overtime125Percent = 1061;
    /// <summary>Overtime</summary>
    public static readonly decimal Overtime = 1065;
    /// <summary>Overtime after withdrawal</summary>
    public static readonly decimal OvertimeAfterWithdrawal = 1067;
    /// <summary>Shift bonus</summary>
    public static readonly decimal ShiftBonus = 1070;
    /// <summary>Pikett remuneration</summary>
    public static readonly decimal PikettRemuneration = 1071;
    /// <summary>Assignment bonus</summary>
    public static readonly decimal AssignmentBonus = 1072;
    /// <summary>Sunday bonus</summary>
    public static readonly decimal SundayBonus = 1073;
    /// <summary>Inconvenience bonus</summary>
    public static readonly decimal InconvenienceBonus = 1074;
    /// <summary>Night shift bonus</summary>
    public static readonly decimal NightShiftBonus = 1075;
    /// <summary>Night work bonus</summary>
    public static readonly decimal NightWorkBonus = 1076;
    /// <summary>Construction site bonus</summary>
    public static readonly decimal ConstructionSiteBonus = 1100;
    /// <summary>Difficulty bonus</summary>
    public static readonly decimal DifficultyBonus = 1101;
    /// <summary>Dirty work bonus</summary>
    public static readonly decimal DirtyWorkBonus = 1102;
    /// <summary>Dust work bonus</summary>
    public static readonly decimal DustWorkBonus = 1103;
    /// <summary>Underground work bonus</summary>
    public static readonly decimal UndergroundWorkBonus = 1104;
    /// <summary>Propulsion bonus</summary>
    public static readonly decimal PropulsionBonus = 1110;
    /// <summary>Tunneling bonus</summary>
    public static readonly decimal TunnelingBonus = 1111;
    /// <summary>Tenacity bonus</summary>
    public static readonly decimal TenacityBonus = 1112;
    /// <summary>Appearance bonus</summary>
    public static readonly decimal AppearanceBonus = 1130;
    /// <summary>Non appearance compensation</summary>
    public static readonly decimal NonAppearanceCompensation = 1131;
    /// <summary>Holiday compensation</summary>
    public static readonly decimal HolidayCompensation = 1160;
    /// <summary></summary>
    public static readonly decimal PublicHolidayCompensation = 1161;
    /// <summary>Vacation payout</summary>
    public static readonly decimal VacationPayout = 1162;
    /// <summary>Monthly wage 13th</summary>
    public static readonly decimal MonthlyWage13th = 1200;
    /// <summary>Gratuity</summary>
    public static readonly decimal Gratuity = 1201;
    /// <summary>Christmas gratuity</summary>
    public static readonly decimal ChristmasGratuity = 1202;
    /// <summary>Bonus payment</summary>
    public static readonly decimal BonusPayment = 1210;
    /// <summary>Profit participation</summary>
    public static readonly decimal ProfitParticipation = 1211;
    /// <summary>Special allowance</summary>
    public static readonly decimal SpecialAllowance = 1212;
    /// <summary>Success bonus</summary>
    public static readonly decimal SuccessBonus = 1213;
    /// <summary>Performance bonus</summary>
    public static readonly decimal PerformanceBonus = 1214;
    /// <summary>Recognition bonus</summary>
    public static readonly decimal RecognitionBonus = 1215;
    /// <summary>Improvement suggestions</summary>
    public static readonly decimal ImprovementSuggestions = 1216;
    /// <summary>Revenue bonus</summary>
    public static readonly decimal RevenueBonus = 1217;
    /// <summary>Commission</summary>
    public static readonly decimal Commission = 1218;
    /// <summary>Presence bonus</summary>
    public static readonly decimal PresenceBonus = 1219;
    /// <summary>Gift for years of service</summary>
    public static readonly decimal GiftForYearsOfService = 1230;
    /// <summary>Jubilee gift</summary>
    public static readonly decimal JubileeGift = 1231;
    /// <summary>Fidelity bonus</summary>
    public static readonly decimal FidelityBonus = 1232;
    /// <summary>Loss prevention bonus</summary>
    public static readonly decimal LossPreventionBonus = 1250;
    /// <summary>Accident salary</summary>
    public static readonly decimal AccidentSalary = 1300;
    /// <summary>Illness salary</summary>
    public static readonly decimal IllnessSalary = 1301;
    /// <summary>Military service, civil protection salary</summary>
    public static readonly decimal MilitaryServiceCivilProtectionSalary = 1302;
    /// <summary>Education training salary</summary>
    public static readonly decimal EducationTrainingSalary = 1303;
    /// <summary>Severance payment free</summary>
    public static readonly decimal SeverancePaymentFree = 1400;
    /// <summary>Severance payment paid</summary>
    public static readonly decimal SeverancePaymentPaid = 1401;
    /// <summary>Provident nature capital payment</summary>
    public static readonly decimal ProvidentNatureCapitalPayment = 1410;
    /// <summary>Capital payment paid</summary>
    public static readonly decimal CapitalPaymentPaid = 1411;
    /// <summary>Continued wages payment</summary>
    public static readonly decimal ContinuedWagesPayment = 1420;
    /// <summary>Remuneration BoD</summary>
    public static readonly decimal RemunerationBod = 1500;
    /// <summary>Compensation BoD</summary>
    public static readonly decimal CompensationBod = 1501;
    /// <summary>Attendance fees BoD</summary>
    public static readonly decimal AttendanceFeesBod = 1503;
    /// <summary>Royalties BoD</summary>
    public static readonly decimal RoyaltiesBod = 1510;
    /// <summary>Meals free charge</summary>
    public static readonly decimal MealsFreeCharge = 1900;
    /// <summary>Lodging free charge</summary>
    public static readonly decimal LodgingFreeCharge = 1901;
    /// <summary>Accommodation free charge</summary>
    public static readonly decimal AccommodationFreeCharge = 1902;
    /// <summary>Fringe benefits car</summary>
    public static readonly decimal FringeBenefitsCar = 1910;
    /// <summary>Tip paid</summary>
    public static readonly decimal TipPaid = 1920;
    /// <summary>Rented flat price reduction</summary>
    public static readonly decimal RentedFlatPriceReduction = 1950;
    /// <summary>Expatriates payment in kind</summary>
    public static readonly decimal ExpatriatesPaymentInKind = 1953;
    /// <summary>Taxable participation rights</summary>
    public static readonly decimal TaxableParticipationRights = 1960;
    /// <summary>Employee shares</summary>
    public static readonly decimal EmployeeShares = 1961;
    /// <summary>Employee options</summary>
    public static readonly decimal EmployeeOptions = 1962;
    /// <summary>Employer facultative DSA</summary>
    public static readonly decimal EmployerFacultativeDsa = 1971;
    /// <summary>Employer facultative PF/LOB</summary>
    public static readonly decimal EmployerFacultativePfLob = 1972;
    /// <summary>Employer facultative redemption PF/LOB</summary>
    public static readonly decimal EmployerFacultativeRedemptionPfLob = 1973;
    /// <summary>Employer facultative health insurance</summary>
    public static readonly decimal EmployerFacultativeHealthInsurance = 1974;
    /// <summary>Employer facultative SAI</summary>
    public static readonly decimal EmployerFacultativeSai = 1975;
    /// <summary>Employer facultative pillar 3b</summary>
    public static readonly decimal EmployerFacultativePillar3b = 1976;
    /// <summary>Employer facultative pillar 3a</summary>
    public static readonly decimal EmployerFacultativePillar3a = 1977;
    /// <summary>Facultative withholding tax</summary>
    public static readonly decimal FacultativeWithholdingTax = 1978;
    /// <summary>Further training</summary>
    public static readonly decimal FurtherTraining = 1980;

    /// <summary>IC daily allowance</summary>
    public static readonly decimal IcDailyAllowance = 2000;
    /// <summary>Military service insurance</summary>
    public static readonly decimal MilitaryServiceInsurance = 2005;
    /// <summary>Military supplement insurance</summary>
    public static readonly decimal MilitarySupplementInsurance = 2010;
    /// <summary>Parifonds</summary>
    public static readonly decimal Parifonds = 2015;
    /// <summary>Military daily allowance</summary>
    public static readonly decimal MilitaryDailyAllowance = 2020;
    /// <summary>Military pension</summary>
    public static readonly decimal MilitaryPension = 2021;
    /// <summary>DÌ daily allowance</summary>
    public static readonly decimal DiDailyAllowance = 2025;
    /// <summary>DI pension</summary>
    public static readonly decimal DiPension = 2026;
    /// <summary>SUVA daily allowance</summary>
    public static readonly decimal SuvaDailyAllowance = 2030;
    /// <summary>SUVA pension</summary>
    public static readonly decimal SuvaPension = 2031;
    /// <summary>Daily sickness allowance</summary>
    public static readonly decimal DailySicknessAllowance = 2035;
    /// <summary>Maternity compensation</summary>
    public static readonly decimal MaternityCompensation = 2050;
    /// <summary>Daily allowance correction</summary>
    public static readonly decimal DailyAllowanceCorrection = 2050;
    /// <summary>Net wages adjustment</summary>
    public static readonly decimal NetWagesAdjustment = 2051;
    /// <summary>Deduction STW</summary>
    public static readonly decimal DeductionStw = 2060;
    /// <summary>Loss of wages STW</summary>
    public static readonly decimal LossOfWagesStw = 2065;
    /// <summary>Unemployment compensation</summary>
    public static readonly decimal UnemploymentCompensation = 2070;
    /// <summary>Waiting period STW</summary>
    public static readonly decimal WaitingPeriodStw = 2075;

    /// <summary>Child allowance</summary>
    public static readonly decimal ChildAllowance = 3000;
    /// <summary>Education allowance</summary>
    public static readonly decimal EducationAllowance = 3010;
    /// <summary>Family allowance</summary>
    public static readonly decimal FamilyAllowance = 3030;
    /// <summary>Household allowance</summary>
    public static readonly decimal HouseholdAllowance = 3032;
    /// <summary>Birth allowance</summary>
    public static readonly decimal BirthAllowance = 3034;
    /// <summary>Wedding allowance</summary>
    public static readonly decimal WeddingAllowance = 3035;
    /// <summary>Custody allowance</summary>
    public static readonly decimal CustodyAllowance = 3036;
    /// <summary>Employee wage child allowances AHV</summary>
    public static readonly decimal EmployeeWageChildAllowancesAhv = 3038;

    /// <summary>Gross wage</summary>
    public static readonly decimal GrossWage = 5000;
    /// <summary>OASI wage</summary>
    public static readonly decimal OasiWage = 5001;
    /// <summary>UI wage</summary>
    public static readonly decimal UiWage = 5002;
    /// <summary>SUI wage</summary>
    public static readonly decimal SuiWage = 5003;
    /// <summary>Suva wage</summary>
    public static readonly decimal SuvaWage = 5004;
    /// <summary>Sai wage</summary>
    public static readonly decimal SaiWage = 5005m;
    /// <summary>SAI wage 1</summary>
    public static readonly decimal SaiWage1 = 5005.1m;
    /// <summary>SAI wage 2</summary>
    public static readonly decimal SaiWage2 = 5005.2m;
    /// <summary>SAI wage 3</summary>
    public static readonly decimal SaiWage3 = 5005.3m;
    /// <summary>SAI wage 4</summary>
    public static readonly decimal SaiWage4 = 5005.4m;
    /// <summary>SAI wage 5</summary>
    public static readonly decimal SaiWage5 = 5005.5m;
    /// <summary>Dsa wage</summary>
    public static readonly decimal DsaWage = 5006m;
    /// <summary>DSA wage 1</summary>
    public static readonly decimal DsaWage1 = 5006.1m;
    /// <summary>DSA wage 2</summary>
    public static readonly decimal DsaWage2 = 5006.2m;
    /// <summary>DSA wage 3</summary>
    public static readonly decimal DsaWage3 = 5006.3m;
    /// <summary>DSA wage 4</summary>
    public static readonly decimal DsaWage4 = 5006.4m;
    /// <summary>DSA wage 5</summary>
    public static readonly decimal DsaWage5 = 5006.5m;
    /// <summary>QST periodical wage</summary>
    public static readonly decimal QstPeriodicalWage = 5007.1m;
    /// <summary>QST non periodical wage</summary>
    public static readonly decimal QstNonPeriodicalWage = 5007.2m;
    /// <summary>QST non periodical wage</summary>
    public static readonly decimal QstWage = 5007.3m;
    /// <summary>QST percent defining wage</summary>
    public static readonly decimal QstPeriodicalPercentDefiningWage = 5008.1m;
    /// <summary>QST percent defining wage</summary>
    public static readonly decimal QstNonPeriodicalPercentDefiningWage = 5008.2m;
    /// <summary>QST percent defining wage</summary>
    public static readonly decimal QstPercentDefiningWage = 5008.3m;
    /// <summary>OASI contribution</summary>
    public static readonly decimal OasiContribution = 5010;
    /// <summary>UI contribution</summary>
    public static readonly decimal UiContribution = 5020;
    /// <summary>SUI contribution</summary>
    public static readonly decimal SuiContribution = 5030;
    /// <summary>SUVA contribution</summary>
    public static readonly decimal SuvaContribution = 5040;
    /// <summary>SAI contribution</summary>
    public static readonly decimal SaiContribution = 5041m;
    /// <summary>DSA contribution</summary>
    public static readonly decimal DsaContribution = 5045m;
    /// <summary>PF/LOB contribution</summary>
    public static readonly decimal PfLobContribution = 5050;
    /// <summary>PF/LOB redemption contribution</summary>
    public static readonly decimal PfLobRedemptionContribution = 5051;
    /// <summary>Withholding tax deduction</summary>
    public static readonly decimal WithholdingTaxDeduction = 5060;
    /// <summary>Church tax deduction Geneve</summary>
    public static readonly decimal ChurchTaxDeductionGe = 5062;
    /// <summary>Payment in kind adjustment</summary>
    public static readonly decimal PaymentInKindAdjustment = 5100;
    /// <summary>Non cash benefits adjustment</summary>
    public static readonly decimal NonCashBenefitsAdjustment = 5110;
    /// <summary>PF/LOB employer adjustment</summary>
    public static readonly decimal PfLobEmployerAdjustment = 5111;
    /// <summary>PF/LOB employer redemption adjustment</summary>
    public static readonly decimal PfLobEmployerRedemptionAdjustment = 5112;

    /// <summary>Travel expenses</summary>
    public static readonly decimal TravelExpenses = 6000;
    /// <summary>Car expenses</summary>
    public static readonly decimal CarExpenses = 6001;
    /// <summary>Meals expenses</summary>
    public static readonly decimal MealsExpenses = 6002;
    /// <summary>Accommodation costs</summary>
    public static readonly decimal AccommodationCosts = 6010;
    /// <summary>Effective costs expatriates</summary>
    public static readonly decimal EffectiveCostsExpatriates = 6020;
    /// <summary>Effective costs remaining</summary>
    public static readonly decimal EffectiveCostsRemaining = 6030;
    /// <summary>Lump professional expenses expatriates</summary>
    public static readonly decimal LumpProfessionalExpensesExpatriates = 6035;
    /// <summary>Flat-rate representation expenses</summary>
    public static readonly decimal FlatRateRepresentationExpenses = 6040;
    /// <summary>Flat-rate car expenses</summary>
    public static readonly decimal FlatRateCarExpenses = 6050;
    /// <summary>Flat-rate expenses expatriates</summary>
    public static readonly decimal FlatRateExpensesExpatriates = 6060;
    /// <summary>Flat-rate expenses remaining</summary>
    public static readonly decimal FlatRateExpensesRemaining = 6070;
    /// <summary>Net wage retro</summary>
    public static readonly decimal NetWageRetro = 6499;
    /// <summary>Net wage</summary>
    public static readonly decimal NetWage = 6500;
    /// <summary>Advance payment</summary>
    public static readonly decimal AdvancePayment = 6510;
    /// <summary>Period paid wage runs</summary>
    public static readonly decimal PeriodPaidWageRuns = 6550;
    /// <summary>Payment</summary>
    public static readonly decimal Payment = 6600;
}

#endregion

#region Insurance names

/// <summary>Swissdec insurance name</summary>
public static class InsuranceName
{
    /// <summary>UVG insurance name</summary>
    public static readonly string Uvg = "UVG";
    /// <summary>UVGZ insurance name</summary>
    public static readonly string Uvgz = "UVGZ";
    /// <summary>KTG insurance name</summary>
    public static readonly string Ktg = "KTG";
}

#endregion

#region Swissdec Object

/// <summary>Swissdec base object</summary>
public abstract class SwissdecBase<T> where T : Function
{
    /// <summary>The function</summary>
    public T Function { get; }

    /// <summary>Function constructor</summary>
    protected SwissdecBase(T function)
    {
        Function = function ?? throw new ArgumentNullException(nameof(function));
    }
}

#endregion

#region Extensions

/// <summary>Swissdec extensions</summary>
public static class SwissdecExtensions
{
    /// <summary>Get the type name</summary>
    public static string GetTypeName(this Type type)
    {
        var name = type.Name;
        var index = name.IndexOf('`');
        return index == -1 ? name : name.Substring(0, index);
    }

    /// <summary>Get case field name from type and property name</summary>
    public static string GetCaseFieldName(this Type type, string propertyName) =>
        $"{type.Name}{propertyName}".ToNamespace();

    /// <summary>Ensure the Swissdec namespace</summary>
    /// <param name="source">The source value</param>
    public static string ToNamespace(this string source) =>
        source.EnsureStart(Namespace.Root + ".");

    /// <summary>Number as decimal part (1 to 0.1)</summary>
    /// <param name="number">The decimal part number</param>
    public static decimal NumberAsDecimalPart(this int number)
    {
        if (number <= 0)
        {
            return 0;
        }
        // number of digits
        var digits = Math.Floor(Math.Log10(number) + 1);
        // divide number by 10^(digits)
        var result = number / Math.Pow(10, digits);
        return (decimal)result;
    }

    /// <summary>Get the QST shared lookup name</summary>
    /// <param name="canton">The QST canton</param>
    public static string QstLookupName(this Canton canton) =>
        $"Qst{canton}".ToNamespace();

    /// <summary>Get the QST cycle code</summary>
    /// <param name="qstCycle">The QST cycle</param>
    public static string ToQstCycleCode(this QstCalculationCycle qstCycle) =>
        qstCycle switch
        {
            QstCalculationCycle.Monthly => "M",
            QstCalculationCycle.Yearly => "Y",
            _ => throw new ArgumentOutOfRangeException(nameof(qstCycle), qstCycle, null)
        };

    /// <summary>Get the QST cycle from code</summary>
    /// <param name="cycleCode">The QST cycle code</param>
    public static QstCalculationCycle ToQstCycle(this string cycleCode) =>
        cycleCode switch
        {
            "M" => QstCalculationCycle.Monthly,
            "Y" => QstCalculationCycle.Yearly,
            _ => throw new ArgumentOutOfRangeException(nameof(cycleCode), cycleCode, null)
        };

    /// <summary>Get the tagged name</summary>
    /// <param name="_">The function</param>
    /// <param name="name">The source name</param>
    public static string TaggedName(this Function _, [CallerMemberName] string name = "") =>
        name.ToNamespace();

    /// <summary>Get Swissdec XML tag from object property</summary>
    /// <param name="source">The source object</param>
    /// <param name="propertyName">The property name</param>
    public static string GetXmlTag(this object source, string propertyName) =>
        GetXmlTagAttribute(source, propertyName)?.Tag;

    /// <summary>Get Swissdec XML tag attribute, including the alternate tag name</summary>
    /// <param name="source">The source object</param>
    /// <param name="propertyName">The property name</param>
    public static SwissdecXmlTagAttribute GetXmlTagAttribute(this object source, string propertyName)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException(nameof(propertyName));
        }

        // property
        var property = source.GetType().GetProperty(propertyName);
        if (property == null)
        {
            throw new ScriptException($"Unknown property {propertyName} in type {source.GetType()}");
        }
        // xml attribute
        var attribute = property.GetCustomAttributes(typeof(SwissdecXmlTagAttribute), false).FirstOrDefault();
        return attribute as SwissdecXmlTagAttribute;
    }

    /// <summary>Convert value to percent string</summary>
    /// <param name="value"></param>
    /// <returns>Returns value in string percent format with % sign at end.</returns>
    public static string ToPercentFormat(this decimal value)
    {
        return $"{value:P2}";
    }
}

#endregion

#region Attributes

/// <summary>Swissdec XML tag attribute</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SwissdecXmlTagAttribute : Attribute
{
    /// <summary>The Swissdec XML tag name</summary>
    public string Tag { get; }

    /// <summary>The Swissdec XML alternate tag name</summary>
    public string AlternateTag { get; }

    /// <summary>Constructor</summary>
    /// <param name="tag">The XML tag</param>
    /// <param name="alternateTag">The XML alternate tag</param>
    public SwissdecXmlTagAttribute(string tag, string alternateTag = null)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException(nameof(tag));
        }
        Tag = tag;
        AlternateTag = alternateTag;
    }
}

/// <summary> </summary>
public static class AttributeName
{
    /// <summary></summary>
    public static readonly string Amount = "Amount";
    /// <summary></summary>
    public static readonly string Code = "Code";
    /// <summary></summary>
    public static readonly string Percentage = "Percentage";
    /// <summary></summary>
    public static readonly string Report = "Report";
    /// <summary></summary>
    public static readonly string Subtotal = "Subtotal";
    /// <summary></summary>
    public static readonly string Total = "Total";
}

#endregion

#region Reports
/// <summary></summary>
public static class ReportName
{
    /// <summary>Payslip report</summary>
    public static readonly string Payslip = "Payslip";
}

#endregion
