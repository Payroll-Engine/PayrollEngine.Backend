/* ReportFunction */
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;

namespace Ason.Payroll.Client.Scripting.Function;

#region Case Value Column

/// <summary>Case value column</summary>
public class CaseValueColumn
{
    /// <summary>The column name</summary>
    public string Name { get; }

    /// <summary>The lookup name</summary>
    public string LookupName { get; }

    /// <summary>The lookup type</summary>
    public Type LookupType { get; }

    /// <summary>New instance of case value column</summary>
    /// <param name="name">The column name</param>
    public CaseValueColumn(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }
        Name = name;
    }

    /// <summary>New instance of case value column</summary>
    /// <param name="name">The column name</param>
    /// <param name="lookupName">The lookup name</param>
    /// <param name="lookupType">The lookup type</param>
    public CaseValueColumn(string name, string lookupName, Type lookupType = null) :
        this(name)
    {
        if (string.IsNullOrWhiteSpace(lookupName))
        {
            throw new ArgumentException(nameof(lookupName));
        }
        LookupName = lookupName;
        LookupType = lookupType ?? typeof(string);
    }
}

#endregion

/// <summary>Base class for report functions</summary>
// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ReportFunction : Function
{
    /// <summary>Initializes a new instance with the function runtime</summary>
    /// <param name="runtime">The runtime</param>
    protected ReportFunction(object runtime) :
        base(runtime)
    {
        // report
        ReportName = Runtime.ReportName;
        Language = (Language)Runtime.Language;
    }

    /// <summary>New function instance without runtime (scripting development)</summary>
    /// <remarks>Use <see cref="Function.GetSourceFileName"/> in your constructor for the source file name</remarks>
    /// <param name="sourceFileName">The name of the source file</param>
    protected ReportFunction(string sourceFileName) :
        base(sourceFileName)
    {
    }

    #region Reports

    /// <summary>Gets the report name</summary>
    /// <value>The name of the case</value>
    public string ReportName { get; }

    /// <summary>Gets the report language</summary>
    public Language Language { get; }

    /// <summary>Get report attribute value</summary>
    /// <param name="attributeName">Name of the attribute</param>
    /// <returns>The report attribute value</returns>
    public object GetReportAttribute(string attributeName) =>
        Runtime.GetReportAttribute(attributeName);

    /// <summary>Get report attribute typed value</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="attributeName">Name of the attribute</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The report attribute value</returns>
    public T GetReportAttribute<T>(string attributeName, T defaultValue = default)
    {
        var value = GetReportAttribute(attributeName);
        return value == null ? defaultValue : (T)Convert.ChangeType(value, typeof(T));
    }

    /// <summary>Check for existing report parameter</summary>
    /// <param name="parameterName">The parameter name</param>
    public bool HasParameter(string parameterName) =>
        Runtime.HasParameter(parameterName);

    /// <summary>Get report parameter</summary>
    /// <param name="parameterName">The parameter name</param>
    /// <returns>The report parameter value as JSON</returns>
    public string GetParameter(string parameterName) =>
        Runtime.GetParameter(parameterName);

    /// <summary>Get report parameter typed value</summary>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The report parameter value</returns>
    public T GetParameter<T>(string parameterName, T defaultValue = default)
    {
        var value = GetParameter(parameterName);
        return value == null ? defaultValue : (T)Convert.ChangeType(value, typeof(T));
    }

    /// <summary>Get report parameter attribute value</summary>
    /// <param name="attributeName">Name of the attribute</param>
    /// <returns>The report attribute value</returns>
    public object GetParameterAttribute(string attributeName) =>
        Runtime.GetParameterAttribute(attributeName);

    /// <summary>Get report parameter attribute typed value</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="attributeName">Name of the attribute</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The report attribute value</returns>
    public T GetParameterAttribute<T>(string attributeName, T defaultValue = default)
    {
        var value = GetParameterAttribute(attributeName);
        return value == null ? defaultValue : (T)Convert.ChangeType(value, typeof(T));
    }

    #endregion

    #region Data Row Values

    /// <summary>Get data row value</summary>
    /// <param name="dataRow">The data row</param>
    /// <param name="column">The column name</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The data row value</returns>
    public T GetValue<T>(DataRow dataRow, string column, T defaultValue = default)
    {
        if (dataRow == null)
        {
            throw new ArgumentNullException(nameof(dataRow));
        }
        if (string.IsNullOrWhiteSpace(column))
        {
            throw new ArgumentException(nameof(column));
        }

        var value = dataRow[column];
        if (value is null or DBNull)
        {
            return defaultValue;
        }
        if (value is T typeValue)
        {
            return typeValue;
        }
        if (value is string stringValue)
        {
            // json escaping
            stringValue = stringValue.Trim('"');
            return (T)JsonSerializer.Deserialize(stringValue, typeof(T));
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Error in column {column}: convert value {value} to type {typeof(T)}", exception);
        }
    }

    /// <summary>Get data rows value</summary>
    /// <param name="dataRows">The data rows</param>
    /// <param name="column">The column name</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The data rows value</returns>
    public List<T> GetValues<T>(IEnumerable<DataRow> dataRows, string column, T defaultValue = default)
    {
        if (dataRows == null)
        {
            throw new ArgumentNullException(nameof(dataRows));
        }
        if (string.IsNullOrWhiteSpace(column))
        {
            throw new ArgumentException(nameof(column));
        }

        var values = new List<T>();
        foreach (DataRow dataRow in dataRows)
        {
            values.Add(GetValue(dataRow, column, defaultValue));
        }
        return values;
    }

    /// <summary>Get data table rows value</summary>
    /// <param name="dataTable">The data table</param>
    /// <param name="column">The column name</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The data table rows value</returns>
    public List<T> GetValues<T>(DataTable dataTable, string column, T defaultValue = default) =>
        GetValues(dataTable.Select(), column, defaultValue);

    /// <summary>Get data row JSON value as list</summary>
    /// <param name="dataRow">The data row</param>
    /// <param name="column">The column name</param>
    /// <returns>The list</returns>
    public List<T> GetList<T>(DataRow dataRow, string column)
    {
        if (dataRow == null)
        {
            throw new ArgumentNullException(nameof(dataRow));
        }
        if (string.IsNullOrWhiteSpace(column))
        {
            throw new ArgumentException(nameof(column));
        }

        var value = dataRow[column];
        if (value is null or DBNull)
        {
            return new();
        }
        if (value is IEnumerable<T> enumerable)
        {
            return new(enumerable);
        }
        if (value is string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new();
            }
            return JsonSerializer.Deserialize<List<T>>(json);
        }

        throw new ArgumentException($"{value} from column {column} is not a JSON list", nameof(column));
    }

    /// <summary>Get data row JSON value as dictionary</summary>
    /// <param name="dataRow">The data row</param>
    /// <param name="column">The column name</param>
    /// <returns>The dictionary</returns>
    public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(DataRow dataRow, string column)
    {
        if (dataRow == null)
        {
            throw new ArgumentNullException(nameof(dataRow));
        }
        if (string.IsNullOrWhiteSpace(column))
        {
            throw new ArgumentException(nameof(column));
        }

        var value = dataRow[column];
        if (value is null or DBNull)
        {
            return new();
        }
        if (value is IDictionary<TKey, TValue> dictionary)
        {
            return new(dictionary);
        }
        if (value is string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new();
            }
            return JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(json);
        }

        throw new ArgumentException($"{value} from column {column} is not a JSON dictionary", nameof(column));
    }

    /// <summary>Get data row JSON JSON value as attribute dictionary</summary>
    /// <param name="dataRow">The data row</param>
    /// <param name="column">The column name</param>
    /// <returns>The attributes dictionary</returns>
    public Dictionary<string, object> GetAttributes(DataRow dataRow, string column) =>
        GetDictionary<string, object>(dataRow, column);

    /// <summary>Get attribute from a data row JSON value</summary>
    /// <param name="dataRow">The data row</param>
    /// <param name="column">The column name</param>
    /// <param name="attribute">The attribute name</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The attribute value</returns>
    public T GetAttribute<T>(DataRow dataRow, string column, string attribute, T defaultValue = default) =>
        (T)Convert.ChangeType(GetAttribute(dataRow, column, attribute, (object)defaultValue), typeof(T));

    /// <summary>Get attribute from a data row JSON value</summary>
    /// <param name="dataRow">The data row</param>
    /// <param name="column">The column name</param>
    /// <param name="attribute">The attribute name</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The attribute value</returns>
    public object GetAttribute(DataRow dataRow, string column, string attribute, object defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(attribute))
        {
            throw new ArgumentException(nameof(attribute));
        }

        var attributes = GetAttributes(dataRow, column);
        return attributes.ContainsKey(attribute) ? attributes[attribute] : defaultValue;
    }

    /// <summary>Get data row JSON value as localizations dictionary</summary>
    /// <param name="dataRow">The data row</param>
    /// <param name="column">The column name</param>
    /// <returns>The attributes dictionary</returns>
    public Dictionary<string, string> GetLocalizations(DataRow dataRow, string column) =>
        GetDictionary<string, string>(dataRow, column);

    /// <summary>Get attribute from a data row JSON value</summary>
    /// <param name="dataRow">The data row</param>
    /// <param name="column">The column name</param>
    /// <param name="language">The language</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The attribute value</returns>
    public string GetLocalization(DataRow dataRow, string column, Language language, string defaultValue = default) =>
        language.GetLocalization(GetLocalizations(dataRow, column), defaultValue);

    #endregion

    #region Web Query

    /// <summary>Query on Api web method</summary>
    /// <param name="tableName">Target table name</param>
    /// <param name="methodName">The query name</param>
    /// <param name="parameters">The method parameters</param>
    /// <returns>New data table, null on empty collection</returns>
    public DataTable ExecuteQuery(string tableName, string methodName, Dictionary<string, string> parameters = null) =>
        ExecuteQuery(tableName, methodName, Language, parameters);

    /// <summary>Query on Api web method</summary>
    /// <param name="tableName">Target table name</param>
    /// <param name="methodName">The query name</param>
    /// <param name="language">The content language</param>
    /// <param name="parameters">The method parameters</param>
    /// <returns>New data table, null on empty collection</returns>
    public DataTable ExecuteQuery(string tableName, string methodName, Language language, Dictionary<string, string> parameters = null) =>
        Runtime.ExecuteQuery(tableName, methodName, (int)language, parameters);

    #endregion

    #region Lookups

    /// <summary>Represents the API LookupValueData object</summary>
    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class LookupValueData
    {
        /// <summary>The lookup key</summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        internal string Key { get; private set; }

        /// <summary>The lookup value as JSON</summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        internal string Value { get; private set; }
    }

    /// <summary>Get lookup values, grouped by lookup</summary>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="language">The language</param>
    /// <param name="lookupName">The lookup name</param>
    /// <param name="regulationDate">The regulation date</param>
    /// <param name="evaluationDate">The evaluation date</param>
    /// <returns>Lookup values dictionary by lookup name, value is a key/value dictionary</returns>
    /// <code>
    /// var lookup = ExecuteLookupQuery(1, "MyLookupName", Language.Italian);
    /// var lookupValue = lookup["MyLookupKey"];
    /// </code>
    public Dictionary<string, string> ExecuteLookupQuery(int payrollId,
        string lookupName, Language language,
        DateTime? regulationDate = null, DateTime? evaluationDate = null)
    {
        var lookups = ExecuteLookupQuery(payrollId, new[] { lookupName },
            language, regulationDate, evaluationDate);
        return lookups.ContainsKey(lookupName) ? lookups[lookupName] : new();
    }

    /// <summary>Get lookup values, grouped by lookup</summary>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="language">The language</param>
    /// <param name="lookupNames">The lookup names</param>
    /// <param name="regulationDate">The regulation date</param>
    /// <param name="evaluationDate">The evaluation date</param>
    /// <returns>Lookup values dictionary by lookup name, value is a key/value dictionary</returns>
    /// <code>
    /// var lookups = ExecuteLookupQuery(1, new[] { "MyLookupName" }, Language.Italian);
    /// var lookupValue = lookups["MyLookupName"]["MyLookupKey"];
    /// </code>
    public Dictionary<string, Dictionary<string, string>> ExecuteLookupQuery(int payrollId,
        IEnumerable<string> lookupNames, Language language,
        DateTime? regulationDate = null, DateTime? evaluationDate = null)
    {
        if (payrollId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(payrollId));
        }
        if (lookupNames == null)
        {
            throw new ArgumentNullException(nameof(lookupNames));
        }
        var names = new HashSet<string>(lookupNames);
        if (!names.Any())
        {
            throw new ArgumentException("Missing lookup names", nameof(lookupNames));
        }

        var lookups = new Dictionary<string, Dictionary<string, string>>();
        var parameters = new Dictionary<string, string>
        {
            {"TenantId", TenantId.ToString()},
            {"PayrollId", payrollId.ToString()},
            {"Language", language.ToString()},
            {"LookupNames", JsonSerializer.Serialize(names)}
        };
        if (regulationDate.HasValue)
        {
            parameters.Add("RegulationDate", regulationDate.Value.ToUtcString());
        }
        if (evaluationDate.HasValue)
        {
            parameters.Add("EvaluationDate", evaluationDate.Value.ToUtcString());
        }

        DataTable lookupValueTable = ExecuteQuery("LookupValues", "GetPayrollLookupValues", parameters);
        foreach (DataRow lookupValuesRow in lookupValueTable.Rows)
        {
            var lookupName = GetValue<string>(lookupValuesRow, "Name");
            if (string.IsNullOrWhiteSpace(lookupName))
            {
                continue;
            }
            if (lookups.ContainsKey(lookupName))
            {
                throw new ScriptException($"Duplicated lookup {lookupName}");
            }
            var values = GetValue<LookupValueData[]>(lookupValuesRow, "Values");
            if (values != null)
            {
                lookups.Add(lookupName, values.ToDictionary(x => x.Key, x => x.Value));
            }
        }
        return lookups;
    }

    #endregion

    #region Cases and Case Values

    /// <summary>Get payroll case fields</summary>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <param name="clusterSetName">The cluster set</param>
    /// <param name="regulationDate">The regulation date</param>
    /// <param name="evaluationDate">The evaluation date</param>
    /// <returns>Payroll case fields</returns>
    public DataTable ExecuteCaseFieldQuery(int payrollId, IEnumerable<string> caseFieldNames = null,
        string clusterSetName = null, DateTime? regulationDate = null, DateTime? evaluationDate = null)
    {
        if (caseFieldNames == null && string.IsNullOrWhiteSpace(clusterSetName))
        {
            throw new ScriptException("Case field query requires case fields or a cluster set");
        }

        var parameters = new Dictionary<string, string>
        {
            {"TenantId", TenantId.ToString()},
            {"PayrollId", payrollId.ToString()}
        };
        if (caseFieldNames != null)
        {
            parameters.Add("CaseFieldNames", JsonSerializer.Serialize(caseFieldNames));
        }
        if (!string.IsNullOrWhiteSpace(clusterSetName))
        {
            parameters.Add("ClusterSetName", clusterSetName);
        }
        if (regulationDate.HasValue)
        {
            parameters.Add("RegulationDate", regulationDate.Value.ToUtcString());
        }
        if (evaluationDate.HasValue)
        {
            parameters.Add("EvaluationDate", evaluationDate.Value.ToUtcString());
        }
        return ExecuteQuery("CaseFields", "GetPayrollCaseFields", parameters);
    }

    /// <summary>Get employees case values as table
    /// Table structure: first column is the employee id, and for any case field a column</summary>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="employeeIds">The employee ids</param>
    /// <param name="tableName">The table name</param>
    /// <param name="columns">The table columns</param>
    /// <param name="language">The language</param>
    /// <param name="valueDate">The value date</param>
    /// <param name="regulationDate">The regulation date</param>
    /// <param name="evaluationDate">The evaluation date</param>
    /// <returns>Employees case values</returns>
    public DataTable ExecuteCaseValueQuery(string tableName, int payrollId,
        IEnumerable<int> employeeIds, IEnumerable<CaseValueColumn> columns,
        Language language, DateTime? valueDate = null,
        DateTime? regulationDate = null, DateTime? evaluationDate = null)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }
        if (payrollId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(payrollId));
        }
        if (employeeIds == null)
        {
            throw new ArgumentNullException(nameof(employeeIds));
        }
        if (columns == null)
        {
            throw new ArgumentNullException(nameof(columns));
        }
        var columnList = columns.ToList();
        if (!columnList.Any())
        {
            throw new ArgumentException(nameof(columns));
        }

        // columns
        var columnNames = new HashSet<string>(columnList.Select(x => x.Name));

        // lookups
        var lookups = GetLookups(payrollId, language, columnList, regulationDate, evaluationDate);

        // result table
        DataTable caseValuesTable = new(tableName);

        // table columns
        caseValuesTable.Columns.Add("EmployeeId", typeof(int));
        var caseFields = ExecuteCaseFieldQuery(payrollId, columnNames,
            regulationDate: regulationDate, evaluationDate: evaluationDate);
        SetupColumns(columnList, caseFields, caseValuesTable);

        // employee case values rows
        foreach (var employeeId in employeeIds)
        {
            // case values
            var parameters = new Dictionary<string, string>
            {
                {"TenantId", TenantId.ToString()},
                {"PayrollId", payrollId.ToString()},
                {"EmployeeId", employeeId.ToString()},
                {"CaseType", CaseType.Employee.ToString()},
                {"Language", language.ToString()},
                {"CaseFieldNames", JsonSerializer.Serialize(columnNames)}
            };
            if (valueDate.HasValue)
            {
                parameters.Add("ValueDate", valueDate.Value.ToUtcString());
            }
            if (regulationDate.HasValue)
            {
                parameters.Add("RegulationDate", regulationDate.Value.ToUtcString());
            }
            if (evaluationDate.HasValue)
            {
                parameters.Add("EvaluationDate", evaluationDate.Value.ToUtcString());
            }
            var caseTimeValuesTable = ExecuteQuery("CaseValues", "GetPayrollTimeCaseValues", parameters);
            if (caseTimeValuesTable.Rows.Count == 0)
            {
                return caseValuesTable;
            }

            // case values row
            var caseValuesRow = GetCaseValueRow(employeeId, columnList, caseValuesTable, caseTimeValuesTable, lookups);
            caseValuesTable.Rows.Add(caseValuesRow);
        }

        return caseValuesTable;
    }

    private Dictionary<string, Dictionary<string, string>> GetLookups(int payrollId, Language language, List<CaseValueColumn> columns,
        DateTime? regulationDate, DateTime? evaluationDate)
    {
        var lookups = new Dictionary<string, Dictionary<string, string>>();
        var lookupNames = columns.Where(x => !string.IsNullOrWhiteSpace(x.LookupName)).Select(x => x.LookupName).ToList();
        if (lookupNames.Any())
        {
            lookups = ExecuteLookupQuery(payrollId, lookupNames, language, regulationDate, evaluationDate);
        }
        return lookups;
    }

    private DataRow GetCaseValueRow(int employeeId, List<CaseValueColumn> columns, DataTable caseValuesTable,
        DataTable caseTimeValuesTable, Dictionary<string, Dictionary<string, string>> lookups)
    {
        var caseValuesRow = caseValuesTable.NewRow();
        foreach (DataRow timeValueRow in caseTimeValuesTable.Rows)
        {
            // case field name and column name
            var caseFieldName = GetValue<string>(timeValueRow, "CaseFieldName");
            if (string.IsNullOrWhiteSpace(caseFieldName))
            {
                continue;
            }

            // column
            var column = columns.FirstOrDefault(x => string.Equals(x.Name, caseFieldName));
            if (column == null)
            {
                continue;
            }

            DataColumn dataColumn = caseValuesTable.Columns[column.Name];
            if (dataColumn == null)
            {
                continue;
            }

            // employee id
            caseValuesRow["EmployeeId"] = employeeId;

            // value
            var jsonValue = GetValue<string>(timeValueRow, "Value");
            // lookup value
            if (!string.IsNullOrWhiteSpace(column.LookupName))
            {
                if (lookups.ContainsKey(column.LookupName) &&
                    lookups[column.LookupName].ContainsKey(jsonValue))
                {
                    jsonValue = lookups[column.LookupName][jsonValue];
                }
                else
                {
                    jsonValue = string.Empty;
                }
            }

            // value type
            var valueType = GetValue<int>(timeValueRow, "ValueType");
            if (Enum.IsDefined(typeof(ValueType), valueType))
            {
                var value = ((ValueType)valueType).JsonToValue(jsonValue);
                caseValuesRow[column.Name] = Convert.ChangeType(value, dataColumn.DataType);
            }
        }

        return caseValuesRow;
    }

    private static void SetupColumns(List<CaseValueColumn> columnList, DataTable caseFields, DataTable caseValuesTable)
    {
        foreach (var column in columnList)
        {
            // find case field row by column name, equals to case field name
            DataRow caseFieldRow = GetCaseFieldRow(caseFields, column);
            if (caseFieldRow == null)
            {
                throw new ScriptException($"Unknown case field {column.Name}");
            }

            // column type
            var valueType = GetCaseValueType(caseFieldRow);
            if (!valueType.HasValue)
            {
                throw new ScriptException(
                    $"Unknown case field type in column {column.Name} (enum: {typeof(ValueType)})");
            }

            // add column
            caseValuesTable.Columns.Add(column.Name, valueType.Value.GetDataType());
        }
    }

    private static ValueType? GetCaseValueType(DataRow caseFieldRow)
    {
        var rawValueType = caseFieldRow["ValueType"];
        return rawValueType switch
        {
            string stringValue => (ValueType)Enum.Parse(typeof(ValueType), stringValue),
            int intValue when Enum.IsDefined(typeof(ValueType), intValue) => (ValueType)rawValueType,
            _ => null
        };
    }

    private static DataRow GetCaseFieldRow(DataTable caseFields, CaseValueColumn column)
    {
        foreach (DataRow caseFieldRow in caseFields.Rows)
        {
            if (string.Equals(caseFieldRow["Name"] as string, column.Name))
            {
                return caseFieldRow;
            }
        }
        return null;
    }

    #endregion

    #region Webhooks

    /// <summary>Invoke report webhook</summary>
    /// <param name="requestOperation">The request operation</param>
    /// <param name="requestMessage">The webhook request message</param>
    /// <returns>The webhook response object</returns>
    public T InvokeWebhook<T>(string requestOperation, object requestMessage = null)
    {
        if (string.IsNullOrWhiteSpace(requestOperation))
        {
            throw new ArgumentException(nameof(requestOperation));
        }

        // webhook request
        var jsonRequest = requestMessage != null ? JsonSerializer.Serialize(requestMessage) : null;
        var jsonResponse = Runtime.InvokeWebhook(requestOperation, jsonRequest);
        if (string.IsNullOrWhiteSpace(jsonResponse))
        {
            return default;
        }
        var response = JsonSerializer.Deserialize<T>(jsonResponse);
        return response;
    }

    #endregion

}