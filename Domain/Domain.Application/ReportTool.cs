using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application;

public abstract class ReportTool : FunctionToolBase
{
    public static readonly string VariableStartMarker = "$";
    public static readonly string VariableEndMarker = "$";

    public CultureInfo Culture { get; }
    public Tenant Tenant { get; }

    // settings
    // settings
    protected new ReportToolSettings Settings => base.Settings as ReportToolSettings;

    /// <summary>
    /// The webhook dispatcher
    /// </summary>
    public IWebhookDispatchService WebhookDispatchService { get; }

    // repositories
    public IUserRepository UserRepository { get; }
    public IEmployeeRepository EmployeeRepository { get; }
    public IRegulationRepository RegulationRepository { get; }
    public IPayrollRepository PayrollRepository { get; }
    public IPayrunRepository PayrunRepository { get; }
    public IReportSetRepository ReportRepository { get; }
    public IWebhookRepository WebhookRepository { get; }

    protected ReportTool(Tenant tenant, ReportToolSettings settings) :
        base(settings)
    {
        Culture = settings.Culture ?? CultureInfo.CurrentCulture;

        Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));

        UserRepository = settings.UserRepository ?? throw new ArgumentNullException(nameof(settings.UserRepository));
        EmployeeRepository = settings.EmployeeRepository ?? throw new ArgumentNullException(nameof(settings.EmployeeRepository));
        RegulationRepository = settings.RegulationRepository ?? throw new ArgumentNullException(nameof(settings.RegulationRepository));
        PayrollRepository = settings.PayrollRepository ?? throw new ArgumentNullException(nameof(settings.PayrollRepository));
        PayrunRepository = settings.PayrunRepository ?? throw new ArgumentNullException(nameof(settings.PayrunRepository));
        ReportRepository = settings.ReportRepository ?? throw new ArgumentNullException(nameof(settings.ReportRepository));
        WebhookRepository = settings.WebhookRepository ?? throw new ArgumentNullException(nameof(settings.WebhookRepository));

        WebhookDispatchService = settings.WebhookDispatchService ?? throw new ArgumentNullException(nameof(settings.WebhookDispatchService));
    }

    protected async Task SetupReport(ReportSet report, ReportRequest request)
    {
        // report parameters
        if (report.Parameters.Any())
        {
            // parameter validation
            var mandatoryParameters = report.Parameters.Where(x => x.Mandatory);
            foreach (var mandatoryParameter in mandatoryParameters)
            {
                if (request.Parameters == null || !request.Parameters.ContainsKey(mandatoryParameter.Name))
                {
                    // no default value available
                    if (string.IsNullOrWhiteSpace(mandatoryParameter.Value))
                    {
                        throw new QueryException($"Missing mandatory parameter {mandatoryParameter.Name}");
                    }
                }
            }

            // report parameters
            foreach (var reportParameter in report.Parameters)
            {
                reportParameter.Value = await ParseParameter(request.UserId, reportParameter.Value, reportParameter.ParameterType);
            }

            // request parameters
            if (request.Parameters != null)
            {
                foreach (var requestParameter in request.Parameters)
                {
                    var reportParameter = report.Parameters.FirstOrDefault(x => string.Equals(x.Name, requestParameter.Key,
                        StringComparison.InvariantCultureIgnoreCase));
                    if (reportParameter != null)
                    {
                        reportParameter.Value = await ParseParameter(request.UserId, requestParameter.Value, reportParameter.ParameterType);
                    }
                }
            }

            // report parameter variables
            foreach (var reportParameter in report.Parameters)
            {
                reportParameter.Value = SetupParameterVariables(request.UserId, reportParameter.Value, request.Parameters, report.Parameters);
            }
            // apply request parameters
            if (request.Parameters != null)
            {
                foreach (var requestParameter in request.Parameters)
                {
                    request.Parameters[requestParameter.Key] = SetupParameterVariables(request.UserId,
                        requestParameter.Value, request.Parameters, report.Parameters);
                }
            }
        }

        // report relations
        if (report.Relations != null)
        {
            foreach (var relation in report.Relations)
            {
                // child table
                if (!report.Queries.ContainsKey(relation.ChildTable))
                {
                    throw new QueryException($"Invalid relation child table {relation.ChildTable}");
                }
                // parent table
                if (!report.Queries.ContainsKey(relation.ParentTable))
                {
                    throw new QueryException($"Invalid relation parent table {relation.ParentTable}");
                }
            }
        }
    }

    protected void SetupAttributes(DataSet dataSet)
    {
        var relationSourceColumn = "RelationId";
        var relationTargetColumn = nameof(DomainObjectBase.Id);

        // extract attribute tables
        var attributeTables = new Dictionary<DataTable, DataTable>();
        foreach (DataTable dataTable in dataSet.Tables)
        {
            if (dataTable.Columns.Contains(nameof(IDomainAttributeObject.Attributes)))
            {
                var attributesTableName = $"{dataTable.TableName}Attributes";
                var attributesTable = Data.DataTableAttributeExtensions.GetAttributeTable(dataTable, attributesTableName,
                                                                    relationSourceColumn, relationTargetColumn);
                attributeTables.Add(dataTable, attributesTable);
            }
        }

        // add attributes tables and relations
        if (attributeTables.Any())
        {
            foreach (var attributeTable in attributeTables)
            {
                dataSet.Tables.Add(attributeTable.Value);
                var parentColumn = attributeTable.Key.Columns[nameof(DomainObjectBase.Id)];
                if (parentColumn == null)
                {
                    throw new QueryException(
                        $"Missing attribute relation parent column {attributeTable.Key.TableName}.{nameof(DomainObjectBase.Id)}");
                }
                var childColumn = attributeTable.Value.Columns[relationSourceColumn];
                if (childColumn == null)
                {
                    throw new QueryException(
                        $"Missing attribute relation child column {attributeTable.Value.TableName}.{relationSourceColumn}");
                }

                // add data relation
                var relationName = $"{attributeTable.Value.TableName}Relation";
                dataSet.Relations.Add(relationName, parentColumn, childColumn);
            }
        }
    }

    protected async Task<User> GetUserAsync(IDbContext context, int userId)
    {
        // user
        if (userId <= 0)
        {
            throw new QueryException("Missing report user id");
        }
        var user = await UserRepository.GetAsync(context, Tenant.Id, userId);
        if (user == null)
        {
            throw new QueryException($"Unknown user with id {userId}");
        }
        return user;
    }

    #region Argument Parsing

    private async Task<string> ParseParameter(int userId, string parameter, ReportParameterType parameterType)
    {
        // system parameter
        switch (parameterType)
        {
            case ReportParameterType.Value:
                return parameter;
            case ReportParameterType.Now:
                return Date.Now.ToUtcString(Culture);
            case ReportParameterType.Today:
                return Date.Now.Date.ToUtcString(Culture);
            // tenant
            case ReportParameterType.TenantId:
                return Tenant.Id.ToString();
            // user
            case ReportParameterType.UserId:
                return userId.ToString();
        }

        // user parameter
        if (!string.IsNullOrWhiteSpace(parameter))
        {
            return parameterType switch
            {
                // employee
                ReportParameterType.EmployeeId => await GetEmployeeId(parameter),
                // regulation
                ReportParameterType.RegulationId => await GetRegulationId(parameter),
                // payroll
                ReportParameterType.PayrollId => await GetPayrollId(parameter),
                // payrun
                ReportParameterType.PayrunId => await GetPayrunId(parameter),
                // report
                ReportParameterType.ReportId => await GetReportId(parameter),
                // webhook
                ReportParameterType.WebhookId => await GetWebhookId(parameter),
                _ => throw new ArgumentOutOfRangeException(nameof(parameterType), parameterType, null)
            };
        }
        return null;
    }

    private async Task<string> GetEmployeeId(string parameter)
    {
        var employeeId = ParseId(parameter);
        if (!employeeId.HasValue)
        {
            var filter = $"{nameof(Employee.Identifier)} eq '{parameter}'";
            var employee = (await EmployeeRepository.QueryAsync(Settings.DbContext, Tenant.Id, new() { Filter = filter })).FirstOrDefault();
            if (employee != null)
            {
                employeeId = employee.Id;
            }
        }
        return employeeId.HasValue ? employeeId.Value.ToString() : parameter;
    }

    private async Task<string> GetRegulationId(string parameter)
    {
        var regulationId = ParseId(parameter);
        if (!regulationId.HasValue)
        {
            var filter = $"{nameof(Regulation.Name)} eq '{parameter}'";
            var regulation = (await RegulationRepository.QueryAsync(Settings.DbContext, Tenant.Id, new() { Filter = filter })).FirstOrDefault();
            if (regulation != null)
            {
                regulationId = regulation.Id;
            }
        }
        return regulationId.HasValue ? regulationId.Value.ToString() : parameter;
    }

    private async Task<string> GetPayrollId(string parameter)
    {
        var payrollId = ParseId(parameter);
        if (!payrollId.HasValue)
        {
            var filter = $"{nameof(Payroll.Name)} eq '{parameter}'";
            var payroll = (await PayrollRepository.QueryAsync(Settings.DbContext, Tenant.Id, new() { Filter = filter })).FirstOrDefault();
            if (payroll != null)
            {
                payrollId = payroll.Id;
            }
        }
        return payrollId.HasValue ? payrollId.Value.ToString() : parameter;
    }

    private async Task<string> GetPayrunId(string parameter)
    {
        var payrunId = ParseId(parameter);
        if (!payrunId.HasValue)
        {
            var filter = $"{nameof(Payrun.Name)} eq '{parameter}'";
            var payrun = (await PayrunRepository.QueryAsync(Settings.DbContext, Tenant.Id, new() { Filter = filter })).FirstOrDefault();
            if (payrun != null)
            {
                payrunId = payrun.Id;
            }
        }
        return payrunId.HasValue ? payrunId.Value.ToString() : parameter;
    }

    private async Task<string> GetReportId(string parameter)
    {
        var reportId = ParseId(parameter);
        if (!reportId.HasValue)
        {
            var filter = $"{nameof(Report.Name)} eq '{parameter}'";
            var report = (await ReportRepository.QueryAsync(Settings.DbContext, Tenant.Id, new() { Filter = filter })).FirstOrDefault();
            if (report != null)
            {
                reportId = report.Id;
            }
        }
        return reportId.HasValue ? reportId.Value.ToString() : parameter;
    }

    private async Task<string> GetWebhookId(string parameter)
    {
        var webhookId = ParseId(parameter);
        if (!webhookId.HasValue)
        {
            var filter = $"{nameof(Webhook.Name)} eq '{parameter}'";
            var webhook = (await WebhookRepository.QueryAsync(Settings.DbContext, Tenant.Id, new() { Filter = filter })).FirstOrDefault();
            if (webhook != null)
            {
                webhookId = webhook.Id;
            }
        }
        return webhookId.HasValue ? webhookId.Value.ToString() : parameter;
    }

    private string SetupParameterVariables(int userId, string parameterValue,
        IDictionary<string, string> requestParameters, IList<ReportParameter> reportParameters)
    {
        // pattern check
        if (string.IsNullOrWhiteSpace(parameterValue) || !parameterValue.Contains(VariableStartMarker) ||
            !parameterValue.Contains(VariableEndMarker))
        {
            return parameterValue;
        }

        // parameter type
        foreach (var parameterType in Enum.GetValues(typeof(ReportParameterType)))
        {
            var variableTypeName = Enum.GetName(typeof(ReportParameterType), parameterType);
            if (string.IsNullOrWhiteSpace(variableTypeName))
            {
                continue;
            }

            var variableName = $"{VariableStartMarker}{variableTypeName}{VariableEndMarker}";
            if (parameterValue.Contains(variableName))
            {
                string variableValue = null;

                // request parameter
                if (requestParameters != null && requestParameters.TryGetValue(variableTypeName, out var parameter))
                {
                    variableValue = ParseParameter(userId, parameter, (ReportParameterType)parameterType).Result;
                }

                if (string.IsNullOrWhiteSpace(variableValue) && reportParameters != null)
                {
                    var reportParameter = reportParameters.FirstOrDefault(x => string.Equals(x.Name, variableTypeName));
                    if (reportParameter != null)
                    {
                        // matching parameter
                        variableValue = reportParameter.Value;
                    }
                }

                if (string.IsNullOrWhiteSpace(variableValue))
                {
                    throw new QueryException($"Unknown report variable {variableName}");
                }

                // apply variable
                return parameterValue.Replace(variableName, variableValue);
            }
        }
        return parameterValue;
    }

    private static int? ParseId(string value)
    {
        if (int.TryParse(value, out var id))
        {
            return id;
        }
        return null;
    }

    #endregion

}