/* Extensions */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Ason.Payroll.Client.Scripting;

/// <summary><see cref="Type">Type</see> extension methods</summary>
public static class TypeExtensions
{
    /// <summary>
    /// Determines whether the type is numeric.
    /// See https://stackoverflow.com/a/1750024
    /// </summary>
    /// <param name="type">The type</param>
    /// <returns>True for numeric types</returns>
    public static bool IsNumericType(this Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }
}

/// <summary><see cref="Nullable">Type</see> extension methods</summary>
public static class NullableExtensions
{
    /// <summary>Safe nullable boolean cast</summary>
    public static bool Safe(this bool? value, bool defaultValue = default) =>
        value ?? defaultValue;

    /// <summary>Safe nullable int cast</summary>
    public static int Safe(this int? value, int defaultValue = default) =>
        value ?? defaultValue;

    /// <summary>Safe nullable decimal cast</summary>
    public static decimal Safe(this decimal? value, decimal defaultValue = default) =>
        value ?? defaultValue;

    /// <summary>Safe nullable DateTime cast</summary>
    public static DateTime Safe(this DateTime? value, DateTime defaultValue = default) =>
        value ?? defaultValue;
}

/// <summary><see cref="string">String</see> extension methods</summary>
public static class StringExtensions
{

    #region Modification

    /// <summary>Ensures a start prefix</summary>
    /// <param name="source">The source value</param>
    /// <param name="prefix">The prefix to add</param>
    /// <returns>The string with prefix</returns>
    public static string EnsureStart(this string source, string prefix)
    {
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                source = prefix;
            }
            else if (!source.StartsWith(prefix))
            {
                source = $"{prefix}{source}";
            }
        }
        return source;
    }

    /// <summary>Ensures a start prefix</summary>
    /// <param name="source">The source value</param>
    /// <param name="prefix">The prefix to add</param>
    /// <param name="comparison">The comparison culture</param>
    /// <returns>The string with prefix</returns>
    public static string EnsureStart(this string source, string prefix, StringComparison comparison)
    {
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                source = prefix;
            }
            else if (!source.StartsWith(prefix, comparison))
            {
                source = $"{prefix}{source}";
            }
        }
        return source;
    }

    /// <summary>Ensures an ending suffix</summary>
    /// <param name="source">The source value</param>
    /// <param name="suffix">The suffix to add</param>
    /// <returns>The string with suffix</returns>
    public static string EnsureEnd(this string source, string suffix)
    {
        if (!string.IsNullOrWhiteSpace(suffix))
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                source = suffix;
            }
            else if (!source.EndsWith(suffix))
            {
                source = $"{source}{suffix}";
            }
        }
        return source;
    }

    /// <summary>Ensures an ending suffix</summary>
    /// <param name="source">The source value</param>
    /// <param name="suffix">The suffix to add</param>
    /// <param name="comparison">The comparison culture</param>
    /// <returns>The string with suffix</returns>
    public static string EnsureEnd(this string source, string suffix, StringComparison comparison)
    {
        if (!string.IsNullOrWhiteSpace(suffix))
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                source = suffix;
            }
            else if (!source.EndsWith(suffix, comparison))
            {
                source = $"{source}{suffix}";
            }
        }
        return source;
    }

    /// <summary>Remove prefix from string</summary>
    /// <param name="source">The source value</param>
    /// <param name="prefix">The prefix to remove</param>
    /// <returns>The string without suffix</returns>
    public static string RemoveFromStart(this string source, string prefix)
    {
        if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(prefix) &&
            source.StartsWith(prefix))
        {
            source = source.Substring(prefix.Length);
        }
        return source;
    }

    /// <summary>Remove prefix from string</summary>
    /// <param name="source">The source value</param>
    /// <param name="prefix">The prefix to remove</param>
    /// <param name="comparison">The comparison culture</param>
    /// <returns>The string without the starting prefix</returns>
    public static string RemoveFromStart(this string source, string prefix, StringComparison comparison)
    {
        if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(prefix) &&
            source.StartsWith(prefix, comparison))
        {
            source = source.Substring(prefix.Length);
        }
        return source;
    }

    /// <summary>Remove suffix from string</summary>
    /// <param name="source">The source value</param>
    /// <param name="suffix">The suffix to remove</param>
    /// <returns>The string without the ending suffix</returns>
    public static string RemoveFromEnd(this string source, string suffix)
    {
        if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(suffix) &&
            source.EndsWith(suffix))
        {
            source = source.Substring(0, source.Length - suffix.Length);
        }
        return source;
    }

    /// <summary>Remove suffix from string</summary>
    /// <param name="source">The source value</param>
    /// <param name="suffix">The suffix to remove</param>
    /// <param name="comparison">The comparison culture</param>
    /// <returns>The string without the ending suffix</returns>
    public static string RemoveFromEnd(this string source, string suffix, StringComparison comparison)
    {
        if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(suffix) &&
            source.EndsWith(suffix, comparison))
        {
            source = source.Substring(0, source.Length - suffix.Length);
        }
        return source;
    }

    /// <summary>Remove all special characters</summary>
    /// <param name="source">The source value</param>
    /// <returns>The source value without special characters</returns>
    public static string RemoveSpecialCharacters(this string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return source;
        }
        var builder = new StringBuilder();
        foreach (var c in source)
        {
            if (c is >= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z')
            {
                builder.Append(c);
            }
        }
        return builder.ToString();
    }

    #endregion

    #region Csv

    /// <summary>Test for a CSV token</summary>
    /// <param name="source">The source value</param>
    /// <param name="token">The token to search</param>
    /// <param name="separator">The token separator</param>
    /// <returns>True if the token is available</returns>
    public static bool ContainsCsvToken(this string source, string token, char separator = ',')
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(token))
        {
            return false;
        }
        return source.Split(separator, StringSplitOptions.RemoveEmptyEntries).Contains(token);
    }

    #endregion

    #region Attributes

    /// <summary>Prefix for text attribute fields</summary>
    public static readonly string TextAttributePrefix = "TA_";

    /// <summary>Prefix for date attribute fields</summary>
    public static readonly string DateAttributePrefix = "DA_";

    /// <summary>Prefix for numeric attribute fields</summary>
    public static readonly string NumericAttributePrefix = "NA_";

    /// <summary>To text attribute field name</summary>
    /// <param name="attribute">The attribute</param>
    /// <returns>String starting uppercase</returns>
    public static string ToTextAttributeField(this string attribute) =>
        ToAttributeField(attribute, TextAttributePrefix);

    /// <summary>To date attribute field name</summary>
    /// <param name="attribute">The attribute</param>
    /// <returns>String starting uppercase</returns>
    public static string ToDateAttributeField(this string attribute) =>
        ToAttributeField(attribute, DateAttributePrefix);

    /// <summary>To numeric attribute field name</summary>
    /// <param name="attribute">The attribute</param>
    /// <returns>String starting uppercase</returns>
    public static string ToNumericAttributeField(this string attribute) =>
        ToAttributeField(attribute, NumericAttributePrefix);

    private static string ToAttributeField(this string attribute, string prefix)
    {
        if (string.IsNullOrWhiteSpace(attribute))
        {
            throw new ArgumentException(nameof(attribute));
        }
        return attribute.EnsureStart(prefix);
    }

    #endregion

    #region Case Relation

    /// <summary>The related case separator</summary>
    public static readonly char RelatedCaseSeparator = ':';

    /// <summary>The case field slot separator</summary>
    public static readonly char CaseFieldSlotSeparator = ':';

    /// <summary>Extract related cases from a case relation string, format is 'sourceCaseName:targetCaseName'</summary>
    /// <param name="caseRelation">The case relation</param>
    /// <returns>The related cases a tuple: item1=source case, item2=target case</returns>
    public static Tuple<string, string> ToRelatedCaseNames(this string caseRelation)
    {
        if (string.IsNullOrWhiteSpace(caseRelation))
        {
            return default;
        }

        var relatedCases = caseRelation.Split(RelatedCaseSeparator, StringSplitOptions.RemoveEmptyEntries);
        if (relatedCases.Length != 2)
        {
            throw new ArgumentException($"invalid case relation {caseRelation}, please use 'sourceCaseName:targetCaseName')");
        }
        return new(relatedCases[0], relatedCases[1]);
    }

    /// <summary>Extract related cases from a case relation string, format is 'sourceCaseName:targetCaseName'</summary>
    /// <param name="sourceCaseName">The source case name</param>
    /// <param name="targetCaseName">The target case name</param>
    /// <returns>The related cases a tuple: item1=source case, item2=target case</returns>
    public static string ToCaseRelationKey(this string sourceCaseName, string targetCaseName)
    {
        if (string.IsNullOrWhiteSpace(sourceCaseName))
        {
            return default;
        }
        if (string.IsNullOrWhiteSpace(targetCaseName))
        {
            return default;
        }

        return $"{sourceCaseName}{RelatedCaseSeparator}{targetCaseName}";
    }

    #endregion

    #region Json

    /// <summary>Convert string to JSON value</summary>
    /// <param name="json">The JSON string</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The converted value</returns>
    public static T ConvertJson<T>(this string json, T defaultValue = default) =>
        string.IsNullOrWhiteSpace(json) ? defaultValue : JsonSerializer.Deserialize<T>(json);

    /// <summary>Convert JSON value</summary>
    /// <param name="json">The JSON string containing an object</param>
    /// <param name="objectKey">The object key</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The converted value</returns>
    public static T ObjectValueJson<T>(this string json, string objectKey, T defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            throw new ArgumentException(nameof(objectKey));
        }

        var dictionary = ConvertJson<Dictionary<string, object>>(json);
        if (dictionary.ContainsKey(objectKey))
        {
            return defaultValue;
        }
        var value = dictionary[objectKey];
        if (value == null)
        {
            return defaultValue;
        }
        return (T)value;
    }

    #endregion

}

/// <summary><see cref="IEnumerable{T}">IEnumerable</see> extension methods</summary>
public static class EnumerableExtensions
{
    /// <summary>Test if all items are included</summary>
    /// <param name="source">The source collection</param>
    /// <param name="test">The collection with the test items</param>
    /// <returns>True if all items of the test items are available in the source</returns>
    public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> test) =>
        test.All(source.Contains);

    /// <summary>Test if any item is included</summary>
    /// <param name="source">The source collection</param>
    /// <param name="test">The collection with the test items</param>
    /// <returns>True if any item of the test items is available in the source</returns>
    public static bool ContainsAny<T>(this IEnumerable<T> source, IEnumerable<T> test) =>
        test.Any(source.Contains);
}

/// <summary><see cref="Language">Language</see> extension methods</summary>
public static class LanguageExtensions
{
    /// <summary>The ISO 639-1 language code</summary>
    private static readonly string[] LanguageCodes = {
        // ReSharper disable CommentTypo
        "en", // English (Default)

        "af", // Afrikaans
        "ar", // Arabic
        "az", // Azerbaijani
        "be", // Belarusian
        "bg", // Bulgarian
        "bs", // Bosnian
        "cs", // Czech
        "da", // Danish
        "de", // German
        "el", // Greek
        "es", // Spanish
        "et", // Estonian
        "fa", // Persian
        "fi", // Finnish
        "fr", // French
        "ga", // Irish
        "he", // Hebrew
        "hi", // Hindi
        "hr", // Croatian
        "hu", // Hungarian
        "hy", // Armenian
        "is", // Icelandic
        "it", // Italian
        "ja", // Japanese
        "ka", // Georgian
        "ko", // Korean
        "lb", // Luxembourgish
        "lt", // Lithuanian
        "lv", // Latvian
        "mk", // Macedonian
        "nl", // Dutch
        "no", // Norwegian
        "pl", // Polish
        "pt", // Portuguese
        "ro", // Romanian
        "ru", // Russian
        "sk", // Slovak
        "sl", // Slovenian
        "sq", // Albanian
        "sr", // Serbian
        "sw", // Swedish
        "th", // Thai
        "tr", // Turkish
        "uk", // Ukrainian
        "uz", // Uzbek
        "vi", // Vietnamese
        "zh" // Chinese
        // ReSharper restore CommentTypo
    };

    /// <summary>Gets the localized text from a dictionary</summary>
    /// <param name="language">The language</param>
    /// <param name="localizations">The localizations</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The localized text, the default value in case of absent language</returns>
    public static string GetLocalization(this Language language, Dictionary<string, string> localizations, string defaultValue)
    {
        if (localizations != null && localizations.TryGetValue(language.LanguageCode(), out var localization))
        {
            return localization;
        }
        return defaultValue;
    }

    /// <summary>Get the ISO 639-1 language code</summary>
    /// <param name="language">The value type</param>
    /// <returns>The ISO 639-1 language code</returns>
    public static string LanguageCode(this Language language) =>
        LanguageCodes[(int)language];
}

/// <summary><see cref="int">Integer</see> extension methods</summary>
public static class IntExtensions
{
    /// <summary>Determines whether a value is within a range</summary>
    /// <param name="value">The value to test</param>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    /// <returns>True if the specified value is within minimum and maximum</returns>
    public static bool IsWithin(this int value, int min, int max) =>
        value >= min && value <= max;

    /// <summary>Determines whether a value is within a range</summary>
    /// <param name="value">The value to test</param>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    /// <returns>True if the specified value is within minimum and maximum</returns>
    public static bool IsWithin(this int? value, int min, int max) =>
        value.HasValue && IsWithin(value.Value, min, max);
}

/// <summary><see cref="decimal">Decimal</see> extension methods</summary>
public static class DecimalExtensions
{
    /// <summary>Determines whether a value is within a range</summary>
    /// <param name="value">The value to test</param>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    /// <returns>True if the specified value is within minimum and maximum</returns>
    public static bool IsWithin(this decimal value, decimal min, decimal max) =>
        value >= min && value <= max;

    /// <summary>Determines whether a value is within a range</summary>
    /// <param name="value">The value to test</param>
    /// <param name="min">The minimum value</param>
    /// <param name="max">The maximum value</param>
    /// <returns>True if the specified value is within minimum and maximum</returns>
    public static bool IsWithin(this decimal? value, decimal min, decimal max) =>
        value.HasValue && IsWithin(value.Value, min, max);

    /// <summary>Returns the integral digits of the specified decimal, using a step size</summary>
    /// <param name="value">The decimal number to truncate</param>
    /// <param name="stepSize">The step size used to truncate</param>
    /// <returns>The result of d rounded toward zero, to the nearest whole number within the step size</returns>
    public static decimal Truncate(this decimal value, int stepSize) =>
        value == default ? default : value - (value % stepSize);

    /// <summary>Rounds a decimal value up</summary>
    /// <param name="value">The decimal value to round</param>
    /// <param name="stepSize">The round step size</param>
    /// <returns>The up-rounded value</returns>
    public static decimal RoundUp(this decimal value, decimal stepSize) =>
        value == default || stepSize == 0 ? value : Math.Ceiling(value / stepSize) * stepSize;

    /// <summary>Rounds a decimal value down</summary>
    /// <param name="value">The decimal value to round</param>
    /// <param name="stepSize">The round step size</param>
    /// <returns>The rounded value</returns>
    public static decimal RoundDown(this decimal value, decimal stepSize) =>
        value == default || stepSize == 0 ? value : Math.Floor(value / stepSize) * stepSize;

    /// <summary>Rounds a decimal value to a one-tenth</summary>
    /// <param name="value">The decimal value to round</param>
    /// <returns>The down-rounded value to one-tenth</returns>
    public static decimal RoundTenth(this decimal value) =>
        RoundPartOfOne(value, 10);

    /// <summary>Rounds a decimal value to a one-twentieth</summary>
    /// <param name="value">The decimal value to round</param>
    /// <returns>The rounded value to one-twentieth</returns>
    public static decimal RoundTwentieth(this decimal value) =>
        RoundPartOfOne(value, 20);

    /// <summary>Rounds a decimal value to a one-tenth</summary>
    /// <param name="value">The decimal value to round</param>
    /// <param name="divisor">The divisor factor</param>
    /// <returns>The rounded value to one-tenth</returns>
    public static decimal RoundPartOfOne(this decimal value, int divisor) =>
        value == default ? default : Math.Round(value * divisor, MidpointRounding.AwayFromZero) / divisor;
}

/// <summary><see cref="DateTime">DateTime</see> extension methods</summary>
public static class DateTimeExtensions
{
    /// <summary>Returns the DateTime resulting from subtracting
    /// a time span to this DateTime</summary>
    /// <param name="moment">The moment to subtract from</param>
    /// <param name="timeSpan">The time span to subtract</param>
    public static DateTime Subtract(this DateTime moment, TimeSpan timeSpan) =>
        moment.Add(timeSpan.Negate());

    /// <summary>Returns the DateTime resulting from subtracting
    /// a number of ticks to this DateTime</summary>
    /// <param name="moment">The moment to subtract from</param>
    /// <param name="ticks">The ticks to subtract</param>
    public static DateTime SubtractTicks(this DateTime moment, long ticks) =>
        moment.AddTicks(ticks * -1);

    /// <summary>Returns the DateTime resulting from subtracting
    /// a fractional number of seconds to this DateTime</summary>
    /// <param name="moment">The moment to subtract from</param>
    /// <param name="seconds">The seconds to subtract</param>
    public static DateTime SubtractSeconds(this DateTime moment, double seconds) =>
        moment.AddSeconds(seconds * -1);

    /// <summary>Returns the DateTime resulting from subtracting
    /// a fractional number of minutes to this DateTime</summary>
    /// <param name="moment">The moment to subtract from</param>
    /// <param name="minutes">The minutes to subtract</param>
    public static DateTime SubtractMinutes(this DateTime moment, double minutes) =>
        moment.AddMinutes(minutes * -1);

    /// <summary>Returns the DateTime resulting from subtracting
    /// a fractional number of hours to this DateTime</summary>
    /// <param name="moment">The moment to subtract from</param>
    /// <param name="hours">The hours to subtract</param>
    public static DateTime SubtractHours(this DateTime moment, double hours) =>
        moment.AddHours(hours * -1);

    /// <summary>Returns the DateTime resulting from subtracting
    /// a fractional number of days to this DateTime</summary>
    /// <param name="moment">The moment to subtract from</param>
    /// <param name="days">The days to subtract</param>
    public static DateTime SubtractDays(this DateTime moment, double days) =>
        moment.AddDays(days * -1);

    /// <summary>Returns the DateTime resulting from subtracting
    /// a number of months to this DateTime</summary>
    /// <param name="moment">The moment to subtract from</param>
    /// <param name="months">The months to subtract</param>
    public static DateTime SubtractMonths(this DateTime moment, int months) =>
        moment.AddMonths(months * -1);

    /// <summary>Returns the DateTime resulting from subtracting
    /// a number of years to this DateTime</summary>
    /// <param name="moment">The moment to subtract from</param>
    /// <param name="years">The years to subtract</param>
    public static DateTime SubtractYears(this DateTime moment, int years) =>
        moment.AddYears(years * -1);

    /// <summary>Format a period start date (removes empty time parts), using the current culture</summary>
    /// <param name="start">The period start date</param>
    /// <returns>The formatted period start date</returns>
    public static string ToPeriodStartString(this DateTime start) =>
        ToPeriodStartString(start, CultureInfo.CurrentCulture);

    /// <summary>Format a period start date (removes empty time parts)</summary>
    /// <param name="start">The period start date</param>
    /// <param name="provider">The format provider</param>
    /// <returns>The formatted period start date</returns>
    public static string ToPeriodStartString(this DateTime start, IFormatProvider provider) =>
        IsMidnight(start) ? start.ToString("d", provider) : start.ToString("g", provider);

    /// <summary>Format a period end date (removed empty time parts, and round last moment values), using the current culture</summary>
    /// <param name="end">The period end date</param>
    /// <returns>The formatted period end date</returns>
    public static string ToPeriodEndString(this DateTime end) =>
        ToPeriodEndString(end, CultureInfo.CurrentCulture);

    /// <summary>Format a period end date (removed empty time parts, and round last moment values)</summary>
    /// <param name="end">The period end date</param>
    /// <param name="provider">The format provider</param>
    /// <returns>The formatted period end date</returns>
    public static string ToPeriodEndString(this DateTime end, IFormatProvider provider) =>
        IsMidnight(end) || IsLastMomentOfDay(end) ? end.ToString("d", provider) : end.ToString("g", provider);

    /// <summary>Test if the date is in UTC</summary>
    /// <param name="dateTime">The source date time</param>
    /// <returns>True, date time is UTC</returns>
    public static bool IsUtc(this DateTime dateTime) =>
        dateTime.Kind == DateTimeKind.Utc;

    /// <summary>Convert a date into the UTC value. Dates (without time part) are used without time adaption</summary>
    /// <returns>The UTC date time</returns>
    public static DateTime ToUtc(this DateTime moment)
    {
        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch (moment.Kind)
        {
            case DateTimeKind.Utc:
                // already utc
                return moment;
            case DateTimeKind.Unspecified:
                // specify kind
                return DateTime.SpecifyKind(moment, DateTimeKind.Utc);
            case DateTimeKind.Local:
                // convert to utc
                return moment.ToUniversalTime();
            default:
                throw new ArgumentOutOfRangeException($"Unknown date time kind {moment.Kind}");
        }
    }

    /// <summary>Convert a date into the UTC valueDates (without time part) are used without time adaption</summary>
    /// <param name="moment">The source date time</param>
    /// <returns>The UTC date time</returns>
    public static DateTime ToUtcTime(this DateTime moment)
    {
        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch (moment.Kind)
        {
            case DateTimeKind.Utc:
                // already utc
                return moment;
            case DateTimeKind.Unspecified:
                // specify kind
                return DateTime.SpecifyKind(moment, DateTimeKind.Utc);
            case DateTimeKind.Local:
                // convert to utc
                return moment.ToUniversalTime();
            default:
                throw new ArgumentOutOfRangeException($"Unknown date time kind {moment.Kind}");
        }
    }

    /// <summary>Convert a date into the UTC string value, using the current culture</summary>
    /// <param name="dateTime">The date time</param>
    /// <returns>The UTC date time string</returns>
    public static string ToUtcString(this DateTime dateTime) =>
        ToUtcString(dateTime, CultureInfo.CurrentCulture);

    /// <summary>Convert a date into the UTC string value</summary>
    /// <param name="dateTime">The date time</param>
    /// <param name="provider">The format provider</param>
    /// <returns>The UTC date time string</returns>
    public static string ToUtcString(this DateTime dateTime, IFormatProvider provider) =>
        dateTime.ToUtcTime().ToString("o", provider);

    /// <summary>Round to hour, if it is the last tick from a hour</summary>
    /// <param name="dateTime">The source date time</param>
    /// <returns>Date of the next hour if the input is on the last tick on a hour, else the original value</returns>
    public static DateTime RoundTickToHour(this DateTime dateTime)
    {
        if (dateTime >= Date.MaxValue)
        {
            return dateTime;
        }
        var nextTickDate = dateTime.AddTicks(1);
        // check for hour change
        return nextTickDate.Hour == dateTime.Hour ? dateTime : nextTickDate;
    }

    /// <summary>Get the year end date in UTC</summary>
    /// <param name="yearMoment">The year moment</param>
    /// <returns>Last moment of the year</returns>
    public static DateTime YearEnd(this DateTime yearMoment) =>
        Date.YearEnd(yearMoment.Year);

    /// <summary>Get the month end date in UTC</summary>
    /// <param name="monthMoment">The month moment</param>
    /// <returns>Last moment of the month</returns>
    public static DateTime MonthEnd(this DateTime monthMoment) =>
        Date.MonthEnd(monthMoment.Year, monthMoment.Month);

    /// <summary>Get the day end date and time in UTC</summary>
    /// <param name="dayMoment">The day moment</param>
    /// <returns>Last moment of the day</returns>
    public static DateTime DayEnd(this DateTime dayMoment) =>
        Date.DayEnd(dayMoment.Year, dayMoment.Month, dayMoment.Day);

    /// <summary>Test if a specific time moment is within a period</summary>
    /// <param name="test">The moment to test</param>
    /// <param name="start">The period start</param>
    /// <param name="end">The period end</param>
    /// <returns>True if the test is within start and end</returns>
    public static bool IsWithin(this DateTime test, DateTime start, DateTime end) =>
        start < end ?
            test >= start && test <= end :
            test >= end && test <= start;

    /// <summary>Test if a specific time moment is within a period</summary>
    /// <param name="test">The moment to test</param>
    /// <param name="period">The period </param>
    /// <returns>True if the test is within the period</returns>
    public static bool IsWithin(this DateTime test, DatePeriod period) =>
        period.IsWithin(test);

    /// <summary>Test if a specific time moment is before a period</summary>
    /// <param name="test">The moment to test</param>
    /// <param name="period">The period to test</param>
    /// <returns>True, if the moment is before the period</returns>
    public static bool IsBefore(this DateTime test, DatePeriod period) =>
        period.IsBefore(test);

    /// <summary>Test if a specific time moment is after a period</summary>
    /// <param name="test">The moment to test</param>
    /// <param name="period">The period to test</param>
    /// <returns>True, if the moment is after the period</returns>
    public static bool IsAfter(this DateTime test, DatePeriod period) =>
        period.IsAfter(test);

    /// <summary>Test if a specific time moment is the first day of a period</summary>
    /// <param name="test">The moment to test</param>
    /// <param name="period">The period </param>
    /// <returns>True if the test day is the first period day</returns>
    public static bool IsFirstDay(this DateTime test, DatePeriod period) =>
        period.IsFirstDay(test);

    /// <summary>Test if a specific time moment is the last day of a period</summary>
    /// <param name="test">The moment to test</param>
    /// <param name="period">The period </param>
    /// <returns>True if the test day is the last period day</returns>
    public static bool IsLastDay(this DateTime test, DatePeriod period) =>
        period.IsLastDay(test);

    /// <summary>Test if a specific day is the first day of the year</summary>
    /// <param name="test">The moment to test</param>
    /// <returns>True if the test day is the first day in the year</returns>
    public static bool IsFirstDayOfYear(this DateTime test) =>
        IsSameDay(test, new(test.Year, 1, 1));

    /// <summary>Test if a specific time moment is the last day of the year</summary>
    /// <param name="test">The moment to test</param>
    /// <returns>True if the test day is the last day in the year</returns>
    public static bool IsLastDayOfYear(this DateTime test) =>
        IsSameDay(test, new(test.Year, Date.MonthsInYear, 1));

    /// <summary>Returns the number of days in the month of a specific date</summary>
    /// <param name="date">The date</param>
    /// <returns>The number of days in for the specified <paramref name="date" /></returns>
    /// <returns>Last moment of the year</returns>
    public static int DaysInMonth(this DateTime date) =>
        DateTime.DaysInMonth(date.Year, date.Month);

    /// <summary>Returns the date month as enumeration</summary>
    /// <param name="date">The date</param>
    /// <returns>The date month</returns>
    public static Month Month(this DateTime date) =>
        (Month)date.Month;

    /// <summary>Test if a specific day is the first day of the month</summary>
    /// <param name="test">The moment to test</param>
    /// <returns>True if the test day is the first day of the month</returns>
    public static bool IsFirstDayOfMonth(this DateTime test) =>
        IsFirstDayOfMonth(test, (Month)test.Month);

    /// <summary>Test if a specific day is the first day of the month</summary>
    /// <param name="test">The moment to test</param>
    /// <param name="month">The month to test</param>
    /// <returns>True if the test day is the first day of the month</returns>
    public static bool IsFirstDayOfMonth(this DateTime test, Month month) =>
        IsSameDay(test, new(test.Year, (int)month, 1));

    /// <summary>Test if a specific day is the last day of the month</summary>
    /// <param name="test">The moment to test</param>
    /// <param name="ignoreLeapYear">Ignore the leap year day</param>
    /// <returns>True if the test day is the last day of the month</returns>
    public static bool IsLastDayOfMonth(this DateTime test, bool ignoreLeapYear = false) =>
        IsLastDayOfMonth(test, test.Month(), ignoreLeapYear);

    /// <summary>Test if a specific day is the last day of the month</summary>
    /// <param name="test">The moment to test</param>
    /// <param name="month">The month to test</param>
    /// <param name="ignoreLeapYear">Ignore the leap year day</param>
    /// <returns>True if the test day is the last day of the month</returns>
    public static bool IsLastDayOfMonth(this DateTime test, Month month, bool ignoreLeapYear = false)
    {
        var lastMonthDay = new DateTime(test.Year, (int)month, DateTime.DaysInMonth(test.Year, (int)month));
        if (IsSameDay(test, lastMonthDay))
        {
            return true;
        }
        // leap year
        if (ignoreLeapYear && DateTime.IsLeapYear(test.Year))
        {
            // test february 28th
            return IsSameDay(test, lastMonthDay.SubtractDays(1));
        }
        return false;
    }

    /// <summary>Get ISO 8601 week number of the year</summary>
    /// <param name="test">The moment to test</param>
    /// <returns>ISO 8601 week number</returns>
    public static int GetIso8601WeekOfYear(DateTime test) =>
        GetIso8601WeekOfYear(test, CultureInfo.CurrentCulture);

    /// <summary>Get ISO 8601 week number of the year</summary>
    /// <param name="test">The moment to test</param>
    /// <param name="culture">The calendar culture</param>
    /// <returns>ISO 8601 week number</returns>
    public static int GetIso8601WeekOfYear(DateTime test, CultureInfo culture)
    {
        DayOfWeek day = culture.Calendar.GetDayOfWeek(test);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
        {
            test = test.AddDays(3);
        }
        return culture.Calendar.GetWeekOfYear(
            test, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    /// <summary>Get week start date by ISO 8601 week number of the year</summary>
    /// <param name="year">The year</param>
    /// <param name="weekOfYear">The ISO 8601 week number of the year</param>
    /// <returns>First day of the week</returns>
    public static DateTime FirstDayOfWeek(int year, int weekOfYear) =>
        FirstDayOfWeek(year, weekOfYear, CultureInfo.CurrentCulture);

    /// <summary>Get week start date by ISO 8601 week number of the year</summary>
    /// <param name="year">The year</param>
    /// <param name="weekOfYear">The ISO 8601 week number of the year</param>
    /// <param name="culture">The calendar culture</param>
    /// <returns>First day of the week</returns>
    public static DateTime FirstDayOfWeek(int year, int weekOfYear, CultureInfo culture)
    {
        DateTime jan1 = new DateTime(year, 1, 1);
        var daysOffset = (int)culture.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;
        DateTime firstWeekDay = jan1.AddDays(daysOffset);
        var firstWeek = culture.Calendar.GetWeekOfYear(
            jan1, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);
        if (firstWeek is <= 1 or >= 52 && daysOffset >= -3)
        {
            weekOfYear -= 1;
        }
        return firstWeekDay.AddDays(weekOfYear * 7);
    }

    /// <summary>Get week end date by ISO 8601 week number of the year</summary>
    /// <param name="year">The year</param>
    /// <param name="weekOfYear">The ISO 8601 week number of the year</param>
    /// <returns>Last day of the week</returns>
    public static DateTime LastDayOfWeek(int year, int weekOfYear) =>
        LastDayOfWeek(year, weekOfYear, CultureInfo.CurrentCulture);

    /// <summary>Get week end date by ISO 8601 week number of the year</summary>
    /// <param name="year">The year</param>
    /// <param name="weekOfYear">The ISO 8601 week number of the year</param>
    /// <param name="culture">The calendar culture</param>
    /// <returns>Last day of the week</returns>
    public static DateTime LastDayOfWeek(int year, int weekOfYear, CultureInfo culture) =>
        FirstDayOfWeek(year, weekOfYear, culture).AddDays(Date.DaysInWeek - 1);

    /// <summary>Get the previous matching day</summary>
    /// <param name="date">The date to start</param>
    /// <param name="dayOfWeek">Target day of week</param>
    /// <returns>The previous matching day</returns>
    public static DateTime GetPreviousWeekDay(this DateTime date, DayOfWeek dayOfWeek)
    {
        while (date.DayOfWeek != dayOfWeek)
        {
            date = date.AddDays(-1);
        }
        return date;
    }

    /// <summary>Get the next matching day</summary>
    /// <param name="date">The date to start</param>
    /// <param name="dayOfWeek">Target day of week</param>
    /// <returns>The next matching day</returns>
    public static DateTime GetNextWeekDay(this DateTime date, DayOfWeek dayOfWeek)
    {
        while (date.DayOfWeek != dayOfWeek)
        {
            date = date.AddDays(1);
        }
        return date;
    }

    /// <summary>Test for working day</summary>
    /// <param name="date">The date to test</param>
    /// <param name="days">Available days</param>
    /// <returns>True if date is a working day</returns>
    public static bool IsDayOfWeek(this DateTime date, IEnumerable<DayOfWeek> days) =>
        days.Contains(date.DayOfWeek);

    /// <summary>
    /// Test if the date is midnight
    /// </summary>
    /// <param name="moment">The moment to test</param>
    /// <returns>True in case the moment is date</returns>
    public static bool IsMidnight(this DateTime moment) =>
        // https://stackoverflow.com/questions/681435/what-is-the-best-way-to-determine-if-a-system-datetime-is-midnight
        moment.TimeOfDay.Ticks == 0;

    /// <summary>Test if two dates are in the same year</summary>
    /// <param name="date">The first date to test</param>
    /// <param name="compare">The second date to test</param>
    /// <returns>Return true if year and mont of both dates is equal</returns>
    public static bool IsSameYear(this DateTime date, DateTime compare) =>
        date.Year == compare.Year;

    /// <summary>Test if two dates are in the same year and month</summary>
    /// <param name="date">The first date to test</param>
    /// <param name="compare">The second date to test</param>
    /// <returns>Return true if year and mont of both dates is equal</returns>
    public static bool IsSameMonth(this DateTime date, DateTime compare) =>
        date.IsSameYear(compare) && date.Month == compare.Month;

    /// <summary>Test if two dates are in the same day</summary>
    /// <param name="date">The first date to test</param>
    /// <param name="compare">The second date to test</param>
    /// <returns>Return true if both dates are in the same day</returns>
    public static bool IsSameDay(this DateTime date, DateTime compare) =>
        date.IsSameMonth(compare) && date.Day == compare.Day;

    /// <summary>Test if two dates are in the same hour</summary>
    /// <param name="date">The first date to test</param>
    /// <param name="compare">The second date to test</param>
    /// <returns>Return true if both dates are in the same hour</returns>
    public static bool IsSameHour(this DateTime date, DateTime compare) =>
        date.IsSameDay(compare) && date.Hour == compare.Hour;

    /// <summary>Calculates the current age, counting the completed years</summary>
    /// <param name="birthDate">The birth date</param>
    /// <returns>The current age</returns>
    public static int Age(this DateTime birthDate) =>
        Age(birthDate, DateTime.UtcNow);

    /// <summary>Calculates the age at a specific moment, counting the completed years</summary>
    /// <param name="birthDate">The birth date</param>
    /// <param name="testMoment">The test moment</param>
    /// <returns>The age at the test moment</returns>
    public static int Age(this DateTime birthDate, DateTime testMoment)
    {
        if (testMoment <= birthDate)
        {
            throw new ArgumentOutOfRangeException(nameof(testMoment), "calculate age: birth-date must be older than test-date");
        }
        var age = testMoment.Year - birthDate.Year;
        // leap years
        if (birthDate > testMoment.AddYears(-age))
        {
            age--;
        }
        return age;
    }

    /// <summary>Get the previous tick</summary>
    /// <param name="dateTime">The source date time</param>
    /// <returns>The previous tick</returns>
    public static DateTime PreviousTick(this DateTime dateTime) =>
        dateTime.AddTicks(-1);

    /// <summary>Get the next tick</summary>
    /// <param name="dateTime">The source date time</param>
    /// <returns>The next tick</returns>
    public static DateTime NextTick(this DateTime dateTime) =>
        dateTime.AddTicks(1);

    /// <summary>Return the last moment of the day</summary>
    /// <param name="moment">Moment within the day</param>
    /// <returns><seealso cref="DateTime"/> from the latest moment in a day</returns>
    public static DateTime LastMomentOfDay(this DateTime moment) =>
        moment.Date.AddTicks(TimeSpan.TicksPerDay).PreviousTick();

    /// <summary>Test if the date is the last moment of the day</summary>
    /// <param name="moment">Moment to test</param>
    /// <returns>True on the last moment of the day</returns>
    public static bool IsLastMomentOfDay(this DateTime moment) =>
        Equals(LastMomentOfDay(moment), moment);

    /// <summary>Rounds a date time up</summary>
    /// <param name="dateTime">The date time</param>
    /// <param name="stepSize">Size of the rounding step</param>
    /// <returns>The rounded date time</returns>
    public static DateTime RoundUp(this DateTime dateTime, TimeSpan stepSize)
    {
        var modTicks = dateTime.Ticks % stepSize.Ticks;
        var delta = modTicks != default ? stepSize.Ticks - modTicks : 0;
        return delta != 0 ? new(dateTime.Ticks + delta, dateTime.Kind) : dateTime;
    }

    /// <summary>Rounds a date time down</summary>
    /// <param name="dateTime">The date time</param>
    /// <param name="stepSize">Size of the rounding step</param>
    /// <returns>The rounded date time</returns>
    public static DateTime RoundDown(this DateTime dateTime, TimeSpan stepSize)
    {
        var delta = dateTime.Ticks % stepSize.Ticks;
        return delta != default ? new(dateTime.Ticks - delta, dateTime.Kind) : dateTime;
    }

    /// <summary>Rounds a date time to the nearest value</summary>
    /// <param name="dateTime">The date time</param>
    /// <param name="stepSize">Size of the rounding step</param>
    /// <returns>The rounded date time</returns>
    public static DateTime Round(this DateTime dateTime, TimeSpan stepSize)
    {
        var delta = dateTime.Ticks % stepSize.Ticks;
        if (delta == default)
        {
            return dateTime;
        }
        var roundUp = delta > stepSize.Ticks / 2;
        var offset = roundUp ? stepSize.Ticks : 0;
        return new(dateTime.Ticks + offset - delta, dateTime.Kind);
    }
}

/// <summary>Json extensions</summary>
public static class JsonExtensions
{
    /// <summary>Gets the type of the system</summary>
    /// <param name="valueKind">Kind of the value</param>
    /// <returns>The system type</returns>
    public static Type GetSystemType(this JsonValueKind valueKind)
    {
        switch (valueKind)
        {
            case JsonValueKind.Object:
                return typeof(object);
            case JsonValueKind.Array:
                return typeof(Array);
            case JsonValueKind.String:
                return typeof(string);
            case JsonValueKind.Number:
                return typeof(int);
            case JsonValueKind.True:
            case JsonValueKind.False:
                return typeof(bool);
            default:
                return null;
        }
    }

    /// <summary>Gets the type of the system</summary>
    /// <param name="valueKind">Kind of the value</param>
    /// <param name="value">The value, used to determine the numeric type</param>
    /// <returns>The system type</returns>
    public static Type GetSystemType(this JsonValueKind valueKind, object value)
    {
        if (valueKind != JsonValueKind.Number || value == null)
        {
            return GetSystemType(valueKind);
        }

        // integral types
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types
        if (value is sbyte)
        {
            return typeof(sbyte);
        }
        if (value is byte)
        {
            return typeof(byte);
        }
        if (value is short)
        {
            return typeof(short);
        }
        if (value is ushort)
        {
            return typeof(ushort);
        }
        if (value is int)
        {
            return typeof(int);
        }
        if (value is uint)
        {
            return typeof(uint);
        }
        if (value is long)
        {
            return typeof(long);
        }
        if (value is ulong)
        {
            return typeof(ulong);
        }

        // floating point types
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types
        if (value is float)
        {
            return typeof(float);
        }
        if (value is double)
        {
            return typeof(double);
        }
        if (value is decimal)
        {
            return typeof(decimal);
        }

        return typeof(int);
    }

    /// <summary>Get the json element value</summary>
    /// <param name="jsonElement">The json element</param>
    /// <returns>The json element value</returns>
    public static object GetValue(this JsonElement jsonElement)
    {
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.String:
                return jsonElement.GetString();
            case JsonValueKind.Number:
                return jsonElement.GetDecimal();
            case JsonValueKind.True:
            case JsonValueKind.False:
                return jsonElement.GetBoolean();
            case JsonValueKind.Array:
                // recursive values
                return jsonElement.EnumerateArray().
                    Select(GetValue).ToList();
            case JsonValueKind.Object:
                // recursive values
                return jsonElement.EnumerateObject().
                    ToDictionary(item => item.Name, item => GetValue(item.Value));
            case JsonValueKind.Undefined:
            case JsonValueKind.Null:
                return null;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

/// <summary><see cref="Dictionary{TKey,TValue}">Dictionary</see> extension methods</summary>
public static class DictionaryExtensions
{
    /// <summary>Get string/object dictionary value</summary>
    /// <param name="dictionary">The dictionary</param>
    /// <param name="key">The value key</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The dictionary value</returns>
    public static object GetValue(this Dictionary<string, object> dictionary, string key, object defaultValue = default)
    {
        if (!dictionary.ContainsKey(key))
        {
            return defaultValue;
        }
        var value = dictionary[key];
        if (value is JsonElement jsonElement)
        {
            value = jsonElement.GetValue();
        }
        return value ?? defaultValue;
    }

    /// <summary>Get string/T dictionary value</summary>
    /// <param name="dictionary">The dictionary</param>
    /// <param name="key">The value key</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The dictionary value</returns>
    public static T GetValue<T>(this Dictionary<string, object> dictionary, string key, T defaultValue = default) =>
        (T)Convert.ChangeType(GetValue(dictionary, key, (object)defaultValue), typeof(T));

    /// <summary>Get value from a string/JSON-string dictionary</summary>
    /// <param name="dictionary">The dictionary</param>
    /// <param name="dictionaryKey">The dictionary key</param>
    /// <param name="objectKey">The object key</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The dictionary value</returns>
    public static T GetValue<T>(this Dictionary<string, string> dictionary, string dictionaryKey,
        string objectKey, T defaultValue = default) =>
        dictionary.ContainsKey(dictionaryKey) ?
            dictionary[dictionaryKey].ObjectValueJson(objectKey, defaultValue) ?? defaultValue :
            defaultValue;
}

/// <summary><see cref="TimeSpan">TimeSpan</see> extension methods</summary>
public static class TimeSpanExtensions
{
    /// <summary>Rounds a time interval up</summary>
    /// <param name="timeSpan">The time span</param>
    /// <param name="stepSize">Size of the rounding step</param>
    /// <returns>The rounded time interval</returns>
    public static TimeSpan RoundUp(this TimeSpan timeSpan, TimeSpan stepSize)
    {
        var modTicks = timeSpan.Ticks % stepSize.Ticks;
        var delta = modTicks != default ? stepSize.Ticks - modTicks : 0;
        return delta != 0 ? new(timeSpan.Ticks + delta) : timeSpan;
    }

    /// <summary>Rounds a time interval down</summary>
    /// <param name="timeSpan">The time span</param>
    /// <param name="stepSize">Size of the rounding step</param>
    /// <returns>The rounded time interval</returns>
    public static TimeSpan RoundDown(this TimeSpan timeSpan, TimeSpan stepSize)
    {
        var delta = timeSpan.Ticks % stepSize.Ticks;
        return delta != default ? new(timeSpan.Ticks - delta) : timeSpan;
    }

    /// <summary>Rounds a time interval to the nearest value</summary>
    /// <param name="timeSpan">The time span</param>
    /// <param name="stepSize">Size of the rounding step</param>
    /// <returns>The rounded time interval</returns>
    public static TimeSpan Round(this TimeSpan timeSpan, TimeSpan stepSize)
    {
        var delta = timeSpan.Ticks % stepSize.Ticks;
        if (delta == default)
        {
            return timeSpan;
        }
        var roundUp = delta > stepSize.Ticks / 2;
        var offset = roundUp ? stepSize.Ticks : 0;
        return new(timeSpan.Ticks + offset - delta);
    }
}

/// <summary><see cref="DatePeriod">DatePeriod</see> extension methods</summary>
public static class DatePeriodExtensions
{
    /// <summary>Test if a specific time moment is before this period</summary>
    /// <param name="period">The period</param>
    /// <param name="testMoment">The moment to test</param>
    /// <returns>True, if the moment is before this period</returns>
    public static bool IsBefore(this DatePeriod period, DateTime testMoment) =>
        testMoment < period.Start;

    /// <summary>Test if a specific time period is before this period</summary>
    /// <param name="period">The period</param>
    /// <param name="testPeriod">The period to test</param>
    /// <returns>True, if the period is before this period</returns>
    public static bool IsBefore(this DatePeriod period, DatePeriod testPeriod) =>
        testPeriod.End < period.Start;

    /// <summary>Test if a specific time moment is after this period</summary>
    /// <param name="period">The period</param>
    /// <param name="testMoment">The moment to test</param>
    /// <returns>True, if the moment is after this period</returns>
    public static bool IsAfter(this DatePeriod period, DateTime testMoment) =>
        testMoment > period.End;

    /// <summary>Test if a specific time period is after this period</summary>
    /// <param name="period">The period</param>
    /// <param name="testPeriod">The period to test</param>
    /// <returns>True, if the period is after this period</returns>
    public static bool IsAfter(this DatePeriod period, DatePeriod testPeriod) =>
        testPeriod.Start > period.End;

    /// <summary>Test if a specific time moment is within the period, including open periods</summary>
    /// <param name="period">The period</param>
    /// <param name="testMoment">The moment to test</param>
    /// <returns>True, if the moment is within this period</returns>
    public static bool IsWithin(this DatePeriod period, DateTime testMoment) =>
        testMoment.IsWithin(period.Start, period.End);

    /// <summary>Test if a specific time period is within the period, including open periods</summary>
    /// <param name="period">The period</param>
    /// <param name="testPeriod">The period to test</param>
    /// <returns>True, if the test period is within this period</returns>
    public static bool IsWithin(this DatePeriod period, DatePeriod testPeriod) =>
        IsWithin(period, testPeriod.Start) && IsWithin(period, testPeriod.End);

    /// <summary>Test if a specific time moment is within or before the period, including open periods</summary>
    /// <param name="period">The period</param>
    /// <param name="testMoment">The moment to test</param>
    /// <returns>True, if the moment is within or before this period</returns>
    public static bool IsWithinOrBefore(this DatePeriod period, DateTime testMoment) =>
        testMoment <= period.End;

    /// <summary>Test if a specific time moment is within or after the period, including open periods</summary>
    /// <param name="period">The period</param>
    /// <param name="testMoment">The moment to test</param>
    /// <returns>True, if the moment is within or after this period</returns>
    public static bool IsWithinOrAfter(this DatePeriod period, DateTime testMoment) =>
        testMoment >= period.Start;

    /// <summary>Test if period is overlapping this period</summary>
    /// <param name="period">The period</param>
    /// <param name="testPeriod">The period to test</param>
    /// <returns>True, if the period is overlapping this period</returns>
    public static bool IsOverlapping(this DatePeriod period, DatePeriod testPeriod) =>
        testPeriod.Start < period.End && period.Start < testPeriod.End;

    /// <summary>Get the intersection of a date period with this period</summary>
    /// <param name="period">The period</param>
    /// <param name="intersectPeriod">The period to intersect</param>
    /// <returns>The intersecting date period, null if no intersection is present</returns>
    public static DatePeriod Intersect(this DatePeriod period, DatePeriod intersectPeriod)
    {
        if (!IsOverlapping(period, intersectPeriod))
        {
            return null;
        }
        return new(
            new(Math.Max(period.Start.Ticks, intersectPeriod.Start.Ticks)),
            new(Math.Min(period.End.Ticks, intersectPeriod.End.Ticks)));
    }

    /// <summary>Test if a specific time moment is the first day of a period</summary>
    /// <param name="test">The moment to test</param>
    /// <param name="period">The period </param>
    /// <returns>True if the test day is the first period day</returns>
    public static bool IsFirstDay(this DatePeriod period, DateTime test) =>
        test.IsSameDay(period.Start);

    /// <summary>Test if a specific time moment is the last day of a period</summary>
    /// <param name="period">The period </param>
    /// <param name="test">The moment to test</param>
    /// <returns>True if the test day is the last period day</returns>
    public static bool IsLastDay(this DatePeriod period, DateTime test) =>
        test.IsSameDay(period.End);

    /// <summary>Test if a date is the period start day</summary>
    /// <param name="period">The period</param>
    /// <param name="test">The date to test</param>
    /// <returns>True, if the test date is the period start day</returns>
    public static bool IsStartDay(this DatePeriod period, DateTime test) =>
        period.HasStart && period.Start.IsSameDay(test);

    /// <summary>Test if the period start is on a specific weekday</summary>
    /// <param name="period">The period</param>
    /// <param name="testDay">The weekday to test</param>
    /// <returns>True, if the period start day matches the weekday</returns>
    public static bool IsStartDayOfWeek(this DatePeriod period, DayOfWeek testDay) =>
        period.HasStart && period.Start.DayOfWeek == testDay;

    /// <summary>Test if the period start is a monday</summary>
    /// <param name="period">The period</param>
    /// <returns>True, if the period start day is a monday</returns>
    public static bool IsStartMonday(this DatePeriod period) =>
        IsStartDayOfWeek(period, DayOfWeek.Monday);

    /// <summary>Test if the period start is a tuesday</summary>
    /// <param name="period">The period</param>
    /// <returns>True, if the period start day is a tuesday</returns>
    public static bool IsStartTuesday(this DatePeriod period) =>
        IsStartDayOfWeek(period, DayOfWeek.Tuesday);

    /// <summary>Test if the period start is a wednesday</summary>
    /// <param name="period">The period</param>
    /// <returns>True, if the period start day is a Wednesday</returns>
    public static bool IsStartWednesday(this DatePeriod period) =>
        IsStartDayOfWeek(period, DayOfWeek.Wednesday);

    /// <summary>Test if the period start is a thursday</summary>
    /// <param name="period">The period</param>
    /// <returns>True, if the period start day is a thursday</returns>
    public static bool IsStartThursday(this DatePeriod period) =>
        IsStartDayOfWeek(period, DayOfWeek.Thursday);

    /// <summary>Test if the period start is a friday</summary>
    /// <param name="period">The period</param>
    /// <returns>True, if the period start day is a friday</returns>
    public static bool IsStartFriday(this DatePeriod period) =>
        IsStartDayOfWeek(period, DayOfWeek.Friday);

    /// <summary>Test if the period start is a saturday</summary>
    /// <param name="period">The period</param>
    /// <returns>True, if the period start day is a saturday</returns>
    public static bool IsStartSaturday(this DatePeriod period) =>
        IsStartDayOfWeek(period, DayOfWeek.Saturday);

    /// <summary>Test if the period start is a sunday</summary>
    /// <param name="period">The period</param>
    /// <returns>True, if the period start day is a sunday</returns>
    public static bool IsStartSunday(this DatePeriod period) =>
        IsStartDayOfWeek(period, DayOfWeek.Sunday);

    /// <summary>Test if a date is the period end day</summary>
    /// <param name="period">The period</param>
    /// <param name="test">The date to test</param>
    /// <returns>True, if the test date is the period end day</returns>
    public static bool IsEndDay(this DatePeriod period, DateTime test) =>
        period.HasEnd && period.End.IsSameDay(test);

    /// <summary>Test if the period end is on a specific weekday</summary>
    /// <param name="period">The period</param>
    /// <param name="testDay">The weekday to test</param>
    /// <returns>True, if the period end day matches the weekday</returns>
    public static bool IsEndDayOfWeek(this DatePeriod period, DayOfWeek testDay) =>
        period.HasEnd && period.End.DayOfWeek == testDay;

    /// <summary>Test if the period end is a monday</summary>
    /// <param name="period">The period</param>
    /// <returns>True, if the period end day is a monday</returns>
    public static bool IsEndMonday(this DatePeriod period) =>
        IsEndDayOfWeek(period, DayOfWeek.Monday);

    /// <summary>Test if the period end is a tuesday</summary>
    /// <param name="period">The period</param>
    /// <returns>True, if the period end day is a tuesday</returns>
    public static bool IsEndTuesday(this DatePeriod period) =>
        IsEndDayOfWeek(period, DayOfWeek.Tuesday);

    /// <summary>Test if the period end is a wednesday</summary>
    /// <param name="period">The period</param>
    /// <returns>True, if the period end day is a Wednesday</returns>
    public static bool IsEndWednesday(this DatePeriod period) =>
        IsEndDayOfWeek(period, DayOfWeek.Wednesday);

    /// <summary>Test if the period end is a thursday</summary>
    /// <param name="period">The period</param>
    /// <returns>True, if the period end day is a thursday</returns>
    public static bool IsEndThursday(this DatePeriod period) =>
        IsEndDayOfWeek(period, DayOfWeek.Thursday);

    /// <summary>Test if the period end is a friday</summary>
    /// <param name="period">The period</param>
    /// <returns>True, if the period end day is a friday</returns>
    public static bool IsEndFriday(this DatePeriod period) =>
        IsEndDayOfWeek(period, DayOfWeek.Friday);

    /// <summary>Test if the period end is a saturday</summary>
    /// <param name="period">The period</param>
    /// <returns>True, if the period end day is a saturday</returns>
    public static bool IsEndSaturday(this DatePeriod period) =>
        IsEndDayOfWeek(period, DayOfWeek.Saturday);

    /// <summary>Test if the period end is a sunday</summary>
    /// <param name="period">The period</param>
    /// <returns>True, if the period end day is a sunday</returns>
    public static bool IsEndSunday(this DatePeriod period) =>
        IsEndDayOfWeek(period, DayOfWeek.Sunday);

    /// <summary>Calculate the count of working days</summary>
    /// <param name="period">The period</param>
    /// <param name="days">Available days</param>
    /// <returns>The number of working days</returns>
    public static int GetWorkingDaysCount(this DatePeriod period, IEnumerable<DayOfWeek> days)
    {
        var dayCount = 0;
        var date = period.Start.Date;
        var dayOfWeeks = days as DayOfWeek[] ?? days.ToArray();
        while (date < period.End.Date)
        {
            if (date.IsDayOfWeek(dayOfWeeks))
            {
                dayCount++;
            }
            date = date.AddDays(1);
        }
        return dayCount;
    }

    /// <summary>Total duration of all date periods</summary>
    /// <param name="datePeriods">The date periods</param>
    /// <returns>Accumulated total duration</returns>
    public static TimeSpan TotalDuration(this IEnumerable<DatePeriod> datePeriods)
    {
        var duration = TimeSpan.Zero;
        return datePeriods.Aggregate(duration, (current, period) => current.Add(period.Duration));
    }

    /// <summary>Test if any period is overlapping another period</summary>
    /// <param name="periodValues">The time periods to test</param>
    /// <returns>True, if the period is overlapping this period</returns>
    public static bool HasOverlapping(this IEnumerable<DatePeriod> periodValues)
    {
        var periodList = periodValues.ToList();
        for (var current = 1; current < periodList.Count; current++)
        {
            for (var remain = current + 1; remain < periodList.Count; remain++)
            {
                if (periodList[remain].IsOverlapping(periodList[current]))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>Test if a specific time moment is within any date period</summary>
    /// <param name="datePeriods">The time periods to test</param>
    /// <param name="testMoment">The moment to test</param>
    /// <returns>True, if the moment is within this period</returns>
    public static bool IsWithinAny(this IEnumerable<DatePeriod> datePeriods, DateTime testMoment) =>
        datePeriods.Any(periodValue => periodValue.IsWithin(testMoment));

    /// <summary>Test if a specific time period is within any date period</summary>
    /// <param name="datePeriods">The time periods to test</param>
    /// <param name="testPeriod">The period to test</param>
    /// <returns>True, if the test period is within this period</returns>
    public static bool IsWithinAny(this IEnumerable<DatePeriod> datePeriods, DatePeriod testPeriod) =>
        datePeriods.Any(periodValue => periodValue.IsWithin(testPeriod));

    /// <summary>Get limits period, from the earliest start to the latest end</summary>
    /// <param name="datePeriods">The time periods to evaluate</param>
    /// <returns>Date period including all date periods, an anytime period for empty collections</returns>
    public static DatePeriod Limits(this IEnumerable<DatePeriod> datePeriods)
    {
        DateTime? start = null;
        DateTime? end = null;
        foreach (var datePeriod in datePeriods)
        {
            // start
            if (!start.HasValue || datePeriod.Start < start.Value)
            {
                start = datePeriod.Start;
            }
            // end
            if (!end.HasValue || datePeriod.End > end.Value)
            {
                end = datePeriod.End;
            }
        }
        return new(start, end);
    }

    /// <summary>Get all intersections of a date period with any date period</summary>
    /// <param name="datePeriods">The time periods to test</param>
    /// <param name="intersectPeriod">The period to intersect</param>
    /// <returns>List of intersecting date periods</returns>
    public static List<DatePeriod> Intersections(this IEnumerable<DatePeriod> datePeriods, DatePeriod intersectPeriod)
    {
        var intersections = new List<DatePeriod>();
        foreach (var datePeriod in datePeriods)
        {
            var intersection = datePeriod.Intersect(intersectPeriod);
            if (intersection != null)
            {
                intersections.Add(intersection);
            }
        }
        return intersections;
    }
}

/// <summary><see cref="CaseValue">CaseValue</see> extension methods</summary>
public static class CaseValueExtensions
{
    /// <summary>Extract date periods</summary>
    /// <param name="caseValue">The case value</param>
    /// <returns>Accumulated total duration</returns>
    public static DatePeriod Period(this CaseValue caseValue) =>
        new(caseValue.Start, caseValue.End);

    /// <summary>Convert the case value to custom type</summary>
    /// <param name="caseValue">The case value</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>Accumulated total duration</returns>
    public static T ValueAs<T>(this CaseValue caseValue, T defaultValue = default)
    {
        if (caseValue == null || caseValue.Value == null)
        {
            return defaultValue;
        }
        return (T)Convert.ChangeType(caseValue.Value, typeof(T));
    }

    /// <summary>Extract date periods</summary>
    /// <param name="periodValues">The case period values</param>
    /// <returns>Accumulated total duration</returns>
    public static IEnumerable<DatePeriod> Periods(this IEnumerable<CaseValue> periodValues)
    {
        foreach (var periodValue in periodValues)
        {
            yield return periodValue.Period();
        }
    }

    /// <summary>Get case period values grouped by value</summary>
    /// <param name="periodValues">The case period values</param>
    /// <returns>Case period values grouped by value</returns>
    public static IEnumerable<IGrouping<PayrollValue, CaseValue>> GroupByValue(this IEnumerable<CaseValue> periodValues) =>
        periodValues.GroupBy(x => x.Value);

    /// <summary>Total duration of all time periods</summary>
    /// <param name="periodValues">The case period values</param>
    /// <returns>Accumulated total duration</returns>
    public static TimeSpan TotalDuration(this IEnumerable<CaseValue> periodValues) =>
        periodValues.Periods().TotalDuration();

    /// <summary>Get all intersections of a date period with any date period</summary>
    /// <param name="periodValues">The time periods to test</param>
    /// <param name="intersectPeriod">The period to intersect</param>
    /// <returns>List of intersecting date periods</returns>
    public static List<CaseValue> Intersections(this IEnumerable<CaseValue> periodValues, DatePeriod intersectPeriod) =>
        new(periodValues.Where(periodValue => periodValue.Period().IsOverlapping(intersectPeriod)));

    /// <summary>Get case period values matching a period predicate</summary>
    /// <param name="periodValues">The time periods to test</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>List of case period value matching the predicate</returns>
    public static IEnumerable<CaseValue> WherePeriod(this IEnumerable<CaseValue> periodValues,
        Func<DatePeriod, bool> predicate)
    {
        if (periodValues == null)
        {
            throw new ArgumentNullException(nameof(periodValues));
        }

        foreach (var periodValue in periodValues)
        {
            if (predicate(periodValue.Period()))
            {
                yield return periodValue;
            }
        }
    }

    /// <summary>Get date periods with start on specific weekdays</summary>
    /// <param name="periodValues">The case period values</param>
    /// <param name="weekdays">The week days</param>
    /// <returns>List of intersecting date periods</returns>
    public static IEnumerable<CaseValue> WhereStart(this IEnumerable<CaseValue> periodValues,
        params DayOfWeek[] weekdays) =>
        WherePeriod(periodValues, x => weekdays.Contains(x.Start.DayOfWeek));

    /// <summary>Get date periods with start is not on specific weekdays</summary>
    /// <param name="periodValues">The case period values</param>
    /// <param name="weekdays">The week days</param>
    /// <returns>List of intersecting date periods</returns>
    public static IEnumerable<CaseValue> WhereStartNot(this IEnumerable<CaseValue> periodValues,
        params DayOfWeek[] weekdays) =>
        WherePeriod(periodValues, x => !weekdays.Contains(x.Start.DayOfWeek));

    /// <summary>Get date periods with end on specific weekdays</summary>
    /// <param name="periodValues">The case period values</param>
    /// <param name="weekdays">The week days</param>
    /// <returns>List of intersecting date periods</returns>
    public static IEnumerable<CaseValue> WhereEnd(this IEnumerable<CaseValue> periodValues,
        params DayOfWeek[] weekdays) =>
        WherePeriod(periodValues, x => weekdays.Contains(x.End.DayOfWeek));

    /// <summary>Get date periods with end is not on specific weekdays</summary>
    /// <param name="periodValues">The case period values</param>
    /// <param name="weekdays">The week days</param>
    /// <returns>List of intersecting date periods</returns>
    public static IEnumerable<CaseValue> WhereEndNot(this IEnumerable<CaseValue> periodValues,
        params DayOfWeek[] weekdays) =>
        WherePeriod(periodValues, x => !weekdays.Contains(x.End.DayOfWeek));
}

/// <summary><see cref="PayrollValue">PayrollValue</see> extension methods</summary>
public static class PayrollValueExtensions
{

    /// <summary>Convert the case value to custom type</summary>
    /// <param name="payrollValue">The payroll value</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>Accumulated total duration</returns>
    public static T ValueAs<T>(this PayrollValue payrollValue, T defaultValue = default)
    {
        if (payrollValue == null || payrollValue.Value == null)
        {
            return defaultValue;
        }
        return (T)payrollValue.Value;
    }

    /// <summary>Extract all decimal values</summary>
    /// <param name="values">The payroll values</param>
    /// <returns>A list containing all decimal values</returns>
    public static IEnumerable<decimal> GetDecimalValues(this IEnumerable<PayrollValue> values) =>
        values.Where(x => x.Value is decimal).Select(x => (decimal)x.Value);
}

/// <summary><see cref="PeriodValue">PeriodValue</see> extension methods</summary>
public static class PeriodValueExtensions
{
    /// <summary>Get the earliest start date</summary>
    /// <param name="values">The period payroll values</param>
    /// <returns>The earliest start date</returns>
    public static DateTime? GetPeriodStart(this IEnumerable<PeriodValue> values)
    {
        DateTime? start = null;
        foreach (var value in values)
        {
            if (!start.HasValue || value.Start < start)
            {
                start = value.Start;
            }
        }
        return start;
    }

    /// <summary>Get the latest end date</summary>
    /// <param name="values">The period payroll values</param>
    /// <returns>The latest end date</returns>
    public static DateTime? GetPeriodEnd(this IEnumerable<PeriodValue> values)
    {
        DateTime? end = null;
        foreach (var value in values)
        {
            if (!end.HasValue || value.End > end)
            {
                end = value.End;
            }
        }
        return end;
    }

    /// <summary>Get the date period over all values, from the earliest start to the latest end</summary>
    /// <param name="values">The period payroll values</param>
    /// <returns>The overall period, anytime in case of an empty source</returns>
    public static DatePeriod GetPeriod(this IEnumerable<PeriodValue> values)
    {
        var periodValues = values as PeriodValue[] ?? values.ToArray();
        return new(GetPeriodStart(periodValues), GetPeriodEnd(periodValues));
    }

    /// <summary>Extract all date start dates</summary>
    /// <param name="values">The period payroll values</param>
    /// <returns>A date period start dates</returns>
    public static IEnumerable<DateTime> GetPeriodStarts(this IEnumerable<PeriodValue> values) =>
        values?.Where(x => x.Start.HasValue).Select(x => x.Start.Value);

    /// <summary>Extract all date end dates</summary>
    /// <param name="values">The period payroll values</param>
    /// <returns>A date period end dates</returns>
    public static IEnumerable<DateTime> GetPeriodEnds(this IEnumerable<PeriodValue> values) =>
        values?.Where(x => x.End.HasValue).Select(x => x.End.Value);

    /// <summary>Extract all date periods</summary>
    /// <param name="values">The period payroll values</param>
    /// <returns>A list containing all date periods</returns>
    public static IEnumerable<DatePeriod> GetPeriods(this IEnumerable<PeriodValue> values) =>
        values?.Where(x => x.Period != null).Select(x => x.Period);

    /// <summary>Extract all date period durations</summary>
    /// <param name="values">The period payroll values</param>
    /// <returns>A list containing all date period durations</returns>
    public static IEnumerable<TimeSpan> GetDurations(this IEnumerable<PeriodValue> values) =>
        values?.Where(x => x.Period != null).Select(x => x.Period.Duration);

    /// <summary>Summarize the total duration from all date period durations</summary>
    /// <param name="values">The period payroll values</param>
    /// <returns>Total duration from all periods,, an empty time span on empty collection</returns>
    public static TimeSpan TotalDuration(this IEnumerable<PeriodValue> values) =>
        // summarize from all durations the time span ticks
        values != null ? new(GetDurations(values).Sum(ts => ts.Ticks)) : TimeSpan.Zero;

    /// <summary>Total days considering the value as factor</summary>
    /// <param name="values">The period payroll values</param>
    /// <returns>Total days by value as factor</returns>
    public static decimal TotalDaysByValue(this IEnumerable<PeriodValue> values) =>
        TotalDaysByValue(values, true);

    /// <summary>Total days considering the value as factor</summary>
    /// <param name="values">The period payroll values</param>
    /// <param name="includeEndDay">Include the end day</param>
    /// <returns>Total days by value as factor</returns>
    public static decimal TotalDaysByValue(this IEnumerable<PeriodValue> values, bool includeEndDay)
    {
        decimal totalDays = 0;
        if (values != null)
        {
            foreach (var value in values)
            {
                if (value.Value is decimal decimalValue)
                {
                    var days = value.Period.Duration.Days;
                    if (days > 0)
                    {
                        // end day handling
                        if (includeEndDay && value.End.HasValue && value.End.Value.IsMidnight())
                        {
                            days++;
                        }
                        totalDays += days * decimalValue;
                    }
                }
            }
        }
        return totalDays;
    }
}

/// <summary>Payroll results extension methods</summary>
public static class PayrollResultsExtensions
{
    /// <summary>Get case value result values</summary>
    /// <param name="results">The case value results</param>
    /// <returns>Case value result values</returns>
    public static List<PayrollValue> Values(this IEnumerable<CaseValueResult> results) =>
        results.Select(x => x.Value).ToList();

    /// <summary>Get summary of decimal case value results</summary>
    /// <param name="results">The case value results</param>
    /// <returns>Case value decimal result values summary</returns>
    public static decimal Sum(this IEnumerable<CaseValueResult> results) =>
        results.Where(x => x.Value.Value is decimal).Select(x => (decimal)x.Value.Value).Sum();

    /// <summary>Get collector result values</summary>
    /// <param name="results">The collector results</param>
    /// <returns>Collector result values</returns>
    public static List<decimal> Values(this IEnumerable<CollectorResult> results) =>
        results.Select(x => x.Value).ToList();

    /// <summary>Get summary of collector results</summary>
    /// <param name="results">The collector results</param>
    /// <returns>Collector result values summary</returns>
    public static decimal Sum(this IEnumerable<CollectorResult> results) =>
        Values(results).Sum();

    /// <summary>Get collector custom result values</summary>
    /// <param name="results">The collector custom results</param>
    /// <returns>Collector custom result values</returns>
    public static List<decimal> Values(this IEnumerable<CollectorCustomResult> results) =>
        results.Select(x => x.Value).ToList();

    /// <summary>Get summary of collector custom results</summary>
    /// <param name="results">The collector custom results</param>
    /// <returns>Collector custom results result values summary</returns>
    public static decimal Sum(this IEnumerable<CollectorCustomResult> results) =>
        Values(results).Sum();

    /// <summary>Get wage type result values</summary>
    /// <param name="results">The wage type results</param>
    /// <returns>Wage type result values</returns>
    public static List<decimal> Values(this IEnumerable<WageTypeResult> results) =>
        results.Select(x => x.Value).ToList();

    /// <summary>Get summary of wage type results</summary>
    /// <param name="results">The wage type results</param>
    /// <returns>Wage type result values summary</returns>
    public static decimal Sum(this IEnumerable<WageTypeResult> results) =>
        Values(results).Sum();

    /// <summary>Get wage type custom results values</summary>
    /// <param name="results">The wage type custom results</param>
    /// <returns>Wage type custom result values</returns>
    public static List<decimal> Values(this IEnumerable<WageTypeCustomResult> results) =>
        results.Select(x => x.Value).ToList();

    /// <summary>Get summary of wage type custom results</summary>
    /// <param name="results">The wage type custom results</param>
    /// <returns>Wage type results result values summary</returns>
    public static decimal Sum(this IEnumerable<WageTypeCustomResult> results) =>
        Values(results).Sum();
}

/// <summary>Value type extension methods</summary>
public static class ValueTypeExtensions
{
    /// <summary>Test if value type is a number</summary>
    /// <param name="valueType">The value type</param>
    /// <returns>True for number value types</returns>
    public static bool IsNumber(this ValueType valueType) =>
        IsDecimal(valueType) || valueType == ValueType.Integer;

    /// <summary>Test if value type is a decimal number</summary>
    /// <param name="valueType">The value type</param>
    /// <returns>True for decimal number value types</returns>
    public static bool IsDecimal(this ValueType valueType) =>
        valueType == ValueType.NumericBoolean ||
        valueType == ValueType.Decimal ||
        valueType == ValueType.Money ||
        valueType == ValueType.Percent ||
        valueType == ValueType.Hour ||
        valueType == ValueType.Day ||
        valueType == ValueType.Distance;

    /// <summary>Get the data type</summary>
    /// <param name="valueType">The value type</param>
    /// <returns>The data type</returns>
    public static Type GetDataType(this ValueType valueType)
    {
        switch (valueType)
        {
            case ValueType.String:
            case ValueType.WebResource:
            case ValueType.Date:
            case ValueType.DateTime:
                return typeof(string);
            case ValueType.Integer:
                return typeof(int);
            case ValueType.NumericBoolean:
            case ValueType.Decimal:
            case ValueType.Money:
            case ValueType.Percent:
            case ValueType.Hour:
            case ValueType.Day:
            case ValueType.Distance:
                return typeof(decimal);
            case ValueType.Boolean:
                return typeof(bool);
            case ValueType.None:
                return typeof(DBNull);
        }
        throw new ScriptException($"Unknown value type {valueType}");
    }

    /// <summary>Get the value type</summary>
    /// <param name="value">The value</param>
    /// <returns>The value type</returns>
    public static ValueType GetValueType(this object value)
    {
        if (value is string)
        {
            return ValueType.String;
        }
        if (value is int)
        {
            return ValueType.Integer;
        }
        if (value is decimal or float)
        {
            return ValueType.Decimal;
        }
        if (value is bool)
        {
            return ValueType.Boolean;
        }
        return ValueType.None;
    }

    /// <summary>Convert json by value type</summary>
    /// <param name="valueType">The value type</param>
    /// <param name="json">The json to convert</param>
    /// <returns>Object value</returns>
    public static object JsonToValue(this ValueType valueType, string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException(nameof(json));
        }
        switch (valueType)
        {
            case ValueType.Integer:
                return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<int>(json);

            case ValueType.NumericBoolean:
            case ValueType.Decimal:
            case ValueType.Money:
            case ValueType.Percent:
            case ValueType.Hour:
            case ValueType.Day:
            case ValueType.Distance:
                return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<decimal>(json);

            case ValueType.String:
            case ValueType.WebResource:
                return string.IsNullOrWhiteSpace(json) ? default :
                    json.StartsWith('"') ? JsonSerializer.Deserialize<string>(json) : json;

            case ValueType.Date:
            case ValueType.DateTime:
                return string.IsNullOrWhiteSpace(json) ? default :
                    json.StartsWith('"') ? JsonSerializer.Deserialize<DateTime>(json) : DateTime.Parse(json, null, DateTimeStyles.AdjustToUniversal);

            case ValueType.Boolean:
                return !string.IsNullOrWhiteSpace(json) && JsonSerializer.Deserialize<bool>(json);
            case ValueType.None:
                return null;
            default:
                throw new ScriptException($"unknown value type {valueType}");
        }
    }
}

/// <exclude />
/// <summary><see cref="Tuple">Tuple</see> extension methods (internal usage)</summary>
public static class TupleExtensions
{
    /// <exclude />
    /// <summary>Convert tuple values to case period values</summary>
    /// <param name="values">The tuple values</param>
    /// <returns>The period values</returns>
    public static List<PeriodValue> TupleToPeriodValues(this List<Tuple<DateTime, DateTime?, object>> values)
    {
        var periodValues = new List<PeriodValue>();
        if (values != null)
        {
            foreach (var value in values)
            {
                if (value != null)
                {
                    periodValues.Add(new(value.Item1, value.Item2, value.Item3));
                }
            }
        }
        return periodValues;
    }

    /// <exclude />
    /// <summary>Convert tuple values to a case value</summary>
    /// <param name="values">The tuple values</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case values</returns>
    public static CasePayrollValue TupleToCasePeriodValue(this List<Tuple<DateTime, DateTime?, object>> values, string caseFieldName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        return new(caseFieldName, values.Select(x => new PeriodValue(x.Item1, x.Item2, x.Item3)));
    }

    /// <exclude />
    /// <summary>Convert tuple values to case value</summary>
    /// <param name="value">The tuple value</param>
    /// <returns>The case period values</returns>
    public static CaseValue TupleToCaseValue(this Tuple<string, DateTime, Tuple<DateTime?, DateTime?>, object, DateTime?, List<string>, Dictionary<string, object>> value)
    {
        if (value == null)
        {
            return null;
        }
        return new(value.Item1, value.Item2, value.Item3.Item1, value.Item3.Item2, new(value.Item4), value.Item5, value.Item6, value.Item7);
    }

    /// <exclude />
    /// <summary>Convert tuple values to case values</summary>
    /// <param name="values">The tuple values</param>
    /// <returns>The case period values</returns>
    public static List<CaseValue> TupleToCaseValues(this List<Tuple<string, DateTime, Tuple<DateTime?, DateTime?>, object, DateTime?, List<string>, Dictionary<string, object>>> values)
    {
        var caseValues = new List<CaseValue>();
        if (values != null)
        {
            foreach (var value in values)
            {
                if (value != null)
                {
                    caseValues.Add(new(value.Item1, value.Item2, value.Item3.Item1, value.Item3.Item2, new(value.Item4), value.Item5, value.Item6, value.Item7));
                }
            }
        }
        return caseValues;
    }

    /// <exclude />
    /// <summary>Convert tuple values to a case value dictionary</summary>
    /// <param name="values">The values</param>
    /// <returns>The case values grouped by case field name</returns>
    public static CasePayrollValueDictionary TupleToCaseValuesDictionary(this Dictionary<string, List<Tuple<DateTime, DateTime?, DateTime?, object>>> values)
    {
        var caseValues = new Dictionary<string, CasePayrollValue>();
        foreach (var value in values)
        {
            var periodValues = value.Value.Select(x => new PeriodValue(x.Item2, x.Item3, x.Item4));
            caseValues.Add(value.Key, new(value.Key, periodValues));
        }
        return new(caseValues);
    }

    /// <exclude />
    /// <summary>Convert tuple values to a collector result</summary>
    /// <param name="values">The tuple values</param>
    /// <returns>The collector results</returns>
    public static List<CollectorResult> TupleToCollectorResults(this List<Tuple<string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>> values) =>
        new(values.Select(x => new CollectorResult
        {
            CollectorName = x.Item1,
            Start = x.Item2.Item1,
            End = x.Item2.Item2,
            Value = x.Item3,
            Tags = x.Item4,
            Attributes = x.Item5
        }));

    /// <exclude />
    /// <summary>Convert tuple values to a collector custom result</summary>
    /// <param name="values">The tuple values</param>
    /// <returns>The collector custom results</returns>
    public static List<CollectorCustomResult> TupleToCollectorCustomResults(this List<Tuple<string, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>> values) =>
        new(values.Select(x => new CollectorCustomResult
        {
            CollectorName = x.Item1,
            Source = x.Item2,
            Start = x.Item3.Item1,
            End = x.Item3.Item2,
            Value = x.Item4,
            Tags = x.Item5,
            Attributes = x.Item6
        }));

    /// <exclude />
    /// <summary>Convert tuple values to a wage type result</summary>
    /// <param name="values">The tuple values</param>
    /// <returns>The wage type results</returns>
    public static List<WageTypeResult> TupleToWageTypeResults(this List<Tuple<decimal, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>> values) =>
        new(values.Select(x => new WageTypeResult
        {
            WageTypeNumber = x.Item1,
            WageTypeName = x.Item2,
            Start = x.Item3.Item1,
            End = x.Item3.Item2,
            Value = x.Item4,
            Tags = x.Item5,
            Attributes = x.Item6
        }));

    /// <exclude />
    /// <summary>Convert tuple values to a wage type custom result</summary>
    /// <param name="values">The tuple values</param>
    /// <returns>The wage type custom results</returns>
    public static List<WageTypeCustomResult> TupleToWageTypeCustomResults(this List<Tuple<decimal, string, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>> values) =>
        new(values.Select(x => new WageTypeCustomResult
        {
            WageTypeNumber = x.Item1,
            Name = x.Item2,
            Source = x.Item3,
            Start = x.Item4.Item1,
            End = x.Item4.Item2,
            Value = x.Item5,
            Tags = x.Item6,
            Attributes = x.Item7
        }));
}