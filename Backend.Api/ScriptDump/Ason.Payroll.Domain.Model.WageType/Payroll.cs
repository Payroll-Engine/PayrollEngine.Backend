/* Payroll */
using System;
using System.Runtime.CompilerServices;
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec payroll</summary>
public abstract class Payroll<TNational, TCompany, TEmployee, TCollectors, TWageTypes> :
    Swissdec<TNational, TCompany, TEmployee, TCollectors, TWageTypes>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : Employee<TNational, TCompany>
    where TCollectors : Collectors
    where TWageTypes : WageTypes
{
    /// <summary>Function constructor</summary>
    protected Payroll(PayrollFunction function) :
        base(function)
    {
    }

    private PayrollNational national;
    /// <summary>The Swissdec payroll national</summary>
    public override TNational National => (TNational)(national ??= new(Function));

    private PayrollCompany<TNational> company;
    /// <summary>The Swissdec payroll company</summary>
    public override TCompany Company => (TCompany)(company ??= new(Function, National));
}

/// <summary>Swissdec extensions for <see cref="PayrollFunction"/></summary>
public static class PayrollExtensions
{
    /// <summary>Get case value</summary>
    public static T GetCaseValue<T>(this PayrollFunction function, [CallerMemberName] string caseFieldName = "") =>
        function.GetCaseValue<T>(caseFieldName.ToNamespace());

    /// <summary>Get lookup by year</summary>
    public static T GetYearLookup<T>(this PayrollFunction function) =>
        GetLookup<T>(function, function.PeriodEnd.Year.ToString());

    /// <summary>Get shared lookup by year</summary>
    public static T GetLookup<T>(this PayrollFunction function, string lookupKey)
    {
        if (string.IsNullOrWhiteSpace(lookupKey))
        {
            throw new ArgumentException(nameof(lookupKey));
        }

        // assume type name is the lookup name
        var lookupName = typeof(T).Name.ToNamespace();
        // use the period end for the lookup value date
        var value = function.GetLookup<T>(lookupName, lookupKey, function.UserLanguage);
        if (value == null)
        {
            throw new ScriptException($"Missing {lookupName} lookup for period {function.EvaluationPeriod}");
        }
        return value;
    }

    /// <summary>Get lookup value, requires a payroll function</summary>
    /// <param name="function">The function</param>
    /// <param name="lookupName">The lookup name</param>
    /// <param name="lookupKeyValues">The lookup key values</param>
    /// <param name="language">The language</param>
    /// <remarks>this cast breaks the architectural layer access from function to payroll function
    /// benefits: single model for cases and access to the Swissdec XML tags through reporting
    /// </remarks>
    public static T GetLookup<T>(this PayrollFunction function, string lookupName, object[] lookupKeyValues,
        Language? language = null)
    {
        if (string.IsNullOrWhiteSpace(lookupName))
        {
            throw new ArgumentException(nameof(lookupName));
        }
        if (lookupKeyValues == null)
        {
            throw new ArgumentNullException(nameof(lookupKeyValues));
        }

        // use the period end for the lookup value date
        language ??= function.UserLanguage;
        var value = GetLookup<T>(function, lookupName, lookupKeyValues, language);
        return value;
    }

    /// <summary>Get lookup value, requires a payroll function</summary>
    /// <param name="function">The function</param>
    /// <param name="lookupKeyValues">The lookup key values</param>
    /// <param name="language">The language</param>
    public static T GetLookup<T>(this PayrollFunction function, object[] lookupKeyValues, Language? language = null) =>
        GetLookup<T>(function, typeof(T).GetTypeName().ToNamespace(), lookupKeyValues, language);
}
