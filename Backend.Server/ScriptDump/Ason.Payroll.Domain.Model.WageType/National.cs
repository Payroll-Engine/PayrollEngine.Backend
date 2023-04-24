/* National */
using System;
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec National</summary>
public class National : SwissdecBase<PayrollFunction>
{
    /// <summary>The national shared lookups</summary>
    private enum NationalSharedLookups
    {
        /// <summary>QST canton</summary>
        QstCanton,
        /// <summary>Receiver</summary>
        Receiver
    }

    /// <summary>Function constructor</summary>
    public National(PayrollFunction function) :
        base(function)
    {
    }

    #region National Timeless Shared Lookups

    /// <summary>Get county by name</summary>
    public CountryData GetCountry(string name) =>
        Function.GetLookup<CountryData>(name);

    /// <summary>Get community</summary>
    public CommunityData GetCommunity(int zipCode, int localZipCode, int communityId, Language? language = null) =>
        Function.GetLookup<CommunityData>(
            lookupKeyValues: new object[] { zipCode, localZipCode, communityId },
            language: language);

    /// <summary>Get QST canton</summary>
    public QstCantonData GetQstCalculationCycle(Canton canton) =>
        Function.GetLookup<QstCantonData>(
            lookupName: Enum.GetName(NationalSharedLookups.QstCanton).ToNamespace(),
            lookupKey: Enum.GetName(typeof(Canton), canton));

    /// <summary>Get Receiver by number</summary>
    public ReceiverData GetReceiver(string number) =>
        Function.GetLookup<ReceiverData>(
            lookupName: Enum.GetName(NationalSharedLookups.Receiver).ToNamespace(),
            lookupKey: number);

    #endregion
}

#region National Data Objects

/// <summary>Swissdec AHV data</summary>
public class AhvData
{
    /// <summary>The pension contribution obligation age value</summary>
    public int ContributionObligationAge { get; set; }
    /// <summary>The retirement age male value</summary>
    public int RetirementAgeMale { get; set; }
    /// <summary>The retirement age female value</summary>
    public int RetirementAgeFemale { get; set; }
    /// <summary>The retirement exemption value</summary>
    public decimal RetirementExemption { get; set; }
    /// <summary>The contribution of employee</summary>
    public decimal ContributionEmployee { get; set; }
    /// <summary>The contribution of employer</summary>
    public decimal ContributionEmployer { get; set; }
}

/// <summary>Swissdec ALV data</summary>
public class AlvData
{
    /// <summary>ALV upper limit</summary>
    public decimal UpperLimit { get; set; }
    /// <summary>The contribution of employee</summary>
    public decimal ContributionEmployee { get; set; }
    /// <summary>The contribution of employer</summary>
    public decimal ContributionEmployer { get; set; }
}

/// <summary>Swissdec ALVZ data</summary>
public class AlvzData
{
    /// <summary>ALVZ upper limit</summary>
    public decimal UpperLimit { get; set; }
    /// <summary>The contribution of employee</summary>
    public decimal ContributionEmployee { get; set; }
    /// <summary>The contribution of employer</summary>
    public decimal ContributionEmployer { get; set; }
}

/// <summary>Swissdec UVG data</summary>
public class UvgData
{
    /// <summary>UVG upper limit</summary>
    public decimal UpperLimit { get; set; }
}

/// <summary>Swissdec BVG data</summary>
public class BvgData
{
    /// <summary>BVG minimum yearly wage</summary>
    public decimal MinimumYearlyWage { get; set; }
    /// <summary>BVG coordinated deduction</summary>
    public decimal CoordinatedDeduction { get; set; }
    /// <summary>BVG upper limit yearly wage</summary>
    public decimal UpperLimitYearlyWage { get; set; }
    /// <summary>BVG minimum coordinated wage</summary>
    public decimal MinimumCoordinatedWage { get; set; }
    /// <summary>BVG maximum coordinated wage</summary>
    public decimal MaximumCoordinatedWage { get; set; }
}

/// <summary>Swissdec QST canton data</summary>
public class QstCantonData
{
    /// <summary>QST canton name</summary>
    public string Name { get; set; }
    /// <summary>QST canton median</summary>
    public decimal? Median { get; set; }
    /// <summary>QST canton calculation cycle</summary>
    public QstCalculationCycle CalculationCycle { get; set; }
}

/// <summary>Swissdec QST data</summary>
public class QstData
{
    /// <summary>QST code</summary>
    public string Code { get; set; }
    /// <summary>QST minimum tax</summary>
    public decimal MinimumTax { get; set; }
    /// <summary>QST tax rate</summary>
    public decimal TaxRate { get; set; }
}

/// <summary>Swissdec receiver data</summary>
public class ReceiverData
{
    /// <summary>Receiver name</summary>
    public string Name { get; set; }
    /// <summary>Receiver id</summary>
    public string Id { get; set; }
}

/// <summary>Swissdec country data</summary>
public class CountryData
{
    /// <summary>Country name</summary>
    public string Name { get; set; }
    /// <summary>Alpha-2 Code (ISO 3166)</summary>
    public string IsoCode2 { get; set; }
    /// <summary>Alpha-3 Code (ISO 3166)</summary>
    public string IsoCode3 { get; set; }
}

/// <summary>Swissdec communities data</summary>
public class CommunityData
{
    /// <summary>Community name</summary>
    public string Name { get; set; }
    /// <summary>Community zip code</summary>
    public int ZipCode { get; set; }
    /// <summary>Community zip name</summary>
    public string ZipName { get; set; }
    /// <summary>Community local zip code</summary>
    public int LocalZipCode { get; set; }
    /// <summary>Community id</summary>
    public int CommunityId { get; set; }
    /// <summary>Canton name</summary>
    public string Canton { get; set; }
}

/// <summary>Swissdec FAK canton data</summary>
public class FakCantonData
{
    /// <summary>FAK canton name</summary>
    public string Name { get; set; }
    /// <summary>FAK child allowance step 1</summary>
    public decimal ChildAllowanceStep1 { get; set; }
    /// <summary>FAK child allowance step 2</summary>
    public decimal ChildAllowanceStep2 { get; set; }
    /// <summary>FAK child allowance step 2 reason</summary>
    public string ChildAllowanceStep2Reason { get; set; }
    /// <summary>FAK education allowance step 1</summary>
    public decimal EducationAllowanceStep1 { get; set; }
    /// <summary>FAK education allowance step 2</summary>
    public decimal EducationAllowanceStep2 { get; set; }
    /// <summary>FAK birth allowance step 1</summary>
    public decimal BirthAllowanceStep1 { get; set; }
    /// <summary>FAK birth allowance step 2</summary>
    public decimal BirthAllowanceStep2 { get; set; }
    /// <summary>FAK adoption allowance step 1</summary>
    public decimal AdoptionAllowanceStep1 { get; set; }
    /// <summary>FAK adoption allowance step 2</summary>
    public decimal AdoptionAllowanceStep2 { get; set; }
    /// <summary>FAK percent contribution</summary>
    public decimal FakContribution { get; set; }
}

/// <summary>Swissdec workplace data</summary>
public class WorkplaceData
{
    /// <summary>The workplace id</summary>
    public string WorkplaceId { get; set; }
    /// <summary>BUR-REE-Number Mask, PKz</summary>
    public string BurReeNumber { get; set; }
    /// <summary>In house id</summary>
    public string InHouseId { get; set; }
    /// <summary>Working time model 1</summary>
    public string WorkingTimeModel1 { get; set; }
    /// <summary>Weekly hours 1</summary>
    public decimal WeeklyHours1 { get; set; }
    /// <summary>Weekly lessons 1</summary>
    public decimal WeeklyLessons1 { get; set; }
    /// <summary>Working time model 2</summary>
    public string WorkingTimeModel2 { get; set; }
    /// <summary>Weekly hours 2</summary>
    public decimal WeeklyHours2 { get; set; }
    /// <summary>Weekly lessons 2</summary>
    public decimal WeeklyLessons2 { get; set; }
    /// <summary>The country name</summary>
    public string Country { get; set; }
    /// <summary>Complementary line</summary>
    public string ComplementaryLine { get; set; }
    /// <summary>Street</summary>
    public string Street { get; set; }
    /// <summary>Locality</summary>
    public string Locality { get; set; }
    /// <summary>Zip code</summary>
    public string ZipCode { get; set; }
    /// <summary>The city name</summary>
    public string City { get; set; }
    /// <summary>The canton name</summary>
    public string Canton { get; set; }
    /// <summary>The municipality id</summary>
    public int MunicipalityId { get; set; }
}

#endregion
