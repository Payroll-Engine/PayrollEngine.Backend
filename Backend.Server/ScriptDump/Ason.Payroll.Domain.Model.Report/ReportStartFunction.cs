/* ReportStartFunction */
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
// ReSharper restore RedundantUsingDirective

namespace Ason.Payroll.Client.Scripting.Function;

/// <summary>Report start function</summary>
// ReSharper disable once PartialTypeWithSinglePart
public partial class ReportStartFunction : ReportFunction
{
    /// <summary>Initializes a new instance with the function runtime</summary>
    /// <param name="runtime">The runtime</param>
    public ReportStartFunction(object runtime) :
        base(runtime)
    {
    }

    /// <summary>New function instance without runtime (scripting development)</summary>
    /// <param name="sourceFileName">The name of the source file</param>
    public ReportStartFunction(string sourceFileName) :
        base(sourceFileName)
    {
    }

    /// <summary>Set report parameter value</summary>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="value">The parameter value as JSON</param>
    public void SetParameter(string parameterName, string value) =>
        Runtime.SetParameter(parameterName, value);

    /// <summary>Set report parameter typed value</summary>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="value">The default value</param>
    /// <returns>The report parameter value</returns>
    public void SetParameter<T>(string parameterName, T value) =>
        SetParameter(parameterName, JsonSerializer.Serialize(value));

    /// <summary>Check for existing report query</summary>
    /// <param name="queryName">The query name</param>
    public bool HasQuery(string queryName) =>
        Runtime.HasQuery(queryName);

    /// <summary>Get report query</summary>
    /// <param name="queryName">The query name</param>
    /// <returns>The report parameter value as JSON</returns>
    public string GetQuery(string queryName) =>
        Runtime.GetQuery(queryName);

    /// <summary>Set report query value as JSON</summary>
    /// <param name="queryName">The query name</param>
    /// <param name="value">The query value as JSON</param>
    public void SetQuery(string queryName, string value) =>
        Runtime.SetQuery(queryName, value);

    /// <exclude />
    public object Start()
    {
                    if (GetParameter<bool>("AllEmployees") != true)
            {
                SetParameter("Kumulativjournal.Filter", $"Identifier eq '{GetParameter("EmployeeIdentifier")}'");
            }
            if (GetParameter<bool>("EmployeeLanguage") != true)
            {
                SetParameter("Language.Filter", $"Identifier eq '{GetParameter("LanguageSelect")}'");
            }
            return null;
;
        // compiler will optimize this out if the code provides a return
#pragma warning disable 162
        return default;
#pragma warning restore 162
    }
}