namespace PayrollEngine.Persistence.DbSchema;

public static class ParameterGetCollectorResults
{
    public static readonly string TenantId = "@tenantId";
    public static readonly string EmployeeId = "@employeeId";
    public static readonly string DivisionId = "@divisionId";
    public static readonly string PayrunJobId = "@payrunJobId";
    public static readonly string ParentPayrunJobId = "@parentPayrunJobId";
    public static readonly string CollectorNameHashes = "@collectorNameHashes";
    public static readonly string PeriodStart = "@periodStart";
    public static readonly string PeriodStartHashes = "@periodStartHashes";
    public static readonly string PeriodEnd = "@periodEnd";
    public static readonly string Forecast = "@forecast";
    public static readonly string JobStatus = "@jobStatus";
    public static readonly string EvaluationDate = "@evaluationDate";
}