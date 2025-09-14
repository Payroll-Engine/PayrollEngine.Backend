using System;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Globalization;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Client.QueryExpression;
using PayrollEngine.Client.Scripting.Runtime;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>runtime for the report script</summary>
public abstract class ReportRuntimeBase : RuntimeBase, IReportRuntime
{
    /// <summary>
    /// The runtime settings
    /// </summary>
    private new ReportRuntimeSettings Settings => base.Settings as ReportRuntimeSettings;

    protected IQueryService QueryService => Settings.QueryService;
    protected IApiControllerContext ControllerContext => Settings.ControllerContext;

    /// <summary>The report</summary>
    protected ReportSet Report => Settings.Report;

    /// <summary>The report request</summary>
    private ReportRequest ReportRequest => Settings.ReportRequest;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportRuntimeBase"/> class.
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    protected ReportRuntimeBase(ReportRuntimeSettings settings) :
        base(settings)
    {
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwner => ReportName;

    /// <inheritdoc />
    public string ReportName => Report.Name;

    /// <inheritdoc />
    public override string UserCulture =>
        ReportRequest.Culture ?? base.UserCulture;

    /// <inheritdoc />
    public object GetReportAttribute(string attributeName) =>
        Report.Attributes?.GetValue<object>(attributeName);

    /// <inheritdoc />
    public void SetReportAttribute(string attributeName, object value)
    {
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }

        // add/change attribute
        if (value != null)
        {
            // ensure attribute collection
            Report.Attributes ??= new();
            Report.Attributes[attributeName] = value;
        }
        else
        {
            // remove attribute
            if (Report.Attributes != null)
            {
                Report.Attributes.Remove(attributeName);
            }
        }
    }

    #region Parameter

    /// <inheritdoc />
    public bool HasParameter(string parameterName)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException(nameof(parameterName));
        }

        // request
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException(nameof(parameterName));
        }

        if (ReportRequest.Parameters != null && ReportRequest.Parameters.ContainsKey(parameterName))
        {
            return true;
        }

        // report
        var parameter = Report.Parameters.FirstOrDefault(x => string.Equals(x.Name, parameterName));
        return parameter?.Value != null;
    }

    /// <inheritdoc />
    public string GetParameter(string parameterName)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException(nameof(parameterName));
        }

        // request
        if (ReportRequest.Parameters != null && ReportRequest.Parameters.TryGetValue(parameterName, out var parameter1))
        {
            return parameter1;
        }

        // report
        var parameter = Report.Parameters.FirstOrDefault(x => string.Equals(x.Name, parameterName));
        return parameter?.Value;
    }

    /// <summary>
    /// Set the report parameter
    /// </summary>
    /// <param name="parameterName">Name of the parameter</param>
    /// <param name="value">The parameter value</param>
    protected void SetParameterInternal(string parameterName, string value)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException(nameof(parameterName));
        }

        // request parameter
        ReportRequest.Parameters ??= new();
        ReportRequest.Parameters[parameterName] = value;

        // report parameter
        var reportParameter = Report.Parameters?.FirstOrDefault(x => string.Equals(x.Name, parameterName));
        if (reportParameter != null)
        {
            reportParameter.Value = value;
        }
    }

    /// <inheritdoc />
    public object GetParameterAttribute(string parameterName, string attributeName)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException(nameof(parameterName));
        }
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }

        // report parameter
        if (Report.Parameters == null)
        {
            throw new ArgumentException($"Invalid report parameter {parameterName}.");
        }
        var reportParameter = Report.Parameters.FirstOrDefault(x => string.Equals(x.Name, parameterName));
        if (reportParameter == null)
        {
            throw new ArgumentException($"Unknown report parameter {parameterName}.");
        }

        // report parameter attribute
        if (reportParameter.Attributes != null && reportParameter.Attributes.TryGetValue(attributeName, out var attribute))
        {
            return attribute;
        }

        return null;
    }

    /// <inheritdoc />
    public void SetParameterAttribute(string parameterName, string attributeName, object value)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException(nameof(parameterName));
        }
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }

        // report parameter
        if (Report.Parameters == null)
        {
            throw new ArgumentException($"Invalid report parameter {parameterName}.");
        }
        var reportParameter = Report.Parameters.FirstOrDefault(x => string.Equals(x.Name, parameterName));
        if (reportParameter == null)
        {
            throw new ArgumentException($"Unknown report parameter {parameterName}.");
        }

        // remove attribute
        if (value == null)
        {
            if (reportParameter.Attributes != null)
            {
                reportParameter.Attributes.Remove(attributeName);
            }
        }
        else
        {
            // add/change attribute
            reportParameter.Attributes ??= new();
            reportParameter.Attributes[attributeName] = value;
        }
    }

    /// <inheritdoc />
    public bool ParameterHidden(string parameterName)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException(nameof(parameterName));
        }

        // report parameter
        if (Report.Parameters == null)
        {
            throw new ArgumentException($"Invalid report parameter {parameterName}.");
        }
        var reportParameter = Report.Parameters.FirstOrDefault(x => string.Equals(x.Name, parameterName));
        if (reportParameter == null)
        {
            throw new ArgumentException($"Unknown report parameter {parameterName}.");
        }
        return reportParameter.Hidden;
    }

    #endregion

    #region Execute Query

    /// <inheritdoc />
    public virtual DataTable ExecuteQuery(string tableName, string methodName, string culture, Dictionary<string, string> parameters)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }
        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException(nameof(methodName));
        }

        // culture
        culture ??= CultureInfo.CurrentCulture.Name;

        try
        {
            // report query
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var resultTable = QueryService.ExecuteQuery(TenantId, methodName, culture, parameters, ControllerContext);
            if (resultTable == null)
            {
                return null;
            }
            Log.Debug($"Execute query: {stopwatch.ElapsedMilliseconds} ms ({resultTable.Rows.Count})");
            resultTable.TableName = tableName;
            resultTable.AcceptChanges();
            return resultTable;
        }
        catch (Exception exception)
        {
            throw new PayrollException(exception.GetBaseMessage(), exception);
        }
    }

    /// <inheritdoc />
    public Dictionary<string, string> ExecuteLookupValueQuery(int regulationId, string lookupName,
        string keyAttribute, string valueAttribute)
    {
        if (string.IsNullOrWhiteSpace(lookupName))
        {
            throw new ArgumentException(nameof(lookupName));
        }
        if (string.IsNullOrWhiteSpace(keyAttribute))
        {
            throw new ArgumentException(nameof(keyAttribute));
        }
        if (string.IsNullOrWhiteSpace(valueAttribute))
        {
            throw new ArgumentException(nameof(valueAttribute));
        }

        // lookup
        var query = new Query
        {
            Status = ObjectStatus.Active,
            Filter = new Equals(nameof(Lookup.Name), lookupName)
        };
        var lookup = Settings.LookupRepository.QueryAsync(Settings.DbContext, regulationId, query).Result.FirstOrDefault();
        if (lookup == null)
        {
            return new();
        }

        // lookup values
        var values = new Dictionary<string, string>();
        var lookupValues = Settings.LookupValueRepository.QueryAsync(Settings.DbContext, lookup.Id,
            new() { Status = ObjectStatus.Active }).Result;
        foreach (var lookupValue in lookupValues)
        {
            // localized lookup json value
            var value = UserCulture.GetLocalization(lookupValue.ValueLocalizations, lookupValue.Value);
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            // deserialize lookup value
            var valueObject = JsonSerializer.Deserialize<Dictionary<string, string>>(value);
            if (valueObject != null && valueObject.ContainsKey(keyAttribute) && valueObject.TryGetValue(valueAttribute, out var value1))
            {
                values.Add(valueObject[keyAttribute], value1);
            }
        }
        return values;
    }

    /// <inheritdoc />
    public DataTable ExecuteGlobalCaseValueQuery(string tableName,
        Tuple<int?, string, string, string, long?, long?> queryValues)
    {
        var caseValues = Settings.GlobalCaseValueRepository.
            QueryAsync(Settings.DbContext, TenantId, BuildCaseValueQuery(queryValues)).Result;
        return BuildCaseValueTable(tableName, caseValues);
    }

    /// <inheritdoc />
    public DataTable ExecuteNationalCaseValueQuery(string tableName,
        Tuple<int?, string, string, string, long?, long?> queryValues)
    {
        var caseValues = Settings.NationalCaseValueRepository.
            QueryAsync(Settings.DbContext, TenantId, BuildCaseValueQuery(queryValues)).Result;
        return BuildCaseValueTable(tableName, caseValues);
    }

    /// <inheritdoc />
    public DataTable ExecuteCompanyCaseValueQuery(string tableName,
        Tuple<int?, string, string, string, long?, long?> queryValues)
    {
        var caseValues = Settings.CompanyCaseValueRepository.
            QueryAsync(Settings.DbContext, TenantId, BuildCaseValueQuery(queryValues)).Result;
        return BuildCaseValueTable(tableName, caseValues);
    }

    /// <inheritdoc />
    public DataTable ExecuteEmployeeCaseValueQuery(string tableName, int employeeId,
        Tuple<int?, string, string, string, long?, long?> queryValues)
    {
        var caseValues = Settings.EmployeeCaseValueRepository.
            QueryAsync(Settings.DbContext, employeeId, BuildCaseValueQuery(queryValues)).Result;
        return BuildCaseValueTable(tableName, caseValues);
    }

    private static Query BuildCaseValueQuery(Tuple<int?, string, string, string, long?, long?> queryValues)
    {
        var query = QueryValuesToQuery(queryValues);
        if (string.IsNullOrWhiteSpace(query.OrderBy))
        {
            query.OrderBy = nameof(CaseValue.Start);
        }
        return query;
    }

    private static DataTable BuildCaseValueTable(string tableName, IEnumerable<CaseValue> caseValues)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }

        var dataTable = new DataTable(tableName);

        // setup columns
        dataTable.Columns.Add(nameof(CaseValue.Id), typeof(int));
        dataTable.Columns.Add(nameof(CaseValue.DivisionId), typeof(int));
        dataTable.Columns.Add(nameof(CaseValue.EmployeeId), typeof(int));
        dataTable.Columns.Add(nameof(CaseValue.Status), typeof(int));
        dataTable.Columns.Add(nameof(CaseValue.Created), typeof(DateTime));
        dataTable.Columns.Add(nameof(CaseValue.Updated), typeof(DateTime));
        dataTable.Columns.Add(nameof(CaseValue.CaseName), typeof(string));
        dataTable.Columns.Add(nameof(CaseValue.CaseNameLocalizations), typeof(string));
        dataTable.Columns.Add(nameof(CaseValue.CaseFieldName), typeof(string));
        dataTable.Columns.Add(nameof(CaseValue.CaseFieldNameLocalizations), typeof(string));
        dataTable.Columns.Add(nameof(CaseValue.CaseSlot), typeof(string));
        dataTable.Columns.Add(nameof(CaseValue.Forecast), typeof(string));
        dataTable.Columns.Add(nameof(CaseValue.Start), typeof(DateTime));
        dataTable.Columns.Add(nameof(CaseValue.End), typeof(DateTime));
        dataTable.Columns.Add(nameof(CaseValue.CancellationDate), typeof(DateTime));
        dataTable.Columns.Add(nameof(CaseValue.ValueType), typeof(int));
        dataTable.Columns.Add(nameof(CaseValue.Value), typeof(string));
        dataTable.Columns.Add(nameof(CaseValue.NumericValue), typeof(decimal));
        dataTable.Columns.Add(nameof(CaseValue.Attributes), typeof(string));

        // setup rows
        foreach (var caseValue in caseValues)
        {
            var row = dataTable.NewRow();
            row[nameof(CaseValue.Id)] = caseValue.Id;
            // division
            if (caseValue.DivisionId.HasValue)
            {
                row[nameof(CaseValue.DivisionId)] = caseValue.DivisionId;
            }
            // employee
            if (caseValue.EmployeeId.HasValue)
            {
                row[nameof(CaseValue.EmployeeId)] = caseValue.EmployeeId;
            }
            row[nameof(CaseValue.Status)] = (int)caseValue.Status;
            row[nameof(CaseValue.Created)] = caseValue.Created;
            row[nameof(CaseValue.Updated)] = caseValue.Updated;
            // case
            row[nameof(CaseValue.CaseName)] = caseValue.CaseName;
            row[nameof(CaseValue.CaseNameLocalizations)] = JsonSerializer.Serialize(caseValue.CaseNameLocalizations);
            // cse field
            row[nameof(CaseValue.CaseFieldName)] = caseValue.CaseFieldName;
            row[nameof(CaseValue.CaseFieldNameLocalizations)] = JsonSerializer.Serialize(caseValue.CaseFieldNameLocalizations);
            // case slot
            row[nameof(CaseValue.CaseSlot)] = caseValue.CaseSlot;
            row[nameof(CaseValue.Forecast)] = caseValue.Forecast;
            if (caseValue.Start.HasValue)
            {
                row[nameof(CaseValue.Start)] = caseValue.Start;
            }
            if (caseValue.End.HasValue)
            {
                row[nameof(CaseValue.End)] = caseValue.End;
            }
            if (caseValue.CancellationDate.HasValue)
            {
                row[nameof(CaseValue.CancellationDate)] = caseValue.CancellationDate;
            }
            // value
            row[nameof(CaseValue.ValueType)] = (int)caseValue.ValueType;
            row[nameof(CaseValue.Value)] = caseValue.Value;
            if (caseValue.NumericValue.HasValue)
            {
                row[nameof(CaseValue.NumericValue)] = caseValue.NumericValue;
            }
            row[nameof(CaseValue.Attributes)] = JsonSerializer.Serialize(caseValue.Attributes);
            dataTable.Rows.Add(row);
        }
        dataTable.AcceptChanges();
        return dataTable;
    }

    /// <inheritdoc />
    public DataTable ExecuteWageTypeQuery(string tableName, int regulationId,
        Tuple<int?, string, string, string, long?, long?> queryValues)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }

        // query
        var query = QueryValuesToQuery(queryValues);
        // exclude scripting columns
        if (string.IsNullOrWhiteSpace(query.Select))
        {
            query.Select =
                $"{nameof(WageType.Id)}, {nameof(WageType.Status)}, {nameof(WageType.Created)}, " +
                $"{nameof(WageType.Updated)}, {nameof(WageType.Name)}, {nameof(WageType.NameLocalizations)}, " +
                $"{nameof(WageType.WageTypeNumber)}, {nameof(WageType.Description)}";
        }

        // query wage types
        var wageTypes = Settings.WageTypeRepository.QueryAsync(Settings.DbContext, regulationId, query).Result;

        // setup columns
        var dataTable = new DataTable(tableName);
        dataTable.Columns.Add(nameof(WageType.Id), typeof(int));
        dataTable.Columns.Add(nameof(WageType.Status), typeof(ObjectStatus));
        dataTable.Columns.Add(nameof(WageType.Created), typeof(DateTime));
        dataTable.Columns.Add(nameof(WageType.Updated), typeof(DateTime));
        dataTable.Columns.Add(nameof(WageType.Name), typeof(string));
        dataTable.Columns.Add(nameof(WageType.NameLocalizations), typeof(string));
        dataTable.Columns.Add(nameof(WageType.Description), typeof(string));
        dataTable.Columns.Add(nameof(WageType.Attributes), typeof(string));
        dataTable.Columns.Add(nameof(WageType.WageTypeNumber), typeof(decimal));

        // setup rows
        foreach (var wageType in wageTypes)
        {
            var row = dataTable.NewRow();
            row[nameof(WageType.Id)] = wageType.Id;
            row[nameof(WageType.Status)] = wageType.Status;
            row[nameof(WageType.Created)] = wageType.Created;
            row[nameof(WageType.Updated)] = wageType.Updated;
            row[nameof(WageType.Name)] = wageType.Name;
            row[nameof(WageType.NameLocalizations)] = JsonSerializer.Serialize(wageType.NameLocalizations ?? new());
            row[nameof(WageType.Description)] = wageType.Description;
            row[nameof(WageType.WageTypeNumber)] = wageType.WageTypeNumber;
            row[nameof(WageType.Attributes)] = JsonSerializer.Serialize(wageType.Attributes ?? new());
            dataTable.Rows.Add(row);
        }
        dataTable.AcceptChanges();
        return dataTable;
    }

    /// <inheritdoc />
    public DataTable ExecutePayrollResultQuery(string tableName, Tuple<int?, string, string, string, long?, long?> queryValues)
    {
        // argument check
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }

        // query
        var results = Settings.PayrollResultRepository.QueryAsync(Settings.DbContext, TenantId, QueryValuesToQuery(queryValues)).Result;

        // setup columns
        var dataTable = new DataTable(tableName);
        dataTable.Columns.Add(nameof(PayrollResult.Id), typeof(int));
        dataTable.Columns.Add(nameof(PayrollResult.Status), typeof(ObjectStatus));
        dataTable.Columns.Add(nameof(PayrollResult.Created), typeof(DateTime));
        dataTable.Columns.Add(nameof(PayrollResult.Updated), typeof(DateTime));
        dataTable.Columns.Add(nameof(PayrollResult.PayrollId), typeof(int));
        dataTable.Columns.Add(nameof(PayrollResult.PayrunId), typeof(int));
        dataTable.Columns.Add(nameof(PayrollResult.PayrunJobId), typeof(int));
        dataTable.Columns.Add(nameof(PayrollResult.EmployeeId), typeof(int));
        dataTable.Columns.Add(nameof(PayrollResult.DivisionId), typeof(int));
        dataTable.Columns.Add(nameof(PayrollResult.CycleName), typeof(string));
        dataTable.Columns.Add(nameof(PayrollResult.CycleStart), typeof(DateTime));
        dataTable.Columns.Add(nameof(PayrollResult.CycleEnd), typeof(DateTime));
        dataTable.Columns.Add(nameof(PayrollResult.PeriodName), typeof(string));
        dataTable.Columns.Add(nameof(PayrollResult.PeriodStart), typeof(DateTime));
        dataTable.Columns.Add(nameof(PayrollResult.PeriodEnd), typeof(DateTime));

        // setup rows
        foreach (var result in results)
        {
            var row = dataTable.NewRow();
            row[nameof(PayrollResult.Id)] = result.Id;
            row[nameof(PayrollResult.Status)] = result.Status;
            row[nameof(PayrollResult.Created)] = result.Created;
            row[nameof(PayrollResult.Updated)] = result.Updated;
            row[nameof(PayrollResult.PayrollId)] = result.PayrollId;
            row[nameof(PayrollResult.PayrunId)] = result.PayrunId;
            row[nameof(PayrollResult.PayrunJobId)] = result.PayrunJobId;
            row[nameof(PayrollResult.EmployeeId)] = result.EmployeeId;
            row[nameof(PayrollResult.DivisionId)] = result.DivisionId;
            row[nameof(PayrollResult.CycleName)] = result.CycleName;
            row[nameof(PayrollResult.CycleStart)] = result.CycleStart;
            row[nameof(PayrollResult.CycleEnd)] = result.CycleEnd;
            row[nameof(PayrollResult.PeriodName)] = result.PeriodName;
            row[nameof(PayrollResult.PeriodStart)] = result.PeriodStart;
            row[nameof(PayrollResult.PeriodEnd)] = result.PeriodEnd;
            dataTable.Rows.Add(row);
        }

        // commit changes
        dataTable.AcceptChanges();
        return dataTable;
    }

    /// <inheritdoc />
    public DataTable ExecuteWageTypeResultQuery(string tableName, int payrollResultId,
        Tuple<int?, string, string, string, long?, long?> queryValues)
    {
        // argument check
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }

        // query
        var results = Settings.WageTypeResultRepository.QueryAsync(Settings.DbContext, payrollResultId, QueryValuesToQuery(queryValues)).Result;

        // setup columns
        var dataTable = new DataTable(tableName);
        dataTable.Columns.Add(nameof(WageTypeResult.Id), typeof(int));
        dataTable.Columns.Add(nameof(WageTypeResult.Status), typeof(ObjectStatus));
        dataTable.Columns.Add(nameof(WageTypeResult.Created), typeof(DateTime));
        dataTable.Columns.Add(nameof(WageTypeResult.Updated), typeof(DateTime));
        dataTable.Columns.Add(nameof(WageTypeResult.PayrollResultId), typeof(int));
        dataTable.Columns.Add(nameof(WageTypeResult.WageTypeId), typeof(int));
        dataTable.Columns.Add(nameof(WageTypeResult.WageTypeNumber), typeof(decimal));
        dataTable.Columns.Add(nameof(WageTypeResult.WageTypeName), typeof(string));
        dataTable.Columns.Add(nameof(WageTypeResult.WageTypeNameLocalizations), typeof(string));
        dataTable.Columns.Add(nameof(WageTypeResult.ValueType), typeof(ValueType));
        dataTable.Columns.Add(nameof(WageTypeResult.Value), typeof(decimal));
        dataTable.Columns.Add(nameof(WageTypeResult.Start), typeof(DateTime));
        dataTable.Columns.Add(nameof(WageTypeResult.End), typeof(DateTime));
        dataTable.Columns.Add(nameof(WageTypeResult.Tags), typeof(string));
        dataTable.Columns.Add(nameof(WageTypeResult.Attributes), typeof(string));

        // setup rows
        foreach (var result in results)
        {
            var row = dataTable.NewRow();
            row[nameof(WageTypeResult.Id)] = result.Id;
            row[nameof(WageTypeResult.Status)] = result.Status;
            row[nameof(WageTypeResult.Created)] = result.Created;
            row[nameof(WageTypeResult.Updated)] = result.Updated;
            row[nameof(WageTypeResult.PayrollResultId)] = result.PayrollResultId;
            row[nameof(WageTypeResult.WageTypeId)] = result.WageTypeId;
            row[nameof(WageTypeResult.WageTypeNumber)] = result.WageTypeNumber;
            row[nameof(WageTypeResult.WageTypeName)] = result.WageTypeName;
            row[nameof(WageTypeResult.WageTypeNameLocalizations)] = JsonSerializer.Serialize(result.WageTypeNameLocalizations ?? new());
            row[nameof(WageTypeResult.ValueType)] = result.ValueType;
            row[nameof(WageTypeResult.Value)] = result.Value;
            row[nameof(WageTypeResult.Start)] = result.Start;
            row[nameof(WageTypeResult.End)] = result.End;
            row[nameof(WageTypeResult.Tags)] = JsonSerializer.Serialize(result.Tags ?? []);
            row[nameof(WageTypeResult.Attributes)] = JsonSerializer.Serialize(result.Attributes ?? new());
            dataTable.Rows.Add(row);
        }

        // commit changes
        dataTable.AcceptChanges();
        return dataTable;
    }

    /// <inheritdoc />
    public DataTable ExecuteWageTypeCustomResultQuery(string tableName, int payrollResultId,
        int wageTypeResultId, Tuple<int?, string, string, string, long?, long?> queryValues)
    {
        // argument check
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }

        // query
        var results = Settings.WageTypeCustomResultRepository.QueryAsync(
            context: Settings.DbContext,
            parentId: wageTypeResultId,
            query: QueryValuesToQuery(queryValues)).Result;

        // setup columns
        var dataTable = new DataTable(tableName);
        dataTable.Columns.Add(nameof(WageTypeCustomResult.Id), typeof(int));
        dataTable.Columns.Add(nameof(WageTypeCustomResult.Status), typeof(ObjectStatus));
        dataTable.Columns.Add(nameof(WageTypeCustomResult.Created), typeof(DateTime));
        dataTable.Columns.Add(nameof(WageTypeCustomResult.Updated), typeof(DateTime));
        dataTable.Columns.Add(nameof(WageTypeCustomResult.WageTypeResultId), typeof(int));
        dataTable.Columns.Add(nameof(WageTypeCustomResult.WageTypeNumber), typeof(decimal));
        dataTable.Columns.Add(nameof(WageTypeCustomResult.WageTypeName), typeof(string));
        dataTable.Columns.Add(nameof(WageTypeCustomResult.WageTypeNameLocalizations), typeof(string));
        dataTable.Columns.Add(nameof(WageTypeCustomResult.Source), typeof(string));
        dataTable.Columns.Add(nameof(WageTypeCustomResult.ValueType), typeof(ValueType));
        dataTable.Columns.Add(nameof(WageTypeCustomResult.Value), typeof(decimal));
        dataTable.Columns.Add(nameof(WageTypeCustomResult.Start), typeof(DateTime));
        dataTable.Columns.Add(nameof(WageTypeCustomResult.End), typeof(DateTime));
        dataTable.Columns.Add(nameof(WageTypeCustomResult.Tags), typeof(string));
        dataTable.Columns.Add(nameof(WageTypeCustomResult.Attributes), typeof(string));

        // setup rows
        foreach (var result in results)
        {
            var row = dataTable.NewRow();
            row[nameof(WageTypeCustomResult.Id)] = result.Id;
            row[nameof(WageTypeCustomResult.Status)] = result.Status;
            row[nameof(WageTypeCustomResult.Created)] = result.Created;
            row[nameof(WageTypeCustomResult.Updated)] = result.Updated;
            row[nameof(WageTypeCustomResult.WageTypeResultId)] = result.WageTypeResultId;
            row[nameof(WageTypeCustomResult.WageTypeNumber)] = result.WageTypeNumber;
            row[nameof(WageTypeCustomResult.WageTypeName)] = result.WageTypeName;
            row[nameof(WageTypeCustomResult.WageTypeNameLocalizations)] = JsonSerializer.Serialize(result.WageTypeNameLocalizations ?? new());
            row[nameof(WageTypeCustomResult.Source)] = result.Source;
            row[nameof(WageTypeCustomResult.ValueType)] = result.ValueType;
            row[nameof(WageTypeCustomResult.Value)] = result.Value;
            row[nameof(WageTypeCustomResult.Start)] = result.Start;
            row[nameof(WageTypeCustomResult.End)] = result.End;
            row[nameof(WageTypeCustomResult.Tags)] = JsonSerializer.Serialize(result.Tags ?? []);
            row[nameof(WageTypeCustomResult.Attributes)] = JsonSerializer.Serialize(result.Attributes ?? new());
            dataTable.Rows.Add(row);
        }

        // commit changes
        dataTable.AcceptChanges();
        return dataTable;
    }

    /// <inheritdoc />
    public DataTable ExecuteCollectorResultQuery(string tableName, int payrollResultId,
        Tuple<int?, string, string, string, long?, long?> queryValues)
    {
        // argument check
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }

        // query
        var results = Settings.CollectorResultRepository.QueryAsync(Settings.DbContext, payrollResultId, QueryValuesToQuery(queryValues)).Result;

        // setup columns
        var dataTable = new DataTable(tableName);
        dataTable.Columns.Add(nameof(CollectorResult.Id), typeof(int));
        dataTable.Columns.Add(nameof(CollectorResult.Status), typeof(ObjectStatus));
        dataTable.Columns.Add(nameof(CollectorResult.Created), typeof(DateTime));
        dataTable.Columns.Add(nameof(CollectorResult.Updated), typeof(DateTime));
        dataTable.Columns.Add(nameof(CollectorResult.PayrollResultId), typeof(int));
        dataTable.Columns.Add(nameof(CollectorResult.CollectorId), typeof(int));
        dataTable.Columns.Add(nameof(CollectorResult.CollectorName), typeof(string));
        dataTable.Columns.Add(nameof(CollectorResult.CollectorNameLocalizations), typeof(string));
        dataTable.Columns.Add(nameof(CollectorResult.CollectMode), typeof(CollectMode));
        dataTable.Columns.Add(nameof(CollectorResult.Negated), typeof(NetPipeStyleUriParser));
        dataTable.Columns.Add(nameof(CollectorResult.ValueType), typeof(ValueType));
        dataTable.Columns.Add(nameof(CollectorResult.Value), typeof(decimal));
        dataTable.Columns.Add(nameof(CollectorResult.Start), typeof(DateTime));
        dataTable.Columns.Add(nameof(CollectorResult.End), typeof(DateTime));
        dataTable.Columns.Add(nameof(CollectorResult.Tags), typeof(string));
        dataTable.Columns.Add(nameof(CollectorResult.Attributes), typeof(string));

        // setup rows
        foreach (var result in results)
        {
            var row = dataTable.NewRow();
            row[nameof(CollectorResult.Id)] = result.Id;
            row[nameof(CollectorResult.Status)] = result.Status;
            row[nameof(CollectorResult.Created)] = result.Created;
            row[nameof(CollectorResult.Updated)] = result.Updated;
            row[nameof(CollectorResult.PayrollResultId)] = result.PayrollResultId;
            row[nameof(CollectorResult.CollectorId)] = result.CollectorId;
            row[nameof(CollectorResult.CollectorName)] = result.CollectorName;
            row[nameof(CollectorResult.CollectorNameLocalizations)] = JsonSerializer.Serialize(result.CollectorNameLocalizations ?? new());
            row[nameof(CollectorResult.CollectMode)] = result.CollectMode;
            row[nameof(CollectorResult.Negated)] = result.Negated;
            row[nameof(CollectorResult.ValueType)] = result.ValueType;
            row[nameof(CollectorResult.Value)] = result.Value;
            row[nameof(CollectorResult.Start)] = result.Start;
            row[nameof(CollectorResult.End)] = result.End;
            row[nameof(CollectorResult.Tags)] = JsonSerializer.Serialize(result.Tags ?? []);
            row[nameof(CollectorResult.Attributes)] = JsonSerializer.Serialize(result.Attributes ?? new());
            dataTable.Rows.Add(row);
        }

        // commit changes
        dataTable.AcceptChanges();
        return dataTable;
    }

    /// <inheritdoc />
    public DataTable ExecuteCollectorCustomResultQuery(string tableName, int payrollResultId,
        int collectorResultId, Tuple<int?, string, string, string, long?, long?> queryValues)
    {
        // argument check
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }

        // query results
        var results = Settings.CollectorCustomResultRepository.QueryAsync(Settings.DbContext, collectorResultId, QueryValuesToQuery(queryValues)).Result;

        // setup columns
        var dataTable = new DataTable(tableName);
        dataTable.Columns.Add(nameof(CollectorCustomResult.Id), typeof(int));
        dataTable.Columns.Add(nameof(CollectorCustomResult.Status), typeof(ObjectStatus));
        dataTable.Columns.Add(nameof(CollectorCustomResult.Created), typeof(DateTime));
        dataTable.Columns.Add(nameof(CollectorCustomResult.Updated), typeof(DateTime));
        dataTable.Columns.Add(nameof(CollectorCustomResult.CollectorResultId), typeof(int));
        dataTable.Columns.Add(nameof(CollectorCustomResult.CollectorName), typeof(string));
        dataTable.Columns.Add(nameof(CollectorCustomResult.CollectorNameLocalizations), typeof(string));
        dataTable.Columns.Add(nameof(CollectorCustomResult.ValueType), typeof(ValueType));
        dataTable.Columns.Add(nameof(CollectorCustomResult.Value), typeof(decimal));
        dataTable.Columns.Add(nameof(CollectorCustomResult.Start), typeof(DateTime));
        dataTable.Columns.Add(nameof(CollectorCustomResult.End), typeof(DateTime));
        dataTable.Columns.Add(nameof(CollectorCustomResult.Tags), typeof(string));
        dataTable.Columns.Add(nameof(CollectorCustomResult.Attributes), typeof(string));

        // setup rows
        foreach (var result in results)
        {
            var row = dataTable.NewRow();
            row[nameof(CollectorCustomResult.Id)] = result.Id;
            row[nameof(CollectorCustomResult.Status)] = result.Status;
            row[nameof(CollectorCustomResult.Created)] = result.Created;
            row[nameof(CollectorCustomResult.Updated)] = result.Updated;
            row[nameof(CollectorCustomResult.CollectorResultId)] = result.CollectorResultId;
            row[nameof(CollectorCustomResult.CollectorName)] = result.CollectorName;
            row[nameof(CollectorCustomResult.CollectorNameLocalizations)] = JsonSerializer.Serialize(result.CollectorNameLocalizations ?? new());
            row[nameof(CollectorCustomResult.ValueType)] = result.ValueType;
            row[nameof(CollectorCustomResult.Value)] = result.Value;
            row[nameof(CollectorCustomResult.Start)] = result.Start;
            row[nameof(CollectorCustomResult.End)] = result.End;
            row[nameof(CollectorCustomResult.Tags)] = JsonSerializer.Serialize(result.Tags ?? []);
            row[nameof(CollectorCustomResult.Attributes)] = JsonSerializer.Serialize(result.Attributes ?? new());
            dataTable.Rows.Add(row);
        }

        // commit changes
        dataTable.AcceptChanges();
        return dataTable;
    }

    /// <inheritdoc />
    public DataTable ExecutePayrunResultQuery(string tableName, int payrollResultId,
        Tuple<int?, string, string, string, long?, long?> queryValues)
    {
        // argument check
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }

        // query results
        var results = Settings.PayrunResultRepository.QueryAsync(Settings.DbContext, payrollResultId, QueryValuesToQuery(queryValues)).Result;

        // setup columns
        var dataTable = new DataTable(tableName);
        dataTable.Columns.Add(nameof(PayrunResult.Id), typeof(int));
        dataTable.Columns.Add(nameof(PayrunResult.Status), typeof(ObjectStatus));
        dataTable.Columns.Add(nameof(PayrunResult.Created), typeof(DateTime));
        dataTable.Columns.Add(nameof(PayrunResult.Updated), typeof(DateTime));
        dataTable.Columns.Add(nameof(PayrunResult.PayrollResultId), typeof(int));
        dataTable.Columns.Add(nameof(PayrunResult.Source), typeof(string));
        dataTable.Columns.Add(nameof(PayrunResult.Name), typeof(string));
        dataTable.Columns.Add(nameof(PayrunResult.NameLocalizations), typeof(string));
        dataTable.Columns.Add(nameof(PayrunResult.Slot), typeof(string));
        dataTable.Columns.Add(nameof(PayrunResult.ValueType), typeof(ValueType));
        dataTable.Columns.Add(nameof(PayrunResult.Value), typeof(string));
        dataTable.Columns.Add(nameof(PayrunResult.NumericValue), typeof(decimal));
        dataTable.Columns.Add(nameof(PayrunResult.Start), typeof(DateTime));
        dataTable.Columns.Add(nameof(PayrunResult.End), typeof(DateTime));
        dataTable.Columns.Add(nameof(PayrunResult.Tags), typeof(string));
        dataTable.Columns.Add(nameof(PayrunResult.Attributes), typeof(string));

        // setup rows
        foreach (var result in results)
        {
            var row = dataTable.NewRow();
            row[nameof(PayrunResult.Id)] = result.Id;
            row[nameof(PayrunResult.Status)] = result.Status;
            row[nameof(PayrunResult.Created)] = result.Created;
            row[nameof(PayrunResult.Updated)] = result.Updated;
            row[nameof(PayrunResult.PayrollResultId)] = result.PayrollResultId;
            row[nameof(PayrunResult.Source)] = result.Source;
            row[nameof(PayrunResult.Name)] = result.Name;
            row[nameof(PayrunResult.NameLocalizations)] = JsonSerializer.Serialize(result.NameLocalizations ?? new());
            row[nameof(PayrunResult.Slot)] = result.Slot;
            row[nameof(PayrunResult.ValueType)] = result.ValueType;
            row[nameof(PayrunResult.Value)] = result.Value;
            if (result.NumericValue.HasValue)
            {
                row[nameof(PayrunResult.NumericValue)] = result.NumericValue;
            }
            row[nameof(PayrunResult.Start)] = result.Start;
            row[nameof(PayrunResult.End)] = result.End;
            row[nameof(PayrunResult.Tags)] = JsonSerializer.Serialize(result.Tags ?? []);
            row[nameof(PayrunResult.Attributes)] = JsonSerializer.Serialize(result.Attributes ?? new());
            dataTable.Rows.Add(row);
        }

        // commit changes
        dataTable.AcceptChanges();
        return dataTable;
    }

    private static Query QueryValuesToQuery(Tuple<int?, string, string, string, long?, long?> queryValues) =>
        new()
        {
            Status = (ObjectStatus?)queryValues.Item1,
            Filter = queryValues.Item2,
            OrderBy = queryValues.Item3,
            Select = queryValues.Item4,
            Top = queryValues.Item5,
            Skip = queryValues.Item6
        };

    #endregion

    #region Report Log

    /// <inheritdoc />
    public void AddReportLog(string message, string key = null, DateTime? reportDate = null)
    {
        _ = Settings.ReportLogRepository.CreateAsync(Settings.DbContext, TenantId, new ReportLog
        {
            ReportName = ReportName,
            ReportDate = reportDate ?? Date.Now,
            User = User.Identifier,
            Message = message,
            Key = key
        }).Result;
    }

    #endregion

}