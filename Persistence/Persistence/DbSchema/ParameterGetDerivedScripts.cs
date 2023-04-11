﻿namespace PayrollEngine.Persistence.DbSchema;

public static class ParameterGetDerivedScripts
{
    public static readonly string TenantId = "@tenantId";
    public static readonly string PayrollId = "@payrollId";
    public static readonly string RegulationDate = "@regulationDate";
    public static readonly string CreatedBefore = "@createdBefore";
    public static readonly string ScriptNames = "@scriptNames";
}