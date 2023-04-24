using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using PayrollEngine.Client.QueryExpression;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>runtime for the report script</summary>
public abstract class ReportRuntime : RuntimeBase, IReportRuntime
{
    /// <summary>
    /// The runtime settings
    /// </summary>
    protected new ReportRuntimeSettings Settings => base.Settings as ReportRuntimeSettings;

    protected IQueryService QueryService => Settings.QueryService;
    protected IApiControllerContext ControllerContext => Settings.ControllerContext;

    /// <summary>The webhook dispatch service</summary>
    protected IWebhookDispatchService WebhookDispatchService => Settings.WebhookDispatchService;

    /// <summary>The report</summary>
    protected ReportSet Report => Settings.Report;

    /// <summary>The report request</summary>
    protected ReportRequest ReportRequest => Settings.ReportRequest;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportRuntime"/> class.
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    protected ReportRuntime(ReportRuntimeSettings settings) :
        base(settings)
    {
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwner => ReportName;

    /// <inheritdoc />
    public string ReportName => Report.Name;

    public Client.Scripting.Language Language => (Client.Scripting.Language)ReportRequest.Language;

    /// <inheritdoc />
    public object GetReportAttribute(string attributeName) =>
        Report.Attributes?.GetValue<object>(attributeName);

    #region Parameter

    /// <inheritdoc />
    public bool HasParameter(string parameterName)
    {
        // request
        if (ReportRequest.Parameters != null && ReportRequest.Parameters.ContainsKey(parameterName))
        {
            return true;
        }

        // report
        var parameter = Report.Parameters.FirstOrDefault(x => string.Equals(x.Name, parameterName));
        return parameter != null;
    }

    /// <inheritdoc />
    public string GetParameter(string parameterName)
    {
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
            throw new ArgumentException($"Invalid report parameter {parameterName}");
        }
        var reportParameter = Report.Parameters.FirstOrDefault(x => string.Equals(x.Name, parameterName));
        if (reportParameter == null)
        {
            throw new ArgumentException($"Unknown report parameter {parameterName}");
        }

        // report parameter attribute
        if (reportParameter.Attributes != null && reportParameter.Attributes.TryGetValue(attributeName, out var attribute))
        {
            return attribute;
        }

        return null;
    }

    #endregion

    #region Execute Query

    /// <inheritdoc />
    public virtual DataTable ExecuteQuery(string tableName, string methodName, int language, Dictionary<string, string> parameters)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }
        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException(nameof(methodName));
        }

        // language
        if (!Enum.IsDefined((Language)language))
        {
            throw new PayrollException($"Invalid language code: {language}");
        }

        try
        {
            // report query
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var resultTable = QueryService.ExecuteQuery(TenantId, methodName,
                (Language)language, parameters, ControllerContext);
            if (resultTable == null)
            {
                return null;
            }
            Log.Warning($"Execute query: {stopwatch.ElapsedMilliseconds} ms ({resultTable.Rows.Count})");
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
            var value = Language.GetLocalization(lookupValue.ValueLocalizations, lookupValue.Value);
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
        var caseValues = Settings.EmployeCaseValueRepository.
            QueryAsync(Settings.DbContext, employeeId, BuildCaseValueQuery(queryValues)).Result;
        return BuildCaseValueTable(tableName, caseValues);
    }

    private static Query BuildCaseValueQuery(Tuple<int?, string, string, string, long?, long?> queryValues)
    {
        var query = QueryValuesToQuery(queryValues);
        if (string.IsNullOrWhiteSpace(query.OrderBy))
        {
            query.OrderBy = nameof(Model.CaseValue.Start);
        }
        return query;
    }

    private static DataTable BuildCaseValueTable(string tableName, IEnumerable<Model.CaseValue> caseValues)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }

        var dataTable = new DataTable(tableName);

        // setup columns
        dataTable.Columns.Add(nameof(Model.CaseValue.Id), typeof(int));
        dataTable.Columns.Add(nameof(Model.CaseValue.DivisionId), typeof(int));
        dataTable.Columns.Add(nameof(Model.CaseValue.EmployeeId), typeof(int));
        dataTable.Columns.Add(nameof(Model.CaseValue.Status), typeof(int));
        dataTable.Columns.Add(nameof(Model.CaseValue.Created), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.CaseValue.Updated), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.CaseValue.CaseName), typeof(string));
        dataTable.Columns.Add(nameof(Model.CaseValue.CaseNameLocalizations), typeof(string));
        dataTable.Columns.Add(nameof(Model.CaseValue.CaseFieldName), typeof(string));
        dataTable.Columns.Add(nameof(Model.CaseValue.CaseFieldNameLocalizations), typeof(string));
        dataTable.Columns.Add(nameof(Model.CaseValue.CaseSlot), typeof(string));
        dataTable.Columns.Add(nameof(Model.CaseValue.Forecast), typeof(string));
        dataTable.Columns.Add(nameof(Model.CaseValue.Start), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.CaseValue.End), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.CaseValue.CancellationDate), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.CaseValue.ValueType), typeof(int));
        dataTable.Columns.Add(nameof(Model.CaseValue.Value), typeof(string));
        dataTable.Columns.Add(nameof(Model.CaseValue.NumericValue), typeof(decimal));
        dataTable.Columns.Add(nameof(Model.CaseValue.Attributes), typeof(string));

        // setup rows
        foreach (var caseValue in caseValues)
        {
            var row = dataTable.NewRow();
            row[nameof(Model.CaseValue.Id)] = caseValue.Id;
            // division
            if (caseValue.DivisionId.HasValue)
            {
                row[nameof(Model.CaseValue.DivisionId)] = caseValue.DivisionId;
            }
            // employee
            if (caseValue.EmployeeId.HasValue)
            {
                row[nameof(Model.CaseValue.EmployeeId)] = caseValue.EmployeeId;
            }
            row[nameof(Model.CaseValue.Status)] = (int)caseValue.Status;
            row[nameof(Model.CaseValue.Created)] = caseValue.Created;
            row[nameof(Model.CaseValue.Updated)] = caseValue.Updated;
            // case
            row[nameof(Model.CaseValue.CaseName)] = caseValue.CaseName;
            row[nameof(Model.CaseValue.CaseNameLocalizations)] = JsonSerializer.Serialize(caseValue.CaseNameLocalizations);
            // cse field
            row[nameof(Model.CaseValue.CaseFieldName)] = caseValue.CaseFieldName;
            row[nameof(Model.CaseValue.CaseFieldNameLocalizations)] = JsonSerializer.Serialize(caseValue.CaseFieldNameLocalizations);
            // case slot
            row[nameof(Model.CaseValue.CaseSlot)] = caseValue.CaseSlot;
            row[nameof(Model.CaseValue.Forecast)] = caseValue.Forecast;
            if (caseValue.Start.HasValue)
            {
                row[nameof(Model.CaseValue.Start)] = caseValue.Start;
            }
            if (caseValue.End.HasValue)
            {
                row[nameof(Model.CaseValue.End)] = caseValue.End;
            }
            if (caseValue.CancellationDate.HasValue)
            {
                row[nameof(Model.CaseValue.CancellationDate)] = caseValue.CancellationDate;
            }
            // value
            row[nameof(Model.CaseValue.ValueType)] = (int)caseValue.ValueType;
            row[nameof(Model.CaseValue.Value)] = caseValue.Value;
            if (caseValue.NumericValue.HasValue)
            {
                row[nameof(Model.CaseValue.NumericValue)] = caseValue.NumericValue;
            }
            row[nameof(Model.CaseValue.Attributes)] = JsonSerializer.Serialize(caseValue.Attributes);
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
        dataTable.Columns.Add(nameof(Model.WageTypeResult.Id), typeof(int));
        dataTable.Columns.Add(nameof(Model.WageTypeResult.Status), typeof(ObjectStatus));
        dataTable.Columns.Add(nameof(Model.WageTypeResult.Created), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.WageTypeResult.Updated), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.WageTypeResult.PayrollResultId), typeof(int));
        dataTable.Columns.Add(nameof(Model.WageTypeResult.WageTypeId), typeof(int));
        dataTable.Columns.Add(nameof(Model.WageTypeResult.WageTypeNumber), typeof(decimal));
        dataTable.Columns.Add(nameof(Model.WageTypeResult.WageTypeName), typeof(string));
        dataTable.Columns.Add(nameof(Model.WageTypeResult.WageTypeNameLocalizations), typeof(string));
        dataTable.Columns.Add(nameof(Model.WageTypeResult.ValueType), typeof(ValueType));
        dataTable.Columns.Add(nameof(Model.WageTypeResult.Value), typeof(decimal));
        dataTable.Columns.Add(nameof(Model.WageTypeResult.Start), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.WageTypeResult.End), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.WageTypeResult.Tags), typeof(string));
        dataTable.Columns.Add(nameof(Model.WageTypeResult.Attributes), typeof(string));

        // setup rows
        foreach (var result in results)
        {
            var row = dataTable.NewRow();
            row[nameof(Model.WageTypeResult.Id)] = result.Id;
            row[nameof(Model.WageTypeResult.Status)] = result.Status;
            row[nameof(Model.WageTypeResult.Created)] = result.Created;
            row[nameof(Model.WageTypeResult.Updated)] = result.Updated;
            row[nameof(Model.WageTypeResult.PayrollResultId)] = result.PayrollResultId;
            row[nameof(Model.WageTypeResult.WageTypeId)] = result.WageTypeId;
            row[nameof(Model.WageTypeResult.WageTypeNumber)] = result.WageTypeNumber;
            row[nameof(Model.WageTypeResult.WageTypeName)] = result.WageTypeName;
            row[nameof(Model.WageTypeResult.WageTypeNameLocalizations)] = JsonSerializer.Serialize(result.WageTypeNameLocalizations ?? new());
            row[nameof(Model.WageTypeResult.ValueType)] = result.ValueType;
            row[nameof(Model.WageTypeResult.Value)] = result.Value;
            row[nameof(Model.WageTypeResult.Start)] = result.Start;
            row[nameof(Model.WageTypeResult.End)] = result.End;
            row[nameof(Model.WageTypeResult.Tags)] = JsonSerializer.Serialize(result.Tags ?? new());
            row[nameof(Model.WageTypeResult.Attributes)] = JsonSerializer.Serialize(result.Attributes ?? new());
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
        var results = Settings.WageTypeCustomResultRepository.QueryAsync(Settings.DbContext, wageTypeResultId, QueryValuesToQuery(queryValues)).Result;

        // setup columns
        var dataTable = new DataTable(tableName);
        dataTable.Columns.Add(nameof(Model.WageTypeCustomResult.Id), typeof(int));
        dataTable.Columns.Add(nameof(Model.WageTypeCustomResult.Status), typeof(ObjectStatus));
        dataTable.Columns.Add(nameof(Model.WageTypeCustomResult.Created), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.WageTypeCustomResult.Updated), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.WageTypeCustomResult.WageTypeResultId), typeof(int));
        dataTable.Columns.Add(nameof(Model.WageTypeCustomResult.WageTypeNumber), typeof(decimal));
        dataTable.Columns.Add(nameof(Model.WageTypeCustomResult.WageTypeName), typeof(string));
        dataTable.Columns.Add(nameof(Model.WageTypeCustomResult.WageTypeNameLocalizations), typeof(string));
        dataTable.Columns.Add(nameof(Model.WageTypeCustomResult.Source), typeof(string));
        dataTable.Columns.Add(nameof(Model.WageTypeCustomResult.ValueType), typeof(ValueType));
        dataTable.Columns.Add(nameof(Model.WageTypeCustomResult.Value), typeof(decimal));
        dataTable.Columns.Add(nameof(Model.WageTypeCustomResult.Start), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.WageTypeCustomResult.End), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.WageTypeCustomResult.Tags), typeof(string));
        dataTable.Columns.Add(nameof(Model.WageTypeCustomResult.Attributes), typeof(string));

        // setup rows
        foreach (var result in results)
        {
            var row = dataTable.NewRow();
            row[nameof(Model.WageTypeCustomResult.Id)] = result.Id;
            row[nameof(Model.WageTypeCustomResult.Status)] = result.Status;
            row[nameof(Model.WageTypeCustomResult.Created)] = result.Created;
            row[nameof(Model.WageTypeCustomResult.Updated)] = result.Updated;
            row[nameof(Model.WageTypeCustomResult.WageTypeResultId)] = result.WageTypeResultId;
            row[nameof(Model.WageTypeCustomResult.WageTypeNumber)] = result.WageTypeNumber;
            row[nameof(Model.WageTypeCustomResult.WageTypeName)] = result.WageTypeName;
            row[nameof(Model.WageTypeCustomResult.WageTypeNameLocalizations)] = JsonSerializer.Serialize(result.WageTypeNameLocalizations ?? new());
            row[nameof(Model.WageTypeCustomResult.Source)] = result.Source;
            row[nameof(Model.WageTypeCustomResult.ValueType)] = result.ValueType;
            row[nameof(Model.WageTypeCustomResult.Value)] = result.Value;
            row[nameof(Model.WageTypeCustomResult.Start)] = result.Start;
            row[nameof(Model.WageTypeCustomResult.End)] = result.End;
            row[nameof(Model.WageTypeCustomResult.Tags)] = JsonSerializer.Serialize(result.Tags ?? new());
            row[nameof(Model.WageTypeCustomResult.Attributes)] = JsonSerializer.Serialize(result.Attributes ?? new());
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
        dataTable.Columns.Add(nameof(Model.CollectorResult.Id), typeof(int));
        dataTable.Columns.Add(nameof(Model.CollectorResult.Status), typeof(ObjectStatus));
        dataTable.Columns.Add(nameof(Model.CollectorResult.Created), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.CollectorResult.Updated), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.CollectorResult.PayrollResultId), typeof(int));
        dataTable.Columns.Add(nameof(Model.CollectorResult.CollectorId), typeof(int));
        dataTable.Columns.Add(nameof(Model.CollectorResult.CollectorName), typeof(string));
        dataTable.Columns.Add(nameof(Model.CollectorResult.CollectorNameLocalizations), typeof(string));
        dataTable.Columns.Add(nameof(Model.CollectorResult.CollectType), typeof(CollectType));
        dataTable.Columns.Add(nameof(Model.CollectorResult.ValueType), typeof(ValueType));
        dataTable.Columns.Add(nameof(Model.CollectorResult.Value), typeof(decimal));
        dataTable.Columns.Add(nameof(Model.CollectorResult.Start), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.CollectorResult.End), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.CollectorResult.Tags), typeof(string));
        dataTable.Columns.Add(nameof(Model.CollectorResult.Attributes), typeof(string));

        // setup rows
        foreach (var result in results)
        {
            var row = dataTable.NewRow();
            row[nameof(Model.CollectorResult.Id)] = result.Id;
            row[nameof(Model.CollectorResult.Status)] = result.Status;
            row[nameof(Model.CollectorResult.Created)] = result.Created;
            row[nameof(Model.CollectorResult.Updated)] = result.Updated;
            row[nameof(Model.CollectorResult.PayrollResultId)] = result.PayrollResultId;
            row[nameof(Model.CollectorResult.CollectorId)] = result.CollectorId;
            row[nameof(Model.CollectorResult.CollectorName)] = result.CollectorName;
            row[nameof(Model.CollectorResult.CollectorNameLocalizations)] = JsonSerializer.Serialize(result.CollectorNameLocalizations ?? new());
            row[nameof(Model.CollectorResult.CollectType)] = result.CollectType;
            row[nameof(Model.CollectorResult.ValueType)] = result.ValueType;
            row[nameof(Model.CollectorResult.Value)] = result.Value;
            row[nameof(Model.CollectorResult.Start)] = result.Start;
            row[nameof(Model.CollectorResult.End)] = result.End;
            row[nameof(Model.CollectorResult.Tags)] = JsonSerializer.Serialize(result.Tags ?? new());
            row[nameof(Model.CollectorResult.Attributes)] = JsonSerializer.Serialize(result.Attributes ?? new());
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
        dataTable.Columns.Add(nameof(Model.CollectorCustomResult.Id), typeof(int));
        dataTable.Columns.Add(nameof(Model.CollectorCustomResult.Status), typeof(ObjectStatus));
        dataTable.Columns.Add(nameof(Model.CollectorCustomResult.Created), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.CollectorCustomResult.Updated), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.CollectorCustomResult.CollectorResultId), typeof(int));
        dataTable.Columns.Add(nameof(Model.CollectorCustomResult.CollectorName), typeof(string));
        dataTable.Columns.Add(nameof(Model.CollectorCustomResult.CollectorNameLocalizations), typeof(string));
        dataTable.Columns.Add(nameof(Model.CollectorCustomResult.ValueType), typeof(ValueType));
        dataTable.Columns.Add(nameof(Model.CollectorCustomResult.Value), typeof(decimal));
        dataTable.Columns.Add(nameof(Model.CollectorCustomResult.Start), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.CollectorCustomResult.End), typeof(DateTime));
        dataTable.Columns.Add(nameof(Model.CollectorCustomResult.Tags), typeof(string));
        dataTable.Columns.Add(nameof(Model.CollectorCustomResult.Attributes), typeof(string));

        // setup rows
        foreach (var result in results)
        {
            var row = dataTable.NewRow();
            row[nameof(Model.CollectorCustomResult.Id)] = result.Id;
            row[nameof(Model.CollectorCustomResult.Status)] = result.Status;
            row[nameof(Model.CollectorCustomResult.Created)] = result.Created;
            row[nameof(Model.CollectorCustomResult.Updated)] = result.Updated;
            row[nameof(Model.CollectorCustomResult.CollectorResultId)] = result.CollectorResultId;
            row[nameof(Model.CollectorCustomResult.CollectorName)] = result.CollectorName;
            row[nameof(Model.CollectorCustomResult.CollectorNameLocalizations)] = JsonSerializer.Serialize(result.CollectorNameLocalizations ?? new());
            row[nameof(Model.CollectorCustomResult.ValueType)] = result.ValueType;
            row[nameof(Model.CollectorCustomResult.Value)] = result.Value;
            row[nameof(Model.CollectorCustomResult.Start)] = result.Start;
            row[nameof(Model.CollectorCustomResult.End)] = result.End;
            row[nameof(Model.CollectorCustomResult.Tags)] = JsonSerializer.Serialize(result.Tags ?? new());
            row[nameof(Model.CollectorCustomResult.Attributes)] = JsonSerializer.Serialize(result.Attributes ?? new());
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
            row[nameof(PayrunResult.Tags)] = JsonSerializer.Serialize(result.Tags ?? new());
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
        var _ = Settings.ReportLogRepository.CreateAsync(Settings.DbContext, TenantId, new ReportLog
        {
            ReportName = ReportName,
            ReportDate = reportDate ?? Date.Now,
            User = User.Identifier,
            Message = message,
            Key = key
        }).Result;
    }

    #endregion

    #region Webhook

    /// <inheritdoc />
    public string InvokeWebhook(string requestOperation, string requestMessage = null)
    {
        // invoke report function webhook without tracking
        var result = WebhookDispatchService.InvokeAsync(Settings.DbContext, TenantId,
            new()
            {
                Action = WebhookAction.ReportFunctionRequest,
                RequestMessage = requestMessage,
                RequestOperation = requestOperation,
                TrackMessage = false
            },
            userId: UserId).Result;
        return result;
    }

    #endregion

}