/* Company */
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Function;
using System;
using System.Linq;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec Company</summary>
public class Company<TNational> : SwissdecBase<PayrollFunction>
    where TNational : National
{
    /// <summary>Swissdec national</summary>
    public TNational National { get; }

    /// <summary>Function constructor</summary>
    public Company(PayrollFunction function, TNational national) :
        base(function)
    {
        National = national ?? throw new ArgumentNullException(nameof(national));

        Address = new(Function);
        TrusteeAddress = new(Function);
        Ahv = new(Function);
        Certificate = new(Function);
        Statistics = new(Function);
    }

    /// <summary>Company address</summary>
    public virtual CompanyAddress Address { get; }

    /// <summary>Enable trustee address toggle</summary>
    public bool Trustee => Function.GetTypeCaseValue<Company<TNational>, bool?>().Safe();

    /// <summary>Company trustee address</summary>
    public CompanyTrusteeAddress TrusteeAddress { get; }

    /// <summary>Swissdec company AHV</summary>
    public CompanyAhv Ahv { get; }

    /// <summary>Company salary certificate</summary>
    public CompanySalaryCertificate Certificate { get; }

    /// <summary>Company statistics</summary>
    public CompanyStatistics Statistics { get; }

    #region Company FAK

    private CompanyFak fak;
    /// <summary>The payroll company FAK</summary>
    public virtual CompanyFak Fak => fak ??= new(Function);

    #endregion

    #region Company QST

    private CompanyQst qst;
    /// <summary>The payroll company QST</summary>
    public virtual CompanyQst Qst => qst ??= new(Function);

    #endregion

    #region Contract Provider

    private ContractProvider contractProvider;
    /// <summary>The payroll contract provider</summary>
    public virtual ContractProvider ContractProvider => contractProvider ??= new(Function);

    #endregion

    #region Contract Provider Contribution

    private ContractProviderContribution contractProviderContributions;
    /// <summary>The payroll contract provider contributions</summary>
    public virtual ContractProviderContribution ContractProviderContributions =>
        contractProviderContributions ??= new(Function);

    #endregion

}

#region Company Classes

/// <summary>Swissdec company AHV</summary>
public class CompanyAhv : SwissdecBase<PayrollFunction>
{

    /// <summary>Function constructor</summary>
    public CompanyAhv(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>The company AHV branch number</summary>
    /// <remarks>Swissdec XML tag: AK-CC-BranchNumber</remarks>
    [SwissdecXmlTag("AK-CC-BranchNumber")]
    public string BranchNumber => Function.GetTypeCaseValue<CompanyAhv, string>();

    /// <summary>The company AHV branch number</summary>
    /// <remarks>Swissdec XML tag: AK-CC-CustomerNumber</remarks>
    [SwissdecXmlTag("AK-CC-CustomerNumber")]
    public string CustomerNumber => Function.GetTypeCaseValue<CompanyAhv, string>();

    /// <summary>The company AHV sub number</summary>
    /// <remarks>Swissdec XML tag: AK-CC-SubNumber</remarks>
    [SwissdecXmlTag("AK-CC-SubNumber")]
    public string SubNumber => Function.GetTypeCaseValue<CompanyAhv, string>();

    /// <summary>The company AHV UVG name</summary>
    /// <remarks>Swissdec XML tag: Name</remarks>
    [SwissdecXmlTag("Name")]
    public string UvgName => Function.GetTypeCaseValue<CompanyAhv, string>();

    /// <summary>The company AHV UVG id</summary>
    /// <remarks>Swissdec XML tag: UID-BFS | Unknown</remarks>
    [SwissdecXmlTag("UID-BFS", "Unknown")]
    public string UvgId => Function.GetTypeCaseValue<CompanyAhv, string>();

    /// <summary>The company AHV UVG valid date</summary>
    /// <remarks>Swissdec XML tag: ValidAsOf</remarks>
    [SwissdecXmlTag("ValidAsOf")]
    public DateTime? UvgValidDate => Function.GetTypeCaseValue<CompanyAhv, DateTime?>();

    /// <summary>The company AHV BVG name</summary>
    /// <remarks>Swissdec XML tag: Name</remarks>
    [SwissdecXmlTag("Name")]
    public string BvgName => Function.GetTypeCaseValue<CompanyAhv, string>();

    /// <summary>The company AHV BVG id</summary>
    /// <remarks>Swissdec XML tag: UID-BFS | Unknown</remarks>
    [SwissdecXmlTag("UID-BFS", "Unknown")]
    public string BvgId => Function.GetTypeCaseValue<CompanyAhv, string>();

    /// <summary>The company AHV BVG valid date</summary>
    /// <remarks>Swissdec XML tag: ValidAsOf</remarks>
    [SwissdecXmlTag("ValidAsOf")]
    public DateTime? BvgValidDate => Function.GetTypeCaseValue<CompanyAhv, DateTime?>();
}

/// <summary>Swissdec company salary certificate</summary>
public class CompanySalaryCertificate : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public CompanySalaryCertificate(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>The company salary certificate expense date</summary>
    /// <remarks>Swissdec XML tag: WithRegulationAllowed</remarks>
    [SwissdecXmlTag("WithRegulationAllowed")]
    public DateTime? ExpenseDate => Function.GetTypeCaseValue<CompanySalaryCertificate, DateTime?>();

    /// <summary>The company salary certificate expense canton</summary>
    /// <remarks>Swissdec XML tag: WithRegulationCanton</remarks>
    [SwissdecXmlTag("WithRegulationCanton")]
    public string ExpenseCanton => Function.GetTypeCaseValue<CompanySalaryCertificate, string>();

    /// <summary>The company salary certificate employee participation date</summary>
    /// <remarks>Swissdec XML tag: StaffShareMarketValueAllowed</remarks>
    [SwissdecXmlTag("StaffShareMarketValueAllowed")]
    public DateTime? EmployeeParticipationDate => Function.GetTypeCaseValue<CompanySalaryCertificate, DateTime?>();

    /// <summary>The company salary certificate employee participation canton</summary>
    /// <remarks>Swissdec XML tag: StaffShareMarketValueCanton</remarks>
    [SwissdecXmlTag("StaffShareMarketValueCanton")]
    public string EmployeeParticipationCanton => Function.GetTypeCaseValue<CompanySalaryCertificate, string>();

    /// <summary>The company salary certificate expatriate ruling date</summary>
    /// <remarks>Swissdec XML tag: ExpatriaterulingAllowed</remarks>
    [SwissdecXmlTag("ExpatriaterulingAllowed")]
    public DateTime? ExpatriateRulingDate => Function.GetTypeCaseValue<CompanySalaryCertificate, DateTime?>();

    /// <summary>The company salary certificate expatriate ruling canton</summary>
    /// <remarks>Swissdec XML tag: ExpatriaterulingCanton</remarks>
    [SwissdecXmlTag("ExpatriaterulingCanton")]
    public string ExpatriateRulingCanton => Function.GetTypeCaseValue<CompanySalaryCertificate, string>();

    /// <summary>The company salary certificate other fringe benefits</summary>
    /// <remarks>Swissdec XML tag: OtherFringeBenefits</remarks>
    [SwissdecXmlTag("OtherFringeBenefits")]
    public string OtherFringeBenefits => Function.GetTypeCaseValue<CompanySalaryCertificate, string>();

    /// <summary>The company salary certificate guidance toggle</summary>
    /// <remarks>Swissdec XML tag: Guidance</remarks>
    [SwissdecXmlTag("Guidance")]
    public bool? Guidance => Function.GetTypeCaseValue<CompanySalaryCertificate, bool?>();
}

/// <summary>Swissdec company statistics</summary>
public class CompanyStatistics : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public CompanyStatistics(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>The company statistics pay agreement</summary>
    /// <remarks>Swissdec XML tag: PayAgreement</remarks>
    [SwissdecXmlTag("PayAgreement")]
    public string PayAgreement => Function.GetTypeCaseValue<CompanyStatistics, string>();

    /// <summary>The company statistics payroll unit</summary>
    /// <remarks>Swissdec XML tag: PayrollUnit</remarks>
    [SwissdecXmlTag("PayrollUnit")]
    public string PayrollUnit => Function.GetTypeCaseValue<CompanyStatistics, string>();

    /// <summary>The company statistics comment</summary>
    /// <remarks>Swissdec XML tag: Comment</remarks>
    [SwissdecXmlTag("Comment")]
    public string Comment => Function.GetTypeCaseValue<CompanyStatistics, string>();

    /// <summary>The company statistics leaveEntitlementDays 16 year old</summary>
    /// <remarks>Swissdec XML tag: LeaveEntitlement (Employee)</remarks>
    [SwissdecXmlTag("LeaveEntitlement")]
    public decimal? LeaveEntitlementDays16 => Function.GetTypeCaseValue<CompanyStatistics, decimal?>();

    /// <summary>The company statistics leaveEntitlementDays 50 year old</summary>
    /// <remarks>Swissdec XML tag: LeaveEntitlement (Employee)</remarks>
    [SwissdecXmlTag("LeaveEntitlement")]
    public decimal? LeaveEntitlementDays50 => Function.GetTypeCaseValue<CompanyStatistics, decimal?>();

    /// <summary>The company statistics leaveEntitlementDays 60 year old</summary>
    /// <remarks>Swissdec XML tag: LeaveEntitlement (Employee)</remarks>
    [SwissdecXmlTag("LeaveEntitlement")]
    public decimal? LeaveEntitlementDays60 => Function.GetTypeCaseValue<CompanyStatistics, decimal?>();
}

/// <summary>Swissdec company address</summary>
public class CompanyAddress : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public CompanyAddress(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>The company name</summary>
    /// <remarks>Swissdec XML tag: Name</remarks>
    [SwissdecXmlTag("Name")]
    public string Name => Function.GetTypeCaseValue<CompanyAddress, string>();

    /// <summary>The company first name</summary>
    /// <remarks>Swissdec XML tag: Firstname</remarks>
    [SwissdecXmlTag("Firstname")]
    public string FirstName => Function.GetTypeCaseValue<CompanyAddress, string>();

    /// <summary>The company last name</summary>
    /// <remarks>Swissdec XML tag: Lastname</remarks>
    [SwissdecXmlTag("Lastname")]
    public string LastName => Function.GetTypeCaseValue<CompanyAddress, string>();

    /// <summary>The company id</summary>
    /// <remarks>Swissdec XML tag: UID-BFS | Unknown</remarks>
    [SwissdecXmlTag("UID-BFS", "Unknown")]
    public string Id => Function.GetTypeCaseValue<CompanyAddress, string>();

    /// <summary>The company country</summary>
    /// <remarks>Swissdec XML tag: Country</remarks>
    [SwissdecXmlTag("Country")]
    public string Country => Function.GetTypeCaseValue<CompanyAddress, string>();

    /// <summary>The company complementary line</summary>
    /// <remarks>Swissdec XML tag: ComplementaryLine</remarks>
    [SwissdecXmlTag("ComplementaryLine")]
    public string ComplementaryLine => Function.GetTypeCaseValue<CompanyAddress, string>();

    /// <summary>The company street</summary>
    /// <remarks>Swissdec XML tag: Street</remarks>
    [SwissdecXmlTag("Street")]
    public string Street => Function.GetTypeCaseValue<CompanyAddress, string>();

    /// <summary>The company postbox</summary>
    /// <remarks>Swissdec XML tag: Postbox</remarks>
    [SwissdecXmlTag("Postbox")]
    public string Postbox => Function.GetTypeCaseValue<CompanyAddress, string>();

    /// <summary>The company locality</summary>
    /// <remarks>Swissdec XML tag: Locality</remarks>
    [SwissdecXmlTag("Locality")]
    public string Locality => Function.GetTypeCaseValue<CompanyAddress, string>();

    /// <summary>The company zip code</summary>
    /// <remarks>Swissdec XML tag: ZipCode</remarks>
    [SwissdecXmlTag("ZipCode")]
    public string ZipCode => Function.GetTypeCaseValue<CompanyAddress, string>();

    /// <summary>The company city</summary>
    /// <remarks>Swissdec XML tag: City</remarks>
    [SwissdecXmlTag("City")]
    public string City => Function.GetTypeCaseValue<CompanyAddress, string>();

    /// <summary>The company canton</summary>
    /// <remarks>Swissdec XML tag: Canton</remarks>
    [SwissdecXmlTag("Canton")]
    public string Canton => Function.GetTypeCaseValue<CompanyAddress, string>();

    /// <summary>The company community id</summary>
    /// <remarks>Swissdec XML tag: MunicipalityID</remarks>
    [SwissdecXmlTag("MunicipalityID")]
    public int? CommunityId => Function.GetTypeCaseValue<CompanyAddress, int?>();
}

/// <summary>Swissdec company trustee address</summary>
public class CompanyTrusteeAddress : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public CompanyTrusteeAddress(PayrollFunction function) :
        base(function)
    {
    }

    /// <summary>The company trustee name</summary>
    /// <remarks>Swissdec XML tag: Name</remarks>
    [SwissdecXmlTag("Name")]
    public string Name => Function.GetTypeCaseValue<CompanyTrusteeAddress, string>();

    /// <summary>The company trustee first name</summary>
    /// <remarks>Swissdec XML tag: Firstname</remarks>
    [SwissdecXmlTag("Firstname")]
    public string FirstName => Function.GetTypeCaseValue<CompanyTrusteeAddress, string>();

    /// <summary>The company trustee last name</summary>
    /// <remarks>Swissdec XML tag: Lastname</remarks>
    [SwissdecXmlTag("Lastname")]
    public string LastName => Function.GetTypeCaseValue<CompanyTrusteeAddress, string>();

    /// <summary>The company trustee id</summary>
    /// <remarks>Swissdec XML tag: UID-BFS | Unknown</remarks>
    [SwissdecXmlTag("UID-BFS", "Unknown")]
    public string Id => Function.GetTypeCaseValue<CompanyTrusteeAddress, string>();

    /// <summary>The company trustee country</summary>
    /// <remarks>Swissdec XML tag: Country</remarks>
    [SwissdecXmlTag("Country")]
    public string Country => Function.GetTypeCaseValue<CompanyTrusteeAddress, string>();

    /// <summary>The company trustee complementary line</summary>
    /// <remarks>Swissdec XML tag: ComplementaryLine</remarks>
    [SwissdecXmlTag("ComplementaryLine")]
    public string ComplementaryLine => Function.GetTypeCaseValue<CompanyTrusteeAddress, string>();

    /// <summary>The company trustee street</summary>
    /// <remarks>Swissdec XML tag: Street</remarks>
    [SwissdecXmlTag("Street")]
    public string Street => Function.GetTypeCaseValue<CompanyTrusteeAddress, string>();

    /// <summary>The company trustee postbox</summary>
    /// <remarks>Swissdec XML tag: Postbox</remarks>
    [SwissdecXmlTag("Postbox")]
    public string Postbox => Function.GetTypeCaseValue<CompanyTrusteeAddress, string>();

    /// <summary>The company trustee locality</summary>
    /// <remarks>Swissdec XML tag: Locality</remarks>
    [SwissdecXmlTag("Locality")]
    public string Locality => Function.GetTypeCaseValue<CompanyTrusteeAddress, string>();

    /// <summary>The company trustee zip code</summary>
    /// <remarks>Swissdec XML tag: ZipCode</remarks>
    [SwissdecXmlTag("ZipCode")]
    public string ZipCode => Function.GetTypeCaseValue<CompanyTrusteeAddress, string>();

    /// <summary>The company trustee city</summary>
    /// <remarks>Swissdec XML tag: City</remarks>
    [SwissdecXmlTag("City")]
    public string City => Function.GetTypeCaseValue<CompanyTrusteeAddress, string>();

    /// <summary>The company trustee canton</summary>
    /// <remarks>Swissdec XML tag: Canton</remarks>
    [SwissdecXmlTag("Canton")]
    public string Canton => Function.GetTypeCaseValue<CompanyTrusteeAddress, string>();

    /// <summary>The company trustee community id</summary>
    /// <remarks>Swissdec XML tag: MunicipalityID</remarks>
    [SwissdecXmlTag("MunicipalityID")]
    public int? CommunityId => Function.GetTypeCaseValue<CompanyTrusteeAddress, int?>();
}

/// <summary>Swissdec company FAK</summary>
public class CompanyFak : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public CompanyFak(PayrollFunction function) :
        base(function)
    {
        Cantons = new(Function, CantonSelection);
    }

    /// <summary>The cantons</summary>
    public NamedSlotCollection<Function, Canton, CompanyFakCanton> Cantons { get; }

    /// <summary>The canton selection (CSV)</summary>
    public string CantonSelection => Function.GetTypeCaseValue<CompanyFak, string>();
}

/// <summary>Swissdec company FAK canton</summary>
public class CompanyFakCanton : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public CompanyFakCanton(PayrollFunction function, string caseSlot) :
        base(function)
    {
        CaseSlot = !string.IsNullOrWhiteSpace(caseSlot) ? caseSlot : throw new ArgumentNullException(nameof(function));
    }

    /// <summary>The company FAK slot</summary>
    public string CaseSlot { get; }

    /// <summary>The company FAK canton branch number</summary>
    /// <remarks>Swissdec XML tag: FAK-CAF-BranchNumber</remarks>
    [SwissdecXmlTag("FAK-CAF-BranchNumber")]
    public string BranchNumber => Function.GetTypeCaseSlotValue<CompanyFakCanton, string>(CaseSlot);

    /// <summary>The company FAK canton name</summary>
    public string Name => Function.GetTypeCaseSlotValue<CompanyFakCanton, string>(CaseSlot);

    /// <summary>The company FAK canton id</summary>
    /// <remarks>Swissdec XML tag: CantonID</remarks>
    [SwissdecXmlTag("CantonID")]
    public string Id => Function.GetTypeCaseSlotValue<CompanyFakCanton, string>(CaseSlot);

    /// <summary>The company FAK canton customer number</summary>
    /// <remarks>Swissdec XML tag: FAK-CAF-CustomerNumber</remarks>
    [SwissdecXmlTag("FAK-CAF-CustomerNumber")]
    public string CustomerNumber => Function.GetTypeCaseSlotValue<CompanyFakCanton, string>(CaseSlot);

    /// <summary>The company FAK canton sub number</summary>
    /// <remarks>Swissdec XML tag: FAK-CAF-SubNumber</remarks>
    [SwissdecXmlTag("FAK-CAF-SubNumber")]
    public string SubNumber => Function.GetTypeCaseSlotValue<CompanyFakCanton, string>(CaseSlot);
}

/// <summary>Swissdec company QST</summary>
public class CompanyQst : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public CompanyQst(PayrollFunction function) :
        base(function)
    {
        Cantons = new(function, CantonSelection);
    }

    /// <summary>The cantons</summary>
    public NamedSlotCollection<Function, Canton, CompanyQstCanton> Cantons { get; }

    /// <summary>The canton selection (CSV)</summary>
    public string CantonSelection => Function.GetTypeCaseValue<CompanyQst, string>();
}

/// <summary>Swissdec company QST</summary>
public class CompanyQstCanton : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public CompanyQstCanton(PayrollFunction function, string caseSlot) :
        base(function)
    {
        CaseSlot = !string.IsNullOrWhiteSpace(caseSlot) ? caseSlot : throw new ArgumentNullException(nameof(function));
    }

    /// <summary>The company QST canton case slot</summary>
    public string CaseSlot { get; }

    /// <summary>The company QST canton id</summary>
    /// <remarks>Swissdec XML tag: CantonID</remarks>
    [SwissdecXmlTag("CantonID")]
    public string CompanyQstCantonId => Function.GetTypeCaseSlotValue<CompanyQstCanton, string>(CaseSlot);

    /// <summary>The company QST canton customer identity</summary>
    /// <remarks>Swissdec XML tag: CustomerIdentity</remarks>
    [SwissdecXmlTag("CustomerIdentity")]
    public string CustomerIdentity => Function.GetTypeCaseSlotValue<CompanyQstCanton, string>(CaseSlot);

    /// <summary>The company QST canton payroll unit</summary>
    /// <remarks>Swissdec XML tag: PayrollUnit</remarks>
    [SwissdecXmlTag("PayrollUnit")]
    public string PayrollUnit => Function.GetTypeCaseSlotValue<CompanyQstCanton, string>(CaseSlot);
}

/// <summary>Swissdec contract provider</summary>
public class ContractProvider : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public ContractProvider(PayrollFunction function) :
        base(function)
    {
        Insurances = new(function, InsuranceCount);
    }

    /// <summary>The insurances</summary>
    public SlotCollection<ContractProviderInsurance> Insurances { get; }

    /// <summary>The insurance count</summary>
    public int InsuranceCount => Function.GetTypeCaseValue<ContractProvider, int?>().Safe();

    /// <summary>Get insurance</summary>
    /// <param name="insuranceName">The insurance name</param>
    /// <param name="companyName">The company name</param>
    /// <returns>The insurance</returns>
    public ContractProviderInsurance GetInsurance(string insuranceName, string companyName) =>
        Insurances.Values.FirstOrDefault(
            x => string.Equals(insuranceName, x.Type) && string.Equals(x.CompanyName, companyName));
}

/// <summary>Swissdec contract provider insurance</summary>
public class ContractProviderInsurance : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public ContractProviderInsurance(PayrollFunction function, string caseSlot) :
        base(function)
    {
        CaseSlot = !string.IsNullOrWhiteSpace(caseSlot) ? caseSlot : throw new ArgumentNullException(nameof(function));
    }

    /// <summary>The contract provider insurance case slot</summary>
    public string CaseSlot { get; }

    /// <summary>The contract provider insurance type</summary>
    public string Type => Function.GetTypeCaseSlotValue<ContractProviderInsurance, string>(CaseSlot);

    /// <summary>The contract provider insurance number</summary>
    /// <remarks>Swissdec XML tag: InsuranceID</remarks>
    [SwissdecXmlTag("InsuranceID")]
    public string Number => Function.GetTypeCaseSlotValue<ContractProviderInsurance, string>(CaseSlot);

    /// <summary>The contract provider insurance company name</summary>
    /// <remarks>Swissdec XML tag: InsuranceCompanyName</remarks>
    [SwissdecXmlTag("InsuranceCompanyName")]
    public string CompanyName => Function.GetTypeCaseSlotValue<ContractProviderInsurance, string>(CaseSlot);

    /// <summary>The contract provider insurance id</summary>
    /// <remarks>Swissdec XML tag: UID-BFS</remarks>
    [SwissdecXmlTag("UID-BFS")]
    public string Id => Function.GetTypeCaseSlotValue<ContractProviderInsurance, string>(CaseSlot);

    /// <summary>The contract provider insurance customer identity</summary>
    /// <remarks>Swissdec XML tag: CustomerIdentity</remarks>
    [SwissdecXmlTag("CustomerIdentity")]
    public string CustomerIdentity => Function.GetTypeCaseSlotValue<ContractProviderInsurance, string>(CaseSlot);

    /// <summary>The contract provider insurance contract identity</summary>
    /// <remarks>Swissdec XML tag: ContractIdentity</remarks>
    [SwissdecXmlTag("ContractIdentity")]
    public string ContractIdentity => Function.GetTypeCaseSlotValue<ContractProviderInsurance, string>(CaseSlot);

    /// <summary>The contract provider insurance number</summary>
    /// <remarks>Swissdec XML tag: PayrollUnit</remarks>
    [SwissdecXmlTag("PayrollUnit")]
    public string PayrollUnit => Function.GetTypeCaseSlotValue<ContractProviderInsurance, string>(CaseSlot);

    /// <summary>The contract provider insurance default code</summary>
    public string DefaultCode => Function.GetTypeCaseSlotValue<ContractProviderInsurance, string>(CaseSlot);
}

/// <summary>Swissdec contract provider contribution</summary>
public class ContractProviderContribution : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public ContractProviderContribution(PayrollFunction function) :
        base(function)
    {
        Insurances = new(function, InsuranceCount);
    }

    /// <summary>The insurances</summary>
    public SlotCollection<ContractProviderContributionInsurance> Insurances { get; }

    /// <summary>The insurance count</summary>
    public int InsuranceCount => Function.GetTypeCaseValue<ContractProviderContribution, int?>().Safe();

    /// <summary>Get the insurance contribution</summary>
    /// <param name="insuranceName">The insurance name</param>
    /// <param name="insuranceCode">The insurance code</param>
    /// <returns>The insurance contribution</returns>
    public ContractProviderContributionInsurance GetContribution(string insuranceName, string insuranceCode) =>
        Insurances.Values.FirstOrDefault(
            x => string.Equals(insuranceName, x.Type) && string.Equals(x.GroupCode, insuranceCode));
}

/// <summary>Swissdec contract provider contribution insurance</summary>
public class ContractProviderContributionInsurance : SwissdecBase<PayrollFunction>
{
    /// <summary>Function constructor</summary>
    public ContractProviderContributionInsurance(PayrollFunction function, string caseSlot) :
        base(function)
    {
        CaseSlot = !string.IsNullOrWhiteSpace(caseSlot) ? caseSlot : throw new ArgumentNullException(nameof(function));
    }

    /// <summary>The contract provider contribution insurance case slot</summary>
    public string CaseSlot { get; }

    /// <summary>The contract provider contribution insurance type</summary>
    public string Type => Function.GetTypeCaseSlotValue<ContractProviderContributionInsurance, string>(CaseSlot);

    /// <summary>The contract provider contribution insurance number</summary>
    // [SwissdecXmlTag("InsuranceID")]
    public string Number => Function.GetTypeCaseSlotValue<ContractProviderContributionInsurance, string>(CaseSlot);

    /// <summary>The contract provider contribution insurance group code</summary>
    /// <remarks>Swissdec XML tag: GroupCode</remarks>
    [SwissdecXmlTag("GroupCode")]
    public string GroupCode => Function.GetTypeCaseSlotValue<ContractProviderContributionInsurance, string>(CaseSlot);

    /// <summary>The contract provider contribution insurance minimum salary</summary>
    public decimal? SalaryMin => Function.GetTypeCaseSlotValue<ContractProviderContributionInsurance, decimal?>(CaseSlot);

    /// <summary>The contract provider contribution insurance maximum salary</summary>
    public decimal? SalaryMax => Function.GetTypeCaseSlotValue<ContractProviderContributionInsurance, decimal?>(CaseSlot);

    /// <summary>The contract provider contribution insurance AG male</summary>
    public decimal? AgMalePercent => Function.GetTypeCaseSlotValue<ContractProviderContributionInsurance, decimal?>(CaseSlot);

    /// <summary>The contract provider contribution insurance AG female</summary>
    public decimal? AgFemalePercent => Function.GetTypeCaseSlotValue<ContractProviderContributionInsurance, decimal?>(CaseSlot);

    /// <summary>The contract provider contribution insurance AN male</summary>
    public decimal? AnMalePercent => Function.GetTypeCaseSlotValue<ContractProviderContributionInsurance, decimal?>(CaseSlot);

    /// <summary>The contract provider contribution insurance AN female</summary>
    public decimal? AnFemalePercent => Function.GetTypeCaseSlotValue<ContractProviderContributionInsurance, decimal?>(CaseSlot);

    /// <summary>The contract provider contribution insurance UVG BU</summary>
    public decimal? UvgBuPercent => Function.GetTypeCaseSlotValue<ContractProviderContributionInsurance, decimal?>(CaseSlot);

    /// <summary>The contract provider contribution insurance benefit 1</summary>
    public decimal? Benefit1 => Function.GetTypeCaseSlotValue<ContractProviderContributionInsurance, decimal?>(CaseSlot);

    /// <summary>The contract provider contribution insurance benefit 2</summary>
    public decimal? Benefit2 => Function.GetTypeCaseSlotValue<ContractProviderContributionInsurance, decimal?>(CaseSlot);

    /// <summary>Get AN percent</summary>
    /// <param name="gender"></param>
    /// <returns></returns>
    public decimal GetAnPercent(Gender gender) =>
        gender switch
        {
            Gender.Male => AnMalePercent.Safe(),
            Gender.Female => AnFemalePercent.Safe(),
            _ => 0
        };
}

#endregion
