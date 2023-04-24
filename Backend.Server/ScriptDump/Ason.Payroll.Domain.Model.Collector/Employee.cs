/* Employee */
using System;
using System.Collections.Generic;
using System.Linq;
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec Employee</summary>
public class Employee<TNational, TCompany> : SwissdecBase<PayrollFunction>
    where TNational : National
    where TCompany : Company<TNational>
{
    /// <summary>Function constructor</summary>
    public Employee(PayrollFunction function, TNational national, TCompany company) :
        base(function)
    {
        National = national ?? throw new ArgumentNullException(nameof(national));
        Company = company ?? throw new ArgumentNullException(nameof(company));

        Address = new(Function);
        Partner = new(Function);
        BorderCrosses = new(Function);
        qst = new(Function);
        ahv = new(Function);
        uvg = new(Function);
        Certificate = new(Function);
        Statistics = new(Function);
        Wage = new(Function);
        OneTimeWage = new(Function);
        ShortTimeWork = new(Function);
        PresenceAbsence = new(Function);
        Expense = new(Function);
        Activity = new(Function);
    }

    /// <summary>Swissdec national</summary>
    public virtual TNational National { get; }

    /// <summary>Swissdec company</summary>
    public virtual TCompany Company { get; }

    /// <summary>Employee address</summary>
    public virtual EmployeeAddress Address { get; }

    /// <summary>Employee partner</summary>
    public virtual EmployeePartner Partner { get; }

    /// <summary>Employee border crosses</summary>
    public virtual EmployeeBorderCrosses BorderCrosses { get; }

    /// <summary>Employee salary certificate</summary>
    public virtual EmployeeSalaryCertificate Certificate { get; }

    /// <summary>Employee statistics</summary>
    public virtual EmployeeStatistics Statistics { get; }

    /// <summary>Employee wage</summary>
    public virtual EmployeeWage Wage { get; }

    /// <summary>Employee one-time wage</summary>
    public virtual EmployeeOneTimeWage OneTimeWage { get; }

    /// <summary>Employee short-time work</summary>
    public virtual EmployeeShortTimeWork ShortTimeWork { get; }

    /// <summary>Employee presence and absence</summary>
    public virtual EmployeePresenceAbsence PresenceAbsence { get; }

    /// <summary>Employee expense</summary>
    public virtual EmployeeExpense Expense { get; }

    /// <summary>Employee activity</summary>
    public virtual EmployeeActivity Activity { get; }

    #region Insurance

    private EmployeeQst qst;
    /// <summary>Employee QST</summary>
    public virtual EmployeeQst Qst => qst ??= new(Function);

    private EmployeeAhvInsurance ahv;
    /// <summary>Employee AHV</summary>
    public virtual EmployeeAhvInsurance Ahv => ahv ??= new(Function);

    private EmployeeUvgInsurance uvg;
    /// <summary>Employee UVG</summary>
    public virtual EmployeeUvgInsurance Uvg => uvg ??= new(Function);

    private EmployeeUvgz uvgz;
    /// <summary>Employee UVGZ</summary>
    public virtual EmployeeUvgz Uvgz => uvgz ??= new(Function);

    private EmployeeKtg ktg;
    /// <summary>Employee KTG</summary>
    public virtual EmployeeKtg Ktg => ktg ??= new(Function);

    private EmployeeBvg bvg;
    /// <summary>Employee BVG</summary>
    public virtual EmployeeBvg Bvg => bvg ??= new(Function);

    #endregion

    #region Personal

    /// <summary>Employee first name</summary>
    [SwissdecXmlTag("Firstname")]
    public string FirstName => Function.GetTypeCaseValue<
        Employee<TNational, TCompany>, string>();

    /// <summary>Employee last name</summary>
    [SwissdecXmlTag("Lastname")]
    public string LastName => Function.GetTypeCaseValue<Employee<TNational, TCompany>, string>();

    /// <summary>Employee AHV number</summary>
    [SwissdecXmlTag("AhvNumber")]
    public string AhvNumber => Function.GetTypeCaseValue<Employee<TNational, TCompany>, string>();

    /// <summary>Employee sex</summary>
    [SwissdecXmlTag("Sex")]
    public string Sex => Function.GetTypeCaseValue<Employee<TNational, TCompany>, string>();

    /// <summary>Employee birth date</summary>
    [SwissdecXmlTag("DateOfBirth")]
    public DateTime? BirthDate => Function.GetTypeCaseValue<Employee<TNational, TCompany>, DateTime?>();

    /// <summary>Employee birth date</summary>
    [SwissdecXmlTag("LanguageCode")]
    public Language? Language => Function.GetTypeCaseValue<Employee<TNational, TCompany>, Language?>();

    /// <summary>Employee nationality</summary>
    [SwissdecXmlTag("Nationality")]
    public string Nationality => Function.GetTypeCaseValue<Employee<TNational, TCompany>, string>();

    /// <summary>Employee civil status</summary>
    [SwissdecXmlTag("CivilStatus")]
    public string CivilStatus => Function.GetTypeCaseValue<Employee<TNational, TCompany>, string>();

    /// <summary>Employee entry date</summary>
    [SwissdecXmlTag("EntryDate")]
    public DateTime? EntryDate => Function.GetTypeCaseValue<Employee<TNational, TCompany>, DateTime?>();

    /// <summary>Employee withdrawal date</summary>
    [SwissdecXmlTag("WithdrawalDate")]
    public DateTime? WithdrawalDate => Function.GetTypeCaseValue<Employee<TNational, TCompany>, DateTime?>();

    /// <summary>Employee employment status date</summary>
    public decimal? EmploymentStatus => Function.GetTypeCaseValue<Employee<TNational, TCompany>, decimal?>();

    /// <summary>Employee civil status valid from date</summary>
    [SwissdecXmlTag("ValidAsOf")]
    public DateTime? CivilStatusValidFromDate => Function.GetTypeCaseValue<Employee<TNational, TCompany>, DateTime?>();

    /// <summary>Employee residence category</summary>
    [SwissdecXmlTag("ResidenceCategory")]
    public string ResidenceCategory => Function.GetTypeCaseValue<Employee<TNational, TCompany>, string>();

    /// <summary>Employee workplace id</summary>
    [SwissdecXmlTag("WorkplaceIDRef")]
    public string WorkplaceId => Function.GetTypeCaseValue<Employee<TNational, TCompany>, string>();

    /// <summary>Employee working type</summary>
    [SwissdecXmlTag("Steady", "Unsteady")]
    public string WorkingType => Function.GetTypeCaseValue<Employee<TNational, TCompany>, string>();

    /// <summary>Employee weekly hours</summary>
    [SwissdecXmlTag("WeeklyHours")]
    public decimal? WeeklyHours => Function.GetTypeCaseValue<Employee<TNational, TCompany>, decimal?>();

    /// <summary>Employee weekly lessons</summary>
    [SwissdecXmlTag("WeeklyLessons")]
    public decimal? WeeklyLessons => Function.GetTypeCaseValue<Employee<TNational, TCompany>, decimal?>();

    /// <summary>Employee activity rate in percent</summary>
    [SwissdecXmlTag("ActivityRate")]
    public decimal? ActivityRatePercent => Function.GetTypeCaseValue<Employee<TNational, TCompany>, decimal?>();

    /// <summary>Employee monthly work time</summary>
    public int? MonthlyWorkTime => Function.GetTypeCaseValue<Employee<TNational, TCompany>, int?>();

    /// <summary>Employee gender</summary>
    public virtual Gender? GetGender() =>
        Sex switch
        {
            "F" => Gender.Female,
            "M" => Gender.Male,
            _ => null
        };

    /// <summary>Get the employees age</summary>
    /// <returns>True if retired, null on missing base data</returns>
    public virtual int? GetAge(DateTime? moment = null)
    {
        moment ??= Date.Now;
        return BirthDate?.Age(moment.Value.SubtractDays(1));
    }

    #endregion

    #region Employee Children

    /// <summary>Employee child count</summary>
    public int ChildCount => Function.GetTypeCaseValue<Employee<TNational, TCompany>, int?>().Safe();

    private SlotCollection<EmployeeChild> children;

    /// <summary>The employee children</summary>
    public SlotCollection<EmployeeChild> Children => children ??= new(Function, ChildCount);

    /// <summary>Get child</summary>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>The child</returns>
    public virtual EmployeeChild GetChild(string caseSlot) =>
        Children.Values.FirstOrDefault(
            x => string.Equals(caseSlot, x.CaseSlot));

    #endregion

}

#region Employee Classes

/// <summary>Swissdec employee child</summary>
public class EmployeeChild : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeChild(PayrollFunction function, string caseSlot) :
        base(function)
    {
        CaseSlot = !string.IsNullOrWhiteSpace(caseSlot) ? caseSlot : throw new ArgumentNullException(nameof(function));
    }

    /// <summary>The employee child case slot</summary>
    public string CaseSlot { get; }

    /// <summary>The employee child first name</summary>
    /// <remarks>Swissdec XML tag: Firstname</remarks>
    [SwissdecXmlTag("Firstname")]
    public string FirstName => Function.GetTypeCaseSlotValue<EmployeeChild, string>(CaseSlot);

    /// <summary>The employee child last name</summary>
    /// <remarks>Swissdec XML tag: Lastname</remarks>
    [SwissdecXmlTag("Lastname")]
    public string LastName => Function.GetTypeCaseSlotValue<EmployeeChild, string>(CaseSlot);
    /*
    /// <summary>The employee child AHV number</summary>
    /// <remarks>Swissdec XML tag: SV-AS-Number</remarks>
    [SwissdecXmlTag("SV-AS-Number")]
    public string AhvNumber => Function.GetTypeCaseSlotValue<EmployeeChild, string>(CaseSlot);
    */
    /// <summary>The employee child birth date</summary>
    /// <remarks>Swissdec XML tag: DateOfBirth</remarks>
    [SwissdecXmlTag("DateOfBirth")]
    public DateTime? BirthDate => Function.GetTypeCaseSlotValue<EmployeeChild, DateTime?>(CaseSlot);

    /// <summary>The employee child sex</summary>
    /// <remarks>Swissdec XML tag: Sex</remarks>
    [SwissdecXmlTag("Sex")]
    public string Sex => Function.GetTypeCaseSlotValue<EmployeeChild, string>(CaseSlot);

    /// <summary>The employee child FAK start date</summary>
    /// <remarks>Swissdec XML tag: FakStart</remarks>
    [SwissdecXmlTag("FakStart")]
    public DateTime? FakStartDate => Function.GetTypeCaseSlotValue<EmployeeChild, DateTime?>(CaseSlot);

    /// <summary>The employee child FAK end date</summary>
    /// <remarks>Swissdec XML tag: FakEnd</remarks>
    [SwissdecXmlTag("FakEnd")]
    public DateTime? FakEndDate => Function.GetTypeCaseSlotValue<EmployeeChild, DateTime?>(CaseSlot);

    /// <summary>The employee child allowance</summary>
    public decimal? ChildAllowance => Function.GetTypeCaseSlotValue<EmployeeChild, decimal?>(CaseSlot);

    /// <summary>The employee child allowance back payment</summary>
    public decimal? ChildAllowanceBackPayment => Function.GetTypeCaseSlotValue<EmployeeChild, decimal?>(CaseSlot);

    /// <summary>The employee education allowance</summary>
    public decimal? EducationAllowance => Function.GetTypeCaseSlotValue<EmployeeChild, decimal?>(CaseSlot);

    /// <summary>The employee education allowance back payment</summary>
    public decimal? EducationAllowanceBackPayment => Function.GetTypeCaseSlotValue<EmployeeChild, decimal?>(CaseSlot);

    /// <summary>The employee child allowances directly payed by AHV-AK</summary>
    public decimal? AllowancesAhv => Function.GetTypeCaseSlotValue<EmployeeChild, decimal?>(CaseSlot);

    /// <summary>The employee child QST start date</summary>
    /// <remarks>Swissdec XML tag: QstStart</remarks>
    [SwissdecXmlTag("QstStart")]
    public DateTime? QstStartDate => Function.GetTypeCaseSlotValue<EmployeeChild, DateTime?>(CaseSlot);

    /// <summary>The employee child QST end date</summary>
    /// <remarks>Swissdec XML tag: QstEnd</remarks>
    [SwissdecXmlTag("QstEnd")]
    public DateTime? QstEndDate => Function.GetTypeCaseSlotValue<EmployeeChild, DateTime?>(CaseSlot);

    /// <summary>
    /// Get the FAK period
    /// </summary>
    public virtual DatePeriod GetFakPeriod()
    {
        // allowances only valid between FakStartDate & FakEndDate
        if (!FakStartDate.HasValue || !FakEndDate.HasValue)
        {
            return null;
        }
        // check if Function.Period is within FakStartDate & FakEndDate
        return new(FakStartDate.Value.AddDays(-1), FakEndDate.Value.AddDays(1));
    }
}

/// <summary>Swissdec employee address</summary>
public class EmployeeAddress : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeAddress(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>The employee country</summary>
    /// <remarks>Swissdec XML tag: Country</remarks>
    [SwissdecXmlTag("Country")]
    public string Country => Function.GetTypeCaseValue<EmployeeAddress, string>();

    /// <summary>The employee complementary line</summary>
    /// <remarks>Swissdec XML tag: ComplementaryLine</remarks>
    [SwissdecXmlTag("ComplementaryLine")]
    public string ComplementaryLine => Function.GetTypeCaseValue<EmployeeAddress, string>();

    /// <summary>The employee street</summary>
    /// <remarks>Swissdec XML tag: Street</remarks>
    [SwissdecXmlTag("Street")]
    public string Street => Function.GetTypeCaseValue<EmployeeAddress, string>();

    /// <summary>The employee postbox</summary>
    /// <remarks>Swissdec XML tag: Postbox</remarks>
    [SwissdecXmlTag("Postbox")]
    public string Postbox => Function.GetTypeCaseValue<EmployeeAddress, string>();

    /// <summary>The employee locality</summary>
    /// <remarks>Swissdec XML tag: Locality</remarks>
    [SwissdecXmlTag("Locality")]
    public string Locality => Function.GetTypeCaseValue<EmployeeAddress, string>();

    /// <summary>The employee zip code</summary>
    /// <remarks>Swissdec XML tag: ZipCode</remarks>
    [SwissdecXmlTag("ZipCode")]
    public string ZipCode => Function.GetTypeCaseValue<EmployeeAddress, string>();

    /// <summary>The employee city</summary>
    /// <remarks>Swissdec XML tag: City</remarks>
    [SwissdecXmlTag("City")]
    public string City => Function.GetTypeCaseValue<EmployeeAddress, string>();

    /// <summary>The employee canton</summary>
    /// <remarks>Swissdec XML tag: Canton</remarks>
    [SwissdecXmlTag("Canton")]
    public string Canton => Function.GetTypeCaseValue<EmployeeAddress, string>();

    /// <summary>The employee community id</summary>
    /// <remarks>Swissdec XML tag: MunicipalityID</remarks>
    [SwissdecXmlTag("MunicipalityID")]
    public int? CommunityId => Function.GetTypeCaseValue<EmployeeAddress, int?>();
}

/// <summary>Swissdec employee partner</summary>
public class EmployeePartner : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeePartner(PayrollFunction function) :
        base(function)
    {
        Address = new(Function);
    }

    /// <summary>Employee partner address</summary>
    public EmployeePartnerAddress Address { get; }

    /// <summary>The employee partner AHV number</summary>
    /// <remarks>Swissdec XML tag: SV-AS-Number</remarks>
    [SwissdecXmlTag("SV-AS-Number")]
    public string AhvNumber => Function.GetTypeCaseValue<EmployeePartner, string>();

    /// <summary>The employee partner first name</summary>
    /// <remarks>Swissdec XML tag: Firstname</remarks>
    [SwissdecXmlTag("Firstname")]
    public string FirstName => Function.GetTypeCaseValue<EmployeePartner, string>();

    /// <summary>The employee partner last name</summary>
    /// <remarks>Swissdec XML tag: Lastname</remarks>
    [SwissdecXmlTag("Lastname")]
    public string LastName => Function.GetTypeCaseValue<EmployeePartner, string>();

    /// <summary>The employee partner birth date</summary>
    /// <remarks>Swissdec XML tag: DateOfBirth</remarks>
    [SwissdecXmlTag("DateOfBirth")]
    public DateTime? BirthDate => Function.GetTypeCaseValue<EmployeePartner, DateTime?>();

    /// <summary>The employee partner residence</summary>
    /// <remarks>Swissdec XML tag: Residence</remarks>
    [SwissdecXmlTag("Residence")]
    public string Residence => Function.GetTypeCaseValue<EmployeePartner, string>();
}

/// <summary>Swissdec employee partner address</summary>
public class EmployeePartnerAddress : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeePartnerAddress(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>The employee partner address country</summary>
    /// <remarks>Swissdec XML tag: Country</remarks>
    [SwissdecXmlTag("Country")]
    public string Country => Function.GetTypeCaseValue<EmployeePartnerAddress, string>();

    /// <summary>The employee partner address complementary line</summary>
    /// <remarks>Swissdec XML tag: ComplementaryLine</remarks>
    [SwissdecXmlTag("ComplementaryLine")]
    public string ComplementaryLine => Function.GetTypeCaseValue<EmployeePartnerAddress, string>();

    /// <summary>The employee partner address street</summary>
    /// <remarks>Swissdec XML tag: Street</remarks>
    [SwissdecXmlTag("Street")]
    public string Street => Function.GetTypeCaseValue<EmployeePartnerAddress, string>();

    /// <summary>The employee partner address postbox</summary>
    /// <remarks>Swissdec XML tag: Postbox</remarks>
    [SwissdecXmlTag("Postbox")]
    public string Postbox => Function.GetTypeCaseValue<EmployeePartnerAddress, string>();

    /// <summary>The employee partner address locality</summary>
    /// <remarks>Swissdec XML tag: Locality</remarks>
    [SwissdecXmlTag("Locality")]
    public string Locality => Function.GetTypeCaseValue<EmployeePartnerAddress, string>();

    /// <summary>The employee partner address zip code</summary>
    /// <remarks>Swissdec XML tag: ZipCode</remarks>
    [SwissdecXmlTag("ZipCode")]
    public string ZipCode => Function.GetTypeCaseValue<EmployeePartnerAddress, string>();

    /// <summary>The employee partner address city</summary>
    /// <remarks>Swissdec XML tag: City</remarks>
    [SwissdecXmlTag("City")]
    public string City => Function.GetTypeCaseValue<EmployeePartnerAddress, string>();

    /// <summary>The employee partner address canton</summary>
    /// <remarks>Swissdec XML tag: Canton</remarks>
    [SwissdecXmlTag("Canton")]
    public string Canton => Function.GetTypeCaseValue<EmployeePartnerAddress, string>();

    /// <summary>The employee partner address community id</summary>
    /// <remarks>Swissdec XML tag: MunicipalityID</remarks>
    [SwissdecXmlTag("MunicipalityID")]
    public int? CommunityId => Function.GetTypeCaseValue<EmployeePartnerAddress, int?>();
}

/// <summary>Swissdec employee border crosses</summary>
public class EmployeeBorderCrosses : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeBorderCrosses(PayrollFunction function) :
        base(function)
    {
        FrenchItaly = new(function);
    }

    /// <summary>Swissdec employee border crosses french/italy</summary>
    public EmployeeBorderCrossesFrenchItaly FrenchItaly { get; }

    /// <summary>The employee border crosses residence kind</summary>
    /// <remarks>Swissdec XML tag: KindOfResidence</remarks>
    [SwissdecXmlTag("KindOfResidence")]
    public string ResidenceKind => Function.GetTypeCaseValue<EmployeeBorderCrosses, string>();

    /// <summary>The employee border crosses country</summary>
    /// <remarks>Swissdec XML tag: Country</remarks>
    [SwissdecXmlTag("Country")]
    public string Country => Function.GetTypeCaseValue<EmployeeBorderCrosses, string>();

    /// <summary>The employee border crosses complementary line</summary>
    /// <remarks>Swissdec XML tag: ComplementaryLine</remarks>
    [SwissdecXmlTag("ComplementaryLine")]
    public string ComplementaryLine => Function.GetTypeCaseValue<EmployeeBorderCrosses, string>();

    /// <summary>The employee border crosses street</summary>
    /// <remarks>Swissdec XML tag: Street</remarks>
    [SwissdecXmlTag("Street")]
    public string Street => Function.GetTypeCaseValue<EmployeeBorderCrosses, string>();

    /// <summary>The employee border crosses postbox</summary>
    /// <remarks>Swissdec XML tag: Postbox</remarks>
    [SwissdecXmlTag("Postbox")]
    public string Postbox => Function.GetTypeCaseValue<EmployeeBorderCrosses, string>();

    /// <summary>The employee border crosses locality</summary>
    /// <remarks>Swissdec XML tag: Locality</remarks>
    [SwissdecXmlTag("Locality")]
    public string Locality => Function.GetTypeCaseValue<EmployeeBorderCrosses, string>();

    /// <summary>The employee border crosses zip code</summary>
    /// <remarks>Swissdec XML tag: ZipCode</remarks>
    [SwissdecXmlTag("ZipCode")]
    public string ZipCode => Function.GetTypeCaseValue<EmployeeBorderCrosses, string>();

    /// <summary>The employee border crosses city</summary>
    /// <remarks>Swissdec XML tag: City</remarks>
    [SwissdecXmlTag("City")]
    public string City => Function.GetTypeCaseValue<EmployeeBorderCrosses, string>();

    /// <summary>The employee border crosses canton</summary>
    /// <remarks>Swissdec XML tag: Canton</remarks>
    [SwissdecXmlTag("Canton")]
    public string Canton => Function.GetTypeCaseValue<EmployeeBorderCrosses, string>();

    /// <summary>The employee border crosses community id</summary>
    /// <remarks>Swissdec XML tag: MunicipalityID</remarks>
    [SwissdecXmlTag("MunicipalityID")]
    public int? CommunityId => Function.GetTypeCaseValue<EmployeeBorderCrosses, int?>();
}

/// <summary>Swissdec employee border crosses french/italy</summary>
public class EmployeeBorderCrossesFrenchItaly : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeBorderCrossesFrenchItaly(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>The employee border crosses french/italy QST source canton</summary>
    /// <remarks>Swissdec XML tag: TaxAtSourceCanton</remarks>
    [SwissdecXmlTag("TaxAtSourceCanton")]
    public string QstSourceCanton => Function.GetTypeCaseValue<EmployeeBorderCrossesFrenchItaly, string>();

    /// <summary>The employee border crosses french/italy residence abroad country</summary>
    /// <remarks>Swissdec XML tag: ResidenceAbroadCountry</remarks>
    [SwissdecXmlTag("ResidenceAbroadCountry")]
    public string ResidenceAbroadCountry => Function.GetTypeCaseValue<EmployeeBorderCrossesFrenchItaly, string>();

    /// <summary>The employee border crosses french/italy place of birth</summary>
    /// <remarks>Swissdec XML tag: PlaceOfBirth</remarks>
    [SwissdecXmlTag("PlaceOfBirth")]
    public string PlaceOfBirth => Function.GetTypeCaseValue<EmployeeBorderCrossesFrenchItaly, string>();

    /// <summary>The employee border crosses french/italy tax number</summary>
    /// <remarks>Swissdec XML tag: TaxID</remarks>
    [SwissdecXmlTag("TaxID")]
    public string TaxNumber => Function.GetTypeCaseValue<EmployeeBorderCrossesFrenchItaly, string>();

    /// <summary>The employee border crosses french/italy since date</summary>
    /// <remarks>Swissdec XML tag: CrossborderValidAsOf</remarks>
    [SwissdecXmlTag("CrossborderValidAsOf")]
    public DateTime? SinceDate => Function.GetTypeCaseValue<EmployeeBorderCrossesFrenchItaly, DateTime?>();
}

/// <summary>Swissdec employee QST</summary>
public class EmployeeQst : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeQst(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>QST lookup: tax rate name</summary>
    public string QstLookupTaxRate => "TaxRate";

    /// <summary>Get the QST lookup name</summary>
    /// <param name="canton">The canton</param>
    public string GetQstLookupName(string canton)
    {
        if (string.IsNullOrWhiteSpace(canton) || canton.Length != 2)
        {
            return null;
        }
        // correct spelling of canton short code (correct format -> Xx)
        return string.Concat("Qst".ToNamespace(), char.ToUpper(canton[0]), char.ToLower(canton[1]));
    }

    /// <summary>The employee QST tax canton</summary>
    /// <remarks>Swissdec XML tag: TaxAtSourceCanton</remarks>
    [SwissdecXmlTag("TaxAtSourceCanton")]
    public string TaxCanton => Function.GetTypeCaseValue<EmployeeQst, string>();

    /// <summary>The employee QST effective workdays</summary>
    public int EffectiveWorkDays => Function.GetTypeCaseValue<EmployeeQst, int>();

    /// <summary>The employee QST workdays in Switzerland</summary>
    public int WorkDaysSwitzerland => Function.GetTypeCaseValue<EmployeeQst, int>();

    /// <summary>The employee QST community id</summary>
    /// <remarks>Swissdec XML tag: TaxAtSourceMunicipalityID</remarks>
    [SwissdecXmlTag("TaxAtSourceMunicipalityID")]
    public int? CommunityId => Function.GetTypeCaseValue<EmployeeQst, int?>();

    /// <summary>The employee QST tax code</summary>
    /// <remarks>Swissdec XML tag: TaxAtSourceCode</remarks>
    [SwissdecXmlTag("TaxAtSourceCode")]
    public string TaxCode => Function.GetTypeCaseValue<EmployeeQst, string>();

    /// <summary>The employee QST predefined categories</summary>
    /// <remarks>Swissdec XML tag: CategoryPredefined</remarks>
    [SwissdecXmlTag("CategoryPredefined")]
    public string PredefinedCategories => Function.GetTypeCaseValue<EmployeeQst, string>();

    /// <summary>The employee QST category open toggle</summary>
    /// <remarks>Swissdec XML tag: CategoryOpen</remarks>
    [SwissdecXmlTag("CategoryOpen")]
    public bool? CategoryOpen => Function.GetTypeCaseValue<EmployeeQst, bool?>();

    /// <summary>The employee QST denomination</summary>
    /// <remarks>Swissdec XML tag: Denomination</remarks>
    [SwissdecXmlTag("Denomination")]
    public string Denomination => Function.GetTypeCaseValue<EmployeeQst, string>();

    /// <summary>The employee QST other activities</summary>
    /// <remarks>Swissdec XML tag: OtherActivities</remarks>
    [SwissdecXmlTag("OtherActivities")]
    public decimal? OtherActivities => Function.GetTypeCaseValue<EmployeeQst, decimal?>();

    /// <summary>The employee QST extrapolate toggle</summary>
    public bool? Extrapolate => Function.GetTypeCaseValue<EmployeeQst, bool?>();

    /// <summary>The employee QST median toggle</summary>
    public bool? Median => Function.GetTypeCaseValue<EmployeeQst, bool?>();

    /// <summary>The employee QST single parent family toggle</summary>
    /// <remarks>Swissdec XML tag: SingleParentFamily</remarks>
    [SwissdecXmlTag("SingleParentFamily")]
    public bool? SingleParentFamily => Function.GetTypeCaseValue<EmployeeQst, bool?>();
}

/// <summary>Swissdec qst lookup item</summary>
public class QstLookupItem
{
    /// <summary>Qst code</summary>
    public string QstCode { get; set; }

    /// <summary>Qst canton</summary>
    public string QstCanton { get; set; }

    /// <summary>Qst calculation cycle</summary>
    public QstCalculationCycle QstCycle { get; set; }

    /// <summary>Get the lookup result tags</summary>
    public List<string> GetResultTags() =>
        new() { QstCode, QstCanton, QstCycle.ToQstCycleCode() };

    /// <summary>Get the lookup result filter tags (without QST code)</summary>
    public List<string> GetResultFilterTags() =>
        new() { QstCanton, QstCycle.ToQstCycleCode() };
}

/// <summary>Swissdec employee AHV insurance</summary>
public class EmployeeAhvInsurance : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeAhvInsurance(PayrollFunction function) :
        base(function)
    {
        NegativeTax = new(function);
    }

    /// <summary>Employee AHV negative</summary>
    public EmployeeAhvNegativeTax NegativeTax { get; }

    /// <summary>Swissdec employee AHV insurance institution id</summary>
    /// <remarks>Swissdec XML tag: InstitutionIDRef</remarks>
    [SwissdecXmlTag("InstitutionIDRef")]
    public string InstitutionId => Function.GetTypeCaseValue<EmployeeAhvInsurance, string>();

    /// <summary>Swissdec employee AHV insurance special case  toggle</summary>
    public bool? SpecialCase => Function.GetTypeCaseValue<EmployeeAhvInsurance, bool?>();
}

/// <summary>Swissdec employee AHV negative tax</summary>
public class EmployeeAhvNegativeTax : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeAhvNegativeTax(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>Swissdec employee AHV negative tax delivery date</summary>
    /// <remarks>Swissdec XML tag: DeliveryDate</remarks>
    [SwissdecXmlTag("DeliveryDate")]
    public DateTime? DeliveryDate => Function.GetTypeCaseValue<EmployeeAhvNegativeTax, DateTime?>();

    /// <summary>Swissdec employee AHV negative tax year income</summary>
    /// <remarks>Swissdec XML tag: SplitCurrentYearIncome</remarks>
    [SwissdecXmlTag("SplitCurrentYearIncome")]
    public decimal? YearIncome => Function.GetTypeCaseValue<EmployeeAhvNegativeTax, decimal?>();

    /// <summary>Swissdec employee AHV negative tax income 1</summary>
    /// <remarks>Swissdec XML tag: Income</remarks>
    [SwissdecXmlTag("Income")]
    public decimal? Income1 => Function.GetTypeCaseValue<EmployeeAhvNegativeTax, decimal?>();

    /// <summary>Swissdec employee AHV negative tax income 1 from date</summary>
    /// <remarks>Swissdec XML tag: from</remarks>
    [SwissdecXmlTag("from")]
    public DateTime? Income1FromDate => Function.GetTypeCaseValue<EmployeeAhvNegativeTax, DateTime?>();

    /// <summary>Swissdec employee AHV negative tax income 1 until date</summary>
    /// <remarks>Swissdec XML tag: until</remarks>
    [SwissdecXmlTag("until")]
    public DateTime? Income1UntilDate => Function.GetTypeCaseValue<EmployeeAhvNegativeTax, DateTime?>();

    /// <summary>Swissdec employee AHV negative tax income 2</summary>
    /// <remarks>Swissdec XML tag: Income</remarks>
    [SwissdecXmlTag("Income")]
    public decimal? Income2 => Function.GetTypeCaseValue<EmployeeAhvNegativeTax, decimal?>();

    /// <summary>Swissdec employee AHV negative tax income 2 from date</summary>
    /// <remarks>Swissdec XML tag: from</remarks>
    [SwissdecXmlTag("from")]
    public DateTime? Income2FromDate => Function.GetTypeCaseValue<EmployeeAhvNegativeTax, DateTime?>();

    /// <summary>Swissdec employee AHV negative tax income 2 until date</summary>
    /// <remarks>Swissdec XML tag: until</remarks>
    [SwissdecXmlTag("until")]
    public DateTime? Income2UntilDate => Function.GetTypeCaseValue<EmployeeAhvNegativeTax, DateTime?>();

    /// <summary>Swissdec employee AHV negative tax income 3</summary>
    /// <remarks>Swissdec XML tag: Income</remarks>
    [SwissdecXmlTag("Income")]
    public decimal? Income3 => Function.GetTypeCaseValue<EmployeeAhvNegativeTax, decimal?>();

    /// <summary>Swissdec employee AHV negative tax income 3 from date</summary>
    /// <remarks>Swissdec XML tag: from</remarks>
    [SwissdecXmlTag("from")]
    public DateTime? Income3FromDate => Function.GetTypeCaseValue<EmployeeAhvNegativeTax, DateTime?>();

    /// <summary>Swissdec employee AHV negative tax income 3 until date</summary>
    /// <remarks>Swissdec XML tag: until</remarks>
    [SwissdecXmlTag("until")]
    public DateTime? Income3UntilDate => Function.GetTypeCaseValue<EmployeeAhvNegativeTax, DateTime?>();
}

/// <summary>Swissdec employee UVG insurance</summary>
public class EmployeeUvgInsurance : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeUvgInsurance(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>Swissdec employee UVG insurance institution id (insurance name)</summary>
    /// <remarks>Swissdec XML tag: InstitutionIDRef</remarks>
    [SwissdecXmlTag("InstitutionIDRef")]
    public string InstitutionId => Function.GetTypeCaseValue<EmployeeUvgInsurance, string>();

    /// <summary>Swissdec employee UVG insurance code</summary>
    /// <remarks>Swissdec XML tag: UVG-LAA-Code | BranchIdentifier</remarks>
    [SwissdecXmlTag("UVG-LAA-Code", "BranchIdentifier")]
    public string Code => Function.GetTypeCaseValue<EmployeeUvgInsurance, string>();

    /// <summary>Get the UVG insurance type</summary>
    /// <param name="uvgCode">The UVG code</param>
    public static UvgInsuranceType? GetInsuranceType(string uvgCode)
    {
        if (string.IsNullOrWhiteSpace(uvgCode) || uvgCode.Length != 2)
        {
            return null;
        }

        return uvgCode[1] switch
        {
            '0' => UvgInsuranceType.NotInsured,
            '1' => UvgInsuranceType.InsuredShared,
            '2' => UvgInsuranceType.InsuredCompany,
            '3' => UvgInsuranceType.InsuredCompanyOnly,
            _ => null
        };
    }

    /// <summary>Build UVG insurance code</summary>
    /// <param name="insuranceClass">The insurance class</param>
    /// <param name="type">The insurance type</param>
    public static string BuildCode(char insuranceClass, UvgInsuranceType type)
    {
        return type switch
        {
            UvgInsuranceType.NotInsured => insuranceClass + "0",
            UvgInsuranceType.InsuredShared => insuranceClass + "1",
            UvgInsuranceType.InsuredCompany => insuranceClass + "2",
            UvgInsuranceType.InsuredCompanyOnly => insuranceClass + "3",
            _ => null
        };
    }
}

/// <summary>Swissdec employee UVGZ</summary>
public class EmployeeUvgz : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeUvgz(PayrollFunction function) :
        base(function)
    {
        Insurances = new(function, InsuranceCount);
    }

    /// <summary>The insurances</summary>
    public SlotCollection<EmployeeUvgzInsurance> Insurances { get; }

    /// <summary>The insurance count</summary>
    public int InsuranceCount => Function.GetTypeCaseValue<EmployeeUvgz, int?>().Safe();

    /// <summary>Get insurance</summary>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>The insurance</returns>
    public EmployeeUvgzInsurance GetInsurance(string caseSlot) =>
        Insurances.Values.FirstOrDefault(x => string.Equals(caseSlot, x.CaseSlot));
}

/// <summary>Swissdec employee UVGZ insurance</summary>
public class EmployeeUvgzInsurance : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeUvgzInsurance(PayrollFunction function, string caseSlot) :
        base(function)
    {
        CaseSlot = !string.IsNullOrWhiteSpace(caseSlot) ? caseSlot : throw new ArgumentNullException(nameof(function));
    }

    /// <summary>The employee address case slot</summary>
    public string CaseSlot { get; }

    /// <summary>Swissdec employee UVGZ insurance institution id</summary>
    /// <remarks>Swissdec XML tag: InstitutionIDRef</remarks>
    [SwissdecXmlTag("InstitutionIDRef")]
    public string InstitutionId => Function.GetTypeCaseSlotValue<EmployeeUvgzInsurance, string>(CaseSlot);

    /// <summary>Swissdec employee UVGZ insurance code</summary>
    /// <remarks>Swissdec XML tag: UVGZ-LAAC-Code | CategoryCode</remarks>
    [SwissdecXmlTag("UVGZ-LAAC-Code", "CategoryCode")]
    public string Code => Function.GetTypeCaseSlotValue<EmployeeUvgzInsurance, string>(CaseSlot);
}

/// <summary>Swissdec employee KTG</summary>
public class EmployeeKtg : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeKtg(PayrollFunction function) :
        base(function)
    {
        Insurances = new(function, InsuranceCount);
    }

    /// <summary>The insurances</summary>
    public SlotCollection<EmployeeKtgInsurance> Insurances { get; }

    /// <summary>The insurance count</summary>
    public int InsuranceCount => Function.GetTypeCaseValue<EmployeeKtg, int?>().Safe();

    /// <summary>Get insurance</summary>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>The insurance</returns>
    public EmployeeKtgInsurance GetInsurance(string caseSlot) =>
        Insurances.Values.FirstOrDefault(x => string.Equals(caseSlot, x.CaseSlot));
}

/// <summary>Swissdec employee KTG insurance</summary>
public class EmployeeKtgInsurance : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeKtgInsurance(PayrollFunction function, string caseSlot) :
        base(function)
    {
        if (string.IsNullOrWhiteSpace(caseSlot))
        {
            throw new ArgumentNullException(nameof(caseSlot));
        }
        CaseSlot = caseSlot;
    }

    /// <summary>The employee address case slot</summary>
    public string CaseSlot { get; }

    /// <summary>Swissdec employee KTG insurance institution id</summary>
    /// <remarks>Swissdec XML tag: InstitutionIDRef</remarks>
    [SwissdecXmlTag("InstitutionIDRef")]
    public string InstitutionId => Function.GetTypeCaseSlotValue<EmployeeKtgInsurance, string>(CaseSlot);

    /// <summary>Swissdec employee KTG insurance code</summary>
    /// <remarks>Swissdec XML tag: KTG-AMC-Code | CategoryCode</remarks>
    [SwissdecXmlTag("KTG-AMC-Code", "CategoryCode")]
    public string Code => Function.GetTypeCaseSlotValue<EmployeeKtgInsurance, string>(CaseSlot);
}

/// <summary>Swissdec employee BVG</summary>
public class EmployeeBvg : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeBvg(PayrollFunction function) :
        base(function)
    {
        Insurances = new(function, InsuranceCount);
    }

    /// <summary>The insurances</summary>
    public SlotCollection<EmployeeBvgInsurance> Insurances { get; }

    /// <summary>The insurance count</summary>
    public int InsuranceCount => Function.GetTypeCaseValue<EmployeeBvg, int?>().Safe();

    /// <summary>Employee BVG PF/LOB contribution</summary>
    public decimal? PfLobContribution => Function.GetTypeCaseValue<EmployeeBvg, decimal?>();
    /// <summary>Employee BVG PF/LOB redemption contribution</summary>
    public decimal? PfLobRedemptionContribution => Function.GetTypeCaseValue<EmployeeBvg, decimal?>();

    /// <summary>Get insurance</summary>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>The insurance</returns>
    public EmployeeBvgInsurance GetInsurance(string caseSlot) =>
        Insurances.Values.FirstOrDefault(x => string.Equals(caseSlot, x.CaseSlot));
}

/// <summary>Swissdec employee BVG insurance</summary>
public class EmployeeBvgInsurance : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeBvgInsurance(PayrollFunction function, string caseSlot) :
        base(function)
    {
        CaseSlot = !string.IsNullOrWhiteSpace(caseSlot) ? caseSlot : throw new ArgumentNullException(nameof(function));
    }

    /// <summary>The employee address case slot</summary>
    public string CaseSlot { get; }

    /// <summary>Swissdec employee BVG insurance institution id</summary>
    /// <remarks>Swissdec XML tag: InstitutionIDRef</remarks>
    [SwissdecXmlTag("InstitutionIDRef")]
    public string InstitutionId => Function.GetTypeCaseValue<EmployeeBvgInsurance, string>();

    /// <summary>Swissdec employee BVG insurance code</summary>
    /// <remarks>Swissdec XML tag: BVG-LPP-Code | CategoryCode</remarks>
    [SwissdecXmlTag("BVG-LPP-Code", "CategoryCode")]
    public string Code => Function.GetTypeCaseValue<EmployeeBvgInsurance, string>();
}

/// <summary>Swissdec employee salary certificate</summary>
public class EmployeeSalaryCertificate : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeSalaryCertificate(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>Employee salary certificate tax annuity toggle</summary>
    /// <remarks>Swissdec XML tag: TaxAnnuity</remarks>
    [SwissdecXmlTag("TaxAnnuity")]
    public bool? TaxAnnuity => Function.GetTypeCaseValue<EmployeeSalaryCertificate, bool?>();

    /// <summary>Employee salary certificate free transport toggle</summary>
    /// <remarks>Swissdec XML tag: FreeTransport</remarks>
    [SwissdecXmlTag("FreeTransport")]
    public bool? FreeTransport => Function.GetTypeCaseValue<EmployeeSalaryCertificate, bool?>();

    /// <summary>Employee salary certificate food benefits toggle</summary>
    /// <remarks>Swissdec XML tag: CanteenLunchCheck</remarks>
    [SwissdecXmlTag("CanteenLunchCheck")]
    public bool? FoodBenefits => Function.GetTypeCaseValue<EmployeeSalaryCertificate, bool?>();

    /// <summary>Employee salary certificate expatriate ruling toggle</summary>
    /// <remarks>Swissdec XML tag: ExpatriateRuling</remarks>
    [SwissdecXmlTag("ExpatriateRuling")]
    public bool? ExpatriateRuling => Function.GetTypeCaseValue<EmployeeSalaryCertificate, bool?>();

    /// <summary>Employee salary certificate external work</summary>
    /// <remarks>Swissdec XML tag: PercentageExternalWork</remarks>
    [SwissdecXmlTag("PercentageExternalWork")]
    public decimal? ExternalWorkPercent => Function.GetTypeCaseValue<EmployeeSalaryCertificate, decimal?>();

    /// <summary>Employee salary certificate remark</summary>
    /// <remarks>Swissdec XML tag: Remark</remarks>
    [SwissdecXmlTag("Remark")]
    public string Remark => Function.GetTypeCaseValue<EmployeeSalaryCertificate, string>();

    /// <summary>Employee salary certificate with regulation toggle</summary>
    /// <remarks>Swissdec XML tag: WithRegulation</remarks>
    [SwissdecXmlTag("WithRegulation")]
    public bool? WithRegulation => Function.GetTypeCaseValue<EmployeeSalaryCertificate, bool?>();

    /// <summary>Employee salary certificate guidance toggle</summary>
    /// <remarks>Swissdec XML tag: Guidance</remarks>
    [SwissdecXmlTag("Guidance")]
    public bool? Guidance => Function.GetTypeCaseValue<EmployeeSalaryCertificate, bool?>();

    /// <summary>Employee salary certificate staff share market value toggle</summary>
    /// <remarks>Swissdec XML tag: StaffShareMarketValue</remarks>
    [SwissdecXmlTag("StaffShareMarketValue")]
    public bool? StaffShareMarketValue => Function.GetTypeCaseValue<EmployeeSalaryCertificate, bool?>();

    /// <summary>Employee salary certificate expatriate staff share without taxable income toggle</summary>
    /// <remarks>Swissdec XML tag: StaffShareWithoutTaxableIncome</remarks>
    [SwissdecXmlTag("StaffShareWithoutTaxableIncome")]
    public bool? StaffShareWithoutTaxableIncome => Function.GetTypeCaseValue<EmployeeSalaryCertificate, bool?>();

    /// <summary>Employee salary certificate AHV child allowance</summary>
    /// <remarks>Swissdec XML tag: DiPartPensionPerAHV-AVS</remarks>
    [SwissdecXmlTag("ChildAllowancePerAHV-AVS")]
    public decimal? AhvChildAllowance => Function.GetTypeCaseValue<EmployeeSalaryCertificate, decimal?>();

    /// <summary>Employee salary certificate relocation costs</summary>
    /// <remarks>Swissdec XML tag: RelocationCosts</remarks>
    [SwissdecXmlTag("RelocationCosts")]
    public decimal? RelocationCosts => Function.GetTypeCaseValue<EmployeeSalaryCertificate, decimal?>();

    /// <summary>Employee salary certificate company car toggle</summary>
    /// <remarks>Swissdec XML tag: CompanyCarClarify</remarks>
    [SwissdecXmlTag("CompanyCarClarify")]
    public bool? CompanyCar => Function.GetTypeCaseValue<EmployeeSalaryCertificate, bool?>();

    /// <summary>Employee salary certificate company car part</summary>
    /// <remarks>Swissdec XML tag: MinimalEmployeeCarPartPercentage</remarks>
    [SwissdecXmlTag("MinimalEmployeeCarPartPercentage")]
    public decimal? CompanyCarPartPercent => Function.GetTypeCaseValue<EmployeeSalaryCertificate, decimal?>();

    /// <summary>Employee salary certificate continued salary provision</summary>
    /// <remarks>Swissdec XML tag: ContinuedProvisionOfSalary</remarks>
    [SwissdecXmlTag("ContinuedProvisionOfSalary")]
    public string ContinuedSalaryProvision => Function.GetTypeCaseValue<EmployeeSalaryCertificate, string>();
}

/// <summary>Swissdec employee statistics</summary>
public class EmployeeStatistics : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeStatistics(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>Employee statistics eduction</summary>
    /// <remarks>Swissdec XML tag: Education</remarks>
    [SwissdecXmlTag("Education")]
    public string Education => Function.GetTypeCaseValue<EmployeeStatistics, string>();

    /// <summary>Employee statistics position</summary>
    /// <remarks>Swissdec XML tag: Position</remarks>
    [SwissdecXmlTag("Position")]
    public string Position => Function.GetTypeCaseValue<EmployeeStatistics, string>();

    /// <summary>Employee statistics job title</summary>
    /// <remarks>Swissdec XML tag: JobTitle</remarks>
    [SwissdecXmlTag("JobTitle")]
    public string JobTitle => Function.GetTypeCaseValue<EmployeeStatistics, string>();

    /// <summary>Employee statistics working time model</summary>
    /// <remarks>Swissdec XML tag: CompanyWeeklyHoursAndLessonsIDRef</remarks>
    [SwissdecXmlTag("CompanyWeeklyHoursAndLessonsIDRef")]
    public string WorkingTimeModel => Function.GetTypeCaseValue<EmployeeStatistics, string>();

    /// <summary>Employee statistics vacation days</summary>
    /// <remarks>Swissdec XML tag: LeaveEntitlement</remarks>
    [SwissdecXmlTag("LeaveEntitlement")]
    public int? VacationDays => Function.GetTypeCaseValue<EmployeeStatistics, int?>();

    /// <summary>Employee statistics temporary agency worker toggle</summary>
    /// <remarks>Swissdec XML tag: TemporaryAgencyWorker</remarks>
    [SwissdecXmlTag("TemporaryAgencyWorker")]
    public bool? TemporaryAgencyWorker => Function.GetTypeCaseValue<EmployeeStatistics, bool?>();

    /// <summary>Employee statistics permanent staff public admin toggle</summary>
    /// <remarks>Swissdec XML tag: PermanentStaffPublicAdmin</remarks>
    [SwissdecXmlTag("PermanentStaffPublicAdmin")]
    public bool? PermanentStaffPublicAdmin => Function.GetTypeCaseValue<EmployeeStatistics, bool?>();

    /// <summary>Employee statistics flex profiling toggle</summary>
    /// <remarks>Swissdec XML tag: FlexProfiling</remarks>
    [SwissdecXmlTag("FlexProfiling")]
    public bool? FlexProfiling => Function.GetTypeCaseValue<EmployeeStatistics, bool?>();

    /// <summary>Employee statistics contract</summary>
    /// <remarks>Swissdec XML tag: Contract</remarks>
    [SwissdecXmlTag("Contract")]
    public string Contract => Function.GetTypeCaseValue<EmployeeStatistics, string>();

    /// <summary>Employee statistics contractual monthly wage</summary>
    /// <remarks>Swissdec XML tag: ContractualMonthlyWage</remarks>
    [SwissdecXmlTag("ContractualMonthlyWage")]
    public decimal? ContractualMonthlyWage => Function.GetTypeCaseValue<EmployeeStatistics, decimal?>();

    /// <summary>Employee statistics contractual 13th</summary>
    /// <remarks>Swissdec XML tag: Contractual13thPercentage</remarks>
    [SwissdecXmlTag("Contractual13thPercentage")]
    public decimal? Contractual13thPercent => Function.GetTypeCaseValue<EmployeeStatistics, decimal?>();

    /// <summary>Employee statistics contractual 14th</summary>
    /// <remarks>Swissdec XML tag: Contractual14thPercentage</remarks>
    [SwissdecXmlTag("Contractual14thPercentage")]
    public decimal? Contractual14thPercent => Function.GetTypeCaseValue<EmployeeStatistics, decimal?>();

    /// <summary>Employee statistics contractual hourly wage</summary>
    /// <remarks>Swissdec XML tag: ContractualHourlyWage</remarks>
    [SwissdecXmlTag("ContractualHourlyWage")]
    public decimal? ContractualHourlyWage => Function.GetTypeCaseValue<EmployeeStatistics, decimal?>();

    /// <summary>Employee statistics contractual annual wage</summary>
    /// <remarks>Swissdec XML tag: ContractualAnnualWage</remarks>
    [SwissdecXmlTag("ContractualAnnualWage")]
    public decimal? ContractualAnnualWage => Function.GetTypeCaseValue<EmployeeStatistics, decimal?>();

    /// <summary>Employee statistics contractual vacation wage</summary>
    /// <remarks>Swissdec XML tag: ContractualVacationWage</remarks>
    [SwissdecXmlTag("ContractualVacationWage")]
    public decimal? ContractualVacationWage => Function.GetTypeCaseValue<EmployeeStatistics, decimal?>();

    /// <summary>Employee statistics public holiday compensation toggle</summary>
    /// <remarks>Swissdec XML tag: PublicHolidayCompensation</remarks>
    [SwissdecXmlTag("PublicHolidayCompensation")]
    public bool? PublicHolidayCompensation => Function.GetTypeCaseValue<EmployeeStatistics, bool?>();
}

/// <summary>Swissdec employee wage</summary>
public class EmployeeWage : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeWage(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>Employee wage correction</summary>
    public decimal? WageCorrection => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage weekly wage</summary>
    public decimal? WeeklyWage => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage remuneration</summary>
    public decimal? Remuneration => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage temporary staff salaries</summary>
    public decimal? TemporaryStaffSalaries => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage homebased work allowance</summary>
    public decimal? HomebasedWorkAllowance => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage cleaning salary</summary>
    public decimal? CleaningSalary => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage piecework wage</summary>
    public decimal? PieceworkWage => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage absence compensation</summary>
    public decimal? AbsenceCompensation => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage authorities and committee members</summary>
    public decimal? AuthoritiesAndCommitteeMembers => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage seniority allowance</summary>
    public decimal? SeniorityAllowance => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage function allowance</summary>
    public decimal? FunctionAllowance => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage representation allowance</summary>
    public decimal? RepresentationAllowance => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage residential allowance</summary>
    public decimal? ResidentialAllowance => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage cost of living allowance</summary>
    public decimal? CostOfLivingAllowance => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage family cost of living allowance</summary>
    public decimal? FamilyCostOfLivingAllowance => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage accommodation allowance</summary>
    public decimal? AccommodationAllowance => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage travel expenses reimbursement</summary>
    public decimal? TravelExpensesReimbursement => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage displacement allowance</summary>
    public decimal? DisplacementAllowance => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage holiday compensation</summary>
    public decimal? HolidayCompensation => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage public holiday compensation</summary>
    public decimal? PublicHolidayCompensation => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage employer facultative dsa</summary>
    public decimal? EmployerFacultativeDsa => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage employer facultative pf lob</summary>
    public decimal? EmployerFacultativePfLob => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage employer facultative redemption pf lob</summary>
    public decimal? EmployerFacultativeRedemptionPfLob => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage employer facultative health insurance</summary>
    public decimal? EmployerFacultativeHealthInsurance => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage employer facultative suva</summary>
    public decimal? EmployerFacultativeSuva => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage employer facultative sai</summary>
    public decimal? EmployerFacultativeSai => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage employer facultative pillar 3b</summary>
    public decimal? EmployerFacultativePillar3b => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage employer facultative pillar 3a</summary>
    public decimal? EmployerFacultativePillar3a => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage facultative withholding tax</summary>
    public decimal? FacultativeWithholdingTax => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage 13th monthly wage</summary>
    public decimal? MonthlyWage13th => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage 14th monthly wage</summary>
    public decimal? MonthlyWage14th => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage remuneration BoD</summary>
    public decimal? RemunerationBod => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage compensation BoD</summary>
    public decimal? CompensationBod => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage attendance fees BoD</summary>
    public decimal? AttendanceFeesBod => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage royalties BoD</summary>
    public decimal? RoyaltiesBod => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage taxable participation rights</summary>
    public decimal? TaxableParticipationRights => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage employee shares</summary>
    public decimal? EmployeeShares => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage employee options</summary>
    public decimal? EmployeeOptions => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage family allowance</summary>
    public decimal? FamilyAllowance => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage household allowance</summary>
    public decimal? HouseholdAllowance => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage birth allowance</summary>
    public decimal? BirthAllowance => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage wedding allowance</summary>
    public decimal? WeddingAllowance => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage custody allowance</summary>
    public decimal? CustodyAllowance => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
    /// <summary>Employee wage child allowances AHV</summary>
    public decimal? ChildAllowancesAhv => Function.GetTypeCaseValue<EmployeeWage, decimal?>();
}

/// <summary>Swissdec employee short-time work</summary>
public class EmployeeShortTimeWork : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeShortTimeWork(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>Employee short-time work deduction STW</summary>
    public decimal? DeductionStw => Function.GetTypeCaseValue<EmployeeShortTimeWork, decimal?>();
    /// <summary>Employee short-time work loss of wages STW</summary>
    public decimal? LossOfWagesStw => Function.GetTypeCaseValue<EmployeeShortTimeWork, decimal?>();
    /// <summary>Employee short-time work unemployment compensation</summary>
    public decimal? UnemploymentCompensation => Function.GetTypeCaseValue<EmployeeShortTimeWork, decimal?>();
    /// <summary>Employee short-time work waiting period STW</summary>
    public decimal? WaitingPeriodStw => Function.GetTypeCaseValue<EmployeeShortTimeWork, decimal?>();
}

/// <summary>Swissdec employee presence absence</summary>
public class EmployeePresenceAbsence : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeePresenceAbsence(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>Employee presence absence vacation payout</summary>
    public decimal? VacationPayout => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence vacation payout after withdrawal</summary>
    public decimal? VacationPayoutAfterWithdrawal => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence accident salary</summary>
    public decimal? AccidentSalary => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence illness salary</summary>
    public decimal? IllnessSalary => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence military service/cvil protection salary</summary>
    public decimal? MilitaryServiceCivilProtectionSalary => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence education/training salary</summary>
    public decimal? EducationTrainingSalary => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence IC daily allowance</summary>
    public decimal? IcDailyAllowance => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence military service insurance</summary>
    public decimal? MilitaryServiceInsurance => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence military supplement insurance</summary>
    public decimal? MilitarySupplementInsurance => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence Parifonds</summary>
    public decimal? Parifonds => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence military daily allowance</summary>
    public decimal? MilitaryDailyAllowance => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence military pension</summary>
    public decimal? MilitaryPension => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence DI daily allowance</summary>
    public decimal? DiDailyAllowance => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence DI pension</summary>
    public decimal? DiPension => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence SUVA daily allowance</summary>
    public decimal? SuvaDailyAllowance => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence SUVA pension</summary>
    public decimal? SuvaPension => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence daily sickness allowance</summary>
    public decimal? DailySicknessAllowance => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence maternity compensation</summary>
    public decimal? MaternityCompensation => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
    /// <summary>Employee presence absence daily allowance correction</summary>
    public decimal? DailyAllowanceCorrection => Function.GetTypeCaseValue<EmployeePresenceAbsence, decimal?>();
}

/// <summary>Swissdec employee one-time wage</summary>
public class EmployeeOneTimeWage : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeOneTimeWage(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>Employee one-time wage gratuity</summary>
    public decimal? Gratuity => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage christmas gratuity</summary>
    public decimal? ChristmasGratuity => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage bonus payment</summary>
    public decimal? BonusPayment => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage bonus payment</summary>
    public decimal? BonusPaymentPreviousYear => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage profit participation</summary>
    public decimal? ProfitParticipation => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage special allowance</summary>
    public decimal? SpecialAllowance => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage success bonus</summary>
    public decimal? SuccessBonus => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage performance bonus</summary>
    public decimal? PerformanceBonus => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage recognition bonus</summary>
    public decimal? RecognitionBonus => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage improvement suggestions</summary>
    public decimal? ImprovementSuggestions => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage revenue bonus</summary>
    public decimal? RevenueBonus => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage commission</summary>
    public decimal? Commission => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage presence bonus</summary>
    public decimal? PresenceBonus => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage gift for years of service</summary>
    public decimal? GiftForYearsOfService => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage jubilee gift</summary>
    public decimal? JubileeGift => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage fidelity bonus</summary>
    public decimal? FidelityBonus => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage loss prevention bonus</summary>
    public decimal? LossPreventionBonus => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage severance payment, free</summary>
    public decimal? SeverancePaymentFree => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage severance payment, paid</summary>
    public decimal? SeverancePaymentPaid => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage provident nature capital payment</summary>
    public decimal? ProvidentNatureCapitalPayment => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage capital payment, paid</summary>
    public decimal? CapitalPaymentPaid => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
    /// <summary>Employee one-time wage continued wages payment</summary>
    public decimal? ContinuedWagesPayment => Function.GetTypeCaseValue<EmployeeOneTimeWage, decimal?>();
}

/// <summary>Swissdec employee expense</summary>
public class EmployeeExpense : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeExpense(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>Employee expense meals free charge</summary>
    public decimal? MealsFreeCharge => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense lodging free charge</summary>
    public decimal? LodgingFreeCharge => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense accommodation free charge</summary>
    public decimal? AccommodationFreeCharge => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense fringe benefits car</summary>
    public decimal? FringeBenefitsCar => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense tip paid</summary>
    public decimal? TipPaid => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense rented flat price reduction</summary>
    public decimal? RentedFlatPriceReduction => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense expatriates payment in kind</summary>
    public decimal? ExpatriatesPaymentInKind => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense non-cash benefits</summary>
    public decimal? NonCashBenefits => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense further training</summary>
    public decimal? FurtherTraining => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense travel expenses</summary>
    public decimal? TravelExpenses => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense car expenses</summary>
    public decimal? CarExpenses => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense meals expenses</summary>
    public decimal? MealsExpenses => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense accommodation costs</summary>
    public decimal? AccommodationCosts => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense effective costs expatriates</summary>
    public decimal? EffectiveCostsExpatriates => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense effective costs remaining</summary>
    public decimal? EffectiveCostsRemaining => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense Lump professional expenses expatriates</summary>
    public decimal? LumpProfessionalExpensesExpatriates => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense flat-rate representation expenses</summary>
    public decimal? FlatRateRepresentationExpenses => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense flat-rate car expenses</summary>
    public decimal? FlatRateCarExpenses => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense flat-rate expenses expatriates</summary>
    public decimal? FlatRateExpensesExpatriates => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
    /// <summary>Employee expense flat-rate expenses remaining</summary>
    public decimal? FlatRateExpensesRemaining => Function.GetTypeCaseValue<EmployeeExpense, decimal?>();
}

/// <summary>Swissdec employee activity</summary>
public class EmployeeActivity : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public EmployeeActivity(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>Employee activity worked hours</summary>
    public decimal? WorkedHours => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity worked lessons</summary>
    public decimal? WorkedLessons => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity additional work</summary>
    public decimal? AdditionalWork => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity overtime 125%</summary>
    public decimal? Overtime125Percent => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity overtime</summary>
    public decimal? Overtime => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity overtime after withdrawal</summary>
    public decimal? OvertimeAfterWithdrawal => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity shift bonus</summary>
    public decimal? ShiftBonus => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity Pikett remuneration</summary>
    public decimal? PikettRemuneration => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity assignment bonus</summary>
    public decimal? AssignmentBonus => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity sunday bonus</summary>
    public decimal? SundayBonus => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity inconvenience bonus</summary>
    public decimal? InconvenienceBonus => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity night-shift bonus</summary>
    public decimal? NightShiftBonus => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity night-work bonus</summary>
    public decimal? NightWorkBonus => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity construction site bonus</summary>
    public decimal? ConstructionSiteBonus => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity difficulty bonus</summary>
    public decimal? DifficultyBonus => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity dirty work bonus</summary>
    public decimal? DirtyWorkBonus => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity dust work bonus</summary>
    public decimal? DustWorkBonus => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity underground work bonus</summary>
    public decimal? UndergroundWorkBonus => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity propulsion bonus</summary>
    public decimal? PropulsionBonus => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity tunneling bonus</summary>
    public decimal? TunnelingBonus => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity tenacity bonus</summary>
    public decimal? TenacityBonus => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity appearance bonus</summary>
    public decimal? AppearanceBonus => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
    /// <summary>Employee activity non appearance compensation</summary>
    public decimal? NonAppearanceCompensation => Function.GetTypeCaseValue<EmployeeActivity, decimal?>();
}

#endregion
