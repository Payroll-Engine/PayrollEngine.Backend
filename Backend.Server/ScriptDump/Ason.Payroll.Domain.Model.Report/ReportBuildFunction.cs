/* ReportBuildFunction */
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
// ReSharper restore RedundantUsingDirective

namespace Ason.Payroll.Client.Scripting.Function;

/// <summary>Report build function</summary>
// ReSharper disable once PartialTypeWithSinglePart
public partial class ReportBuildFunction : ReportFunction
{
    /// <summary>Initializes a new instance with the function runtime</summary>
    /// <param name="runtime">The runtime</param>
    public ReportBuildFunction(object runtime) :
        base(runtime)
    {
    }

    /// <summary>New function instance without runtime (scripting development)</summary>
    /// <param name="sourceFileName">The name of the source file</param>
    public ReportBuildFunction(string sourceFileName) :
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

    /// <summary>Set report attribute value</summary>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="attributeName">Name of the attribute</param>
    /// <param name="value">The attribute value</param>
    /// <returns>The report attribute value</returns>
    public void SetParameterAttribute(string parameterName, string attributeName, object value) =>
        Runtime.SetParameterAttribute(parameterName, attributeName, value);

    /// <exclude />
    public bool? Build()
    {
                    #region Employee Filter
            var hideEmployeeComboBox = true;
            if (GetParameter<bool>("AllEmployees") != true)
            {

                var queryParameter = "Id eq '" + GetParameter("PayrollId") + "'";
                var payrolls = ExecuteQuery("Payrolls", "QueryPayrolls", new Dictionary<string, string> { { "TenantId", TenantId.ToString() }, { "Filter", Convert.ToString(queryParameter) } });

                var queryParameterDivisions = "";
                if (payrolls.Rows.Count == 1)
                {
                    queryParameterDivisions = "Name eq '" + payrolls.Rows[0]["Name"] + "'";
                }
                var divisions = ExecuteQuery("Divisions", "QueryDivisions", new Dictionary<string, string> { { "TenantId", TenantId.ToString() }, { "Filter", Convert.ToString(queryParameterDivisions) } });
                var divisionIdentifier = "";
                if (divisions.Rows.Count == 1)
                {
                    divisionIdentifier = Convert.ToString(divisions.Rows[0]["id"]);
                }

                hideEmployeeComboBox = false;
                var listSelectionDictionary = new Dictionary<string, object>();
                var employeeDictionary = new Dictionary<string, string>();
                var employees = ExecuteQuery("Kumulativjournal", "QueryEmployees", new Dictionary<string, string> { { "TenantId", TenantId.ToString() }, { "DivisionId", divisionIdentifier } });
                foreach (DataRow employee in employees.Rows)
                {
                    employeeDictionary.Add($"{employee["FirstName"]} {employee["LastName"]} ({employee["Identifier"]})", employee["Identifier"].ToString());
                }
                listSelectionDictionary.Add("dictionary", employeeDictionary);
                listSelectionDictionary.Add("selected", employeeDictionary.Keys.First());
                SetParameterAttribute("EmployeeIdentifier", "input.listSelection", JsonSerializer.Serialize(listSelectionDictionary));
            }
            #endregion
            #region Language Filter

            var hideLanguageComboBox = true;
            if (GetParameter<bool>("EmployeeLanguage") != true)
            {

                hideLanguageComboBox = false;
                var languageSelectionDictionary = new Dictionary<string, object>();
                var regulationId = GetParameter("RegulationId");
                var languageLookups = ExecuteQuery("Lookups", "QueryLookups",
                    new Dictionary<string, string>
                    {
                        {"TenantId", TenantId.ToString()},
                        {"RegulationId", regulationId},
                        {"Filter","name eq 'CH.Swissdec.Language'"}
                    }
                );
                if (languageLookups.Rows.Count < 0)
                {
                    throw new ScriptException("No language lookups found");
                }
                if (languageLookups.Rows[0]["Id"] is not int languageId)
                {
                    throw new ScriptException("language has no id");
                }
                var languageLookupValues = ExecuteQuery("LookupValues", "QueryLookupValues",
                    new Dictionary<string, string>{
                        {"TenantId", TenantId.ToString()},
                        {"LookupId", languageId.ToString()},
                        {"RegulationId", regulationId}
                    });
                var languageDictionary = new Dictionary<string, string> { { string.Empty, string.Empty } };
                foreach (DataRow row in languageLookupValues.Rows)
                {
                    var localization = GetLocalizations(row, "ValueLocalizations")[Language.LanguageCode()];
                    //JsonSerializer must be used, else the result is a string containing -> {"Text":"StatusValue","Code":"CodeValue"}
                    var g = JsonSerializer.Deserialize<Dictionary<string, string>>(localization);
                    languageDictionary.Add(g!["Text"], g["Code"]);
                }
                languageDictionary.Remove(string.Empty);
                languageSelectionDictionary.Add("dictionary", languageDictionary);
                languageSelectionDictionary.Add("selected", languageDictionary.Keys.First());

                SetParameterAttribute("LanguageSelect", "input.listSelection", JsonSerializer.Serialize(languageSelectionDictionary));
            }
            #endregion

            SetParameterAttribute("EmployeeIdentifier", "input.hidden", hideEmployeeComboBox);
            SetParameterAttribute("LanguageSelect", "input.hidden", hideLanguageComboBox);

            return null;
;
        // compiler will optimize this out if the code provides a return
#pragma warning disable 162
        return default;
#pragma warning restore 162
    }
}