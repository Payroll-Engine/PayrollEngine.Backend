namespace PayrollEngine.Persistence.DbSchema;

public static class ParameterGetDerivedCaseRelations
{
    public static readonly string TenantId = "@tenantId";
    public static readonly string PayrollId = "@payrollId";
    public static readonly string RegulationDate = "@regulationDate";
    public static readonly string CreatedBefore = "@createdBefore";
    public static readonly string SourceCaseName = "@sourceCaseName";
    public static readonly string TargetCaseName = "@targetCaseName";
    public static readonly string IncludeClusters = "@includeClusters";
    public static readonly string ExcludeClusters = "@excludeClusters";
}