//#define AUDIT_EXPOSE

namespace PayrollEngine.Api.Core;

/// <summary>
/// Development tool: centralized OpenAPI visibility control.
/// Less stuff to render and less scrolling, while working on a specific controller.
/// Set value to false, to enable the controller.
/// </summary>
public static class ApiServiceIgnore
{
    // defaults
    private const bool Ignore = false;
#if AUDIT_EXPOSE
        private const bool AuditIgnore = false;
#else
    private const bool AuditIgnore = true;
#endif

    // http
    public const bool HttpOptions = true;

    // administration
    public const bool Admin = Ignore;

    // shared
    public const bool SharedRegulation = Ignore;

    // tenant
    public const bool Tenant = Ignore;
    public const bool User = Ignore;
    public const bool Division = Ignore;
    public const bool Task = Ignore;
    public const bool Log = Ignore;
    public const bool ReportLog = Ignore;
    public const bool Employee = Ignore;

    public const bool Webhook = Ignore;
    public const bool WebhookMessage = Ignore;

    // regulation
    public const bool Regulation = Ignore;

    public const bool Case = Ignore;
    public const bool CaseAudit = AuditIgnore;
    public const bool CaseField = Ignore;
    public const bool CaseFieldAudit = AuditIgnore;
    public const bool CaseRelation = Ignore;
    public const bool CaseRelationAudit = AuditIgnore;

    public const bool WageType = Ignore;
    public const bool WageTypeAudit = AuditIgnore;

    public const bool Collector = Ignore;
    public const bool CollectorAudit = AuditIgnore;

    public const bool Lookup = Ignore;
    public const bool LookupAudit = AuditIgnore;
    public const bool LookupValue = Ignore;
    public const bool LookupValueAudit = AuditIgnore;

    public const bool Script = Ignore;
    public const bool ScriptAudit = AuditIgnore;

    // payroll
    public const bool Payroll = Ignore;
    public const bool PayrollLayer = Ignore;

    // cases
    public const bool GlobalCaseValue = Ignore;
    public const bool GlobalCaseDocument = Ignore;
    public const bool GlobalCaseChange = Ignore;

    public const bool NationalCaseValue = Ignore;
    public const bool NationalCaseDocument = Ignore;
    public const bool NationalCaseChange = Ignore;

    public const bool CompanyCaseValue = Ignore;
    public const bool CompanyCaseDocument = Ignore;
    public const bool CompanyCaseChange = Ignore;

    public const bool EmployeeCaseValue = Ignore;
    public const bool EmployeeCaseDocument = Ignore;
    public const bool EmployeeCaseChange = Ignore;

    // payrun and results
    public const bool Payrun = Ignore;
    public const bool PayrunParameter = Ignore;
    public const bool PayrunJob = Ignore;
    public const bool PayrollResult = Ignore;
    public const bool PayrollConsolidatedResult = Ignore;
    public const bool PayrollResultSet = Ignore;

    // report
    public const bool Report = Ignore;
    public const bool ReportAudit = AuditIgnore;
    public const bool ReportParameter = Ignore;
    public const bool ReportParameterAudit = AuditIgnore;
    public const bool ReportTemplate = Ignore;
    public const bool ReportTemplateAudit = AuditIgnore;
}