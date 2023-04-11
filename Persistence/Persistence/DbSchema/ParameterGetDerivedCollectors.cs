namespace PayrollEngine.Persistence.DbSchema;

public static class ParameterGetDerivedCollectors
{
    public static readonly string TenantId = "@tenantId";
    public static readonly string PayrollId = "@payrollId";
    public static readonly string RegulationDate = "@regulationDate";
    public static readonly string CreatedBefore = "@createdBefore";
    public static readonly string CollectorNames = "@collectorNames";
    public static readonly string IncludeClusters = "@includeClusters";
    public static readonly string ExcludeClusters = "@excludeClusters";
}