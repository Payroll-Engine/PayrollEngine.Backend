/* ClientScript */
using System;
using System.Data;

namespace Ason.Payroll.Client.Scripting;

#region Enums

/// <summary>The supported languages</summary>
public enum Language
{
    // ReSharper disable CommentTypo
    // ReSharper disable IdentifierTypo

    /// <summary>English (Default)</summary>
    English,

    /// <summary>Afrikaans</summary>
    Afrikaans,
    /// <summary>Arabic</summary>
    Arabic,
    /// <summary>Azerbaijani</summary>
    Azerbaijani,
    /// <summary>Belarusian</summary>
    Belarusian,
    /// <summary>Bulgarian</summary>
    Bulgarian,
    /// <summary>Bosnian</summary>
    Bosnian,
    /// <summary>Czech</summary>
    Czech,
    /// <summary>Danish</summary>
    Danish,
    /// <summary>German</summary>
    German,
    /// <summary>Greek</summary>
    Greek,
    /// <summary>Spanish</summary>
    Spanish,
    /// <summary>Estonian</summary>
    Estonian,
    /// <summary>Persian</summary>
    Persian,
    /// <summary>Finnish</summary>
    Finnish,
    /// <summary>French</summary>
    French,
    /// <summary>Irish</summary>
    Irish,
    /// <summary>Hebrew</summary>
    Hebrew,
    /// <summary>Hindi</summary>
    Hindi,
    /// <summary>Croatian</summary>
    Croatian,
    /// <summary>Hungarian</summary>
    Hungarian,
    /// <summary>Armenian</summary>
    Armenian,
    /// <summary>Icelandic</summary>
    Icelandic,
    /// <summary>Italian</summary>
    Italian,
    /// <summary>Japanese</summary>
    Japanese,
    /// <summary>Georgian</summary>
    Georgian,
    /// <summary>Korean</summary>
    Korean,
    /// <summary>Luxembourgish</summary>
    Luxembourgish,
    /// <summary>Lithuanian</summary>
    Lithuanian,
    /// <summary>Latvian</summary>
    Latvian,
    /// <summary>Macedonian</summary>
    Macedonian,
    /// <summary>Dutch</summary>
    Dutch,
    /// <summary>Norwegian</summary>
    Norwegian,
    /// <summary>Polish</summary>
    Polish,
    /// <summary>Portuguese</summary>
    Portuguese,
    /// <summary>Romanian</summary>
    Romanian,
    /// <summary>Russian</summary>
    Russian,
    /// <summary>Slovak</summary>
    Slovak,
    /// <summary>Slovenian</summary>
    Slovenian,
    /// <summary>Albanian</summary>
    Albanian,
    /// <summary>Serbian</summary>
    Serbian,
    /// <summary>Swedish</summary>
    Swedish,
    /// <summary>Thai</summary>
    Thai,
    /// <summary>Turkish</summary>
    Turkish,
    /// <summary>Ukrainian</summary>
    Ukrainian,
    /// <summary>Uzbek</summary>
    Uzbek,
    /// <summary>Vietnamese</summary>
    Vietnamese,
    /// <summary>Chinese</summary>
    Chinese
    // ReSharper restore IdentifierTypo
    // ReSharper restore CommentTypo
}

/// <summary>The year months</summary>
public enum Month
{
    /// <summary>January</summary>
    January = 1,
    /// <summary>February</summary>
    February = 2,
    /// <summary>March</summary>
    March = 3,
    /// <summary>April</summary>
    April = 4,
    /// <summary>May</summary>
    May = 5,
    /// <summary>June</summary>
    June = 6,
    /// <summary>July</summary>
    July = 7,
    /// <summary>August</summary>
    August = 8,
    /// <summary>September</summary>
    September = 9,
    /// <summary>October</summary>
    October = 10,
    /// <summary>November</summary>
    November = 11,
    /// <summary>December</summary>
    December = 12
}

/// <summary>Specifies the meaning and relative importance of a log event</summary>
public enum LogLevel
{
    /// <summary>Anything and everything you might want to know about a running block of code</summary>
    Verbose,
    /// <summary>Internal system events that aren't necessarily observable from the outside</summary>
    Debug,
    /// <summary>The lifeblood of operational intelligence - things happen</summary>
    Information,
    /// <summary>Service is degraded or endangered</summary>
    Warning,
    /// <summary>Functionality is unavailable, invariants are broken or data is lost</summary>
    Error,
    /// <summary>If you have a pager, it goes off when one of these occurs/// </summary>
    Fatal
}

/// <summary>The type of a case</summary>
public enum CaseType
{
    /// <summary>Global case</summary>
    Global = 0,
    /// <summary>National case</summary>
    National = 1,
    /// <summary>Company case</summary>
    Company = 2,
    /// <summary>Employee case</summary>
    Employee = 3
}

/// <summary>The payroll value types for cases</summary>
public enum ValueType
{
    /// <summary>String (base type string)</summary>
    String = 0,
    /// <summary>Boolean (base type boolean)</summary>
    Boolean = 1,
    /// <summary>Integer (base type numeric)</summary>
    Integer = 2,
    /// <summary>Numeric boolean, any non-zero value means true (base type numeric)</summary>
    NumericBoolean = 3,
    /// <summary>Decimal (base type numeric)</summary>
    Decimal = 4,
    /// <summary>Date and time (base type string)</summary>
    DateTime = 5,
    /// <summary>No value type (base type null)</summary>
    None = 6,

    /// <summary>Date (base type string)</summary>
    Date = 10,
    /// <summary>Web Resource e.g. Url (base type string)</summary>
    WebResource = 11,

    /// <summary>Money (base type numeric)</summary>
    Money = 20,
    /// <summary>Percentage (base type numeric)</summary>
    Percent = 21,
    /// <summary>Hour (base type numeric)</summary>
    Hour = 22,
    /// <summary>Day (base type numeric)</summary>
    Day = 23,
    /// <summary>Week (base type numeric)</summary>
    Week = 24,
    /// <summary>Month (base type numeric)</summary>
    Month = 25,
    /// <summary>Year (base type numeric)</summary>
    Year = 26,
    /// <summary>Distance (base type numeric)</summary>
    Distance = 27
}

/// <summary>The payrun execution stage</summary>
public enum PayrunExecutionPhase
{
    /// <summary>Job setup execution phase</summary>
    Setup = 0,
    /// <summary>Job reevaluation execution phase</summary>
    Reevaluation = 1
}

/// <summary>The payrun job type</summary>
[Flags]
public enum PayrunJobStatus
{
    /// <summary>Draft Legal results (default)</summary>
    Draft = 0x0000,
    /// <summary>Legal results are released for processing</summary>
    Release = 0x0001,
    /// <summary>Legal results are processed</summary>
    Process = 0x0002,
    /// <summary>Legal results has been processed successfully</summary>
    Complete = 0x0004,
    /// <summary>Forecast results</summary>
    Forecast = 0x0008,
    /// <summary>Unreleased Job has been aborted</summary>
    Abort = 0x0010,
    /// <summary>Released Job has been canceled</summary>
    Cancel = 0x0020,
    /// <summary>Working status, including draft, release and process</summary>
    Working = Draft | Release | Process,
    /// <summary>Legal status, including release, process and complete</summary>
    Legal = Release | Process | Complete,
    /// <summary>Final status, including complete, forecast, abort and cancel</summary>
    Final = Complete | Forecast | Abort | Cancel
}

/// <summary>The data merge schema change</summary>
public enum DataMergeSchemaChange
{
    /// <summary>Adds the necessary columns to complete the schema</summary>
    Add = MissingSchemaAction.Add,
    /// <summary>Ignores the extra columns</summary>
    Ignore = MissingSchemaAction.Ignore,
    /// <summary>An <see cref="InvalidOperationException"/> is generated if the specified column mapping is missing.</summary>
    Error = MissingSchemaAction.Error
}

#endregion

#region Error

/// <summary>Payroll script exception</summary>
public class ScriptException : Exception
{
    /// <summary>Initializes a new instance of the exception</summary>
    public ScriptException(string message) :
        base(message)
    {
    }

    /// <summary>Initializes a new instance of the exception</summary>
    public ScriptException(string message, Exception innerException) :
        base(message, innerException)
    {
    }
}

#endregion

#region Collections

/// <summary>Script value dictionary</summary>
public class ScriptDictionary<TKey, TValue>
{
    private Func<TKey, TValue> GetValueHandler { get; }
    private Action<TKey, TValue> SetValueHandler { get; }

    /// <summary>New read-only dictionary</summary>
    public ScriptDictionary(Func<TKey, TValue> getValueHandler)
    {
        GetValueHandler = getValueHandler ?? throw new ArgumentNullException(nameof(getValueHandler));
    }

    /// <summary>New dictionary</summary>
    public ScriptDictionary(Func<TKey, TValue> getValueHandler, Action<TKey, TValue> setValueHandler) :
        this(getValueHandler)
    {
        SetValueHandler = setValueHandler ?? throw new ArgumentNullException(nameof(setValueHandler));
    }

    /// <summary>Query value by index</summary>
    public TValue this[TKey key]
    {
        get => GetValueHandler(key);
        set
        {
            if (SetValueHandler == null)
            {
                throw new ScriptException($"Write operation on read-only scripting dictionary: {key}={value} ");
            }
            SetValueHandler(key, value);
        }
    }
}

#endregion