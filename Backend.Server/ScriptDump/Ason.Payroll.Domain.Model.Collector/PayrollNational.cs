/* PayrollNational */
using System;
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec Payroll National</summary>
public class PayrollNational : National
{
    /// <summary>Function constructor</summary>
    public PayrollNational(PayrollFunction function) :
        base(function)
    {
    }

    #region National Time Shared Lookups

    private AhvData ahv;
    /// <summary>The Swissdec payroll AHV</summary>
    public AhvData Ahv => ahv ??= GetYearSharedLookup<AhvData>(Function);

    private AlvData alv;
    /// <summary>The Swissdec payroll ALV</summary>
    public AlvData Alv => alv ??= GetYearSharedLookup<AlvData>(Function);

    private AlvzData alvz;
    /// <summary>The Swissdec payroll ALVZ</summary>
    public AlvzData Alvz => alvz ??= GetYearSharedLookup<AlvzData>(Function);

    private UvgData uvg;
    /// <summary>The Swissdec payroll UVG</summary>
    public UvgData Uvg => uvg ??= GetYearSharedLookup<UvgData>(Function);

    private BvgData bvg;
    /// <summary>The Swissdec payroll BVG</summary>
    public BvgData Bvg => bvg ??= GetYearSharedLookup<BvgData>(Function);

    /// <summary>Get FAK canton</summary>
    public FakCantonData GetFakCanton(Canton canton) =>
        Function.GetLookup<FakCantonData>(Enum.GetName(typeof(Canton), canton));

    /// <summary>Get QST</summary>
    public QstData GetQst(Canton canton, string code) =>
        Function.GetLookup<QstData>(
            lookupName: canton.QstLookupName(),
            lookupKeyValues: new object[] { Enum.GetName(typeof(Canton), canton), code });

    #endregion

    /// <summary>Get shared lookup by year</summary>
    private static T GetYearSharedLookup<T>(PayrollFunction function)
    {
        var value = function.GetYearLookup<T>();
        if (value == null)
        {
            throw new ScriptException($"Missing {typeof(T).Name.ToNamespace()} lookup for period {function.EvaluationPeriod}");
        }
        return value;
    }

}
