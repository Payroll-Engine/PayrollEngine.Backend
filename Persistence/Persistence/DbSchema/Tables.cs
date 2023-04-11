
namespace PayrollEngine.Persistence.DbSchema;

public static class Tables
{
    #region Global

    public static readonly string RegulationPermission = "RegulationPermission";

    #endregion

    #region Tenant

    public static readonly string Tenant = "Tenant";
    public static readonly string Webhook = "Webhook";
    public static readonly string WebhookMessage = "WebhookMessage";
    public static readonly string User = "User";
    public static readonly string Division = "Division";
    public static readonly string Task = "Task";
    public static readonly string Log = "Log";
    public static readonly string ReportLog = "ReportLog";
    public static readonly string Employee = "Employee";
    public static readonly string EmployeeDivision = "EmployeeDivision";

    #endregion

    #region Regulation

    public static readonly string Regulation = "Regulation";

    public static readonly string Case = "Case";
    public static readonly string CaseAudit = "CaseAudit";
    public static readonly string CaseRelation = "CaseRelation";
    public static readonly string CaseRelationAudit = "CaseRelationAudit";
    public static readonly string CaseField = "CaseField";
    public static readonly string CaseFieldAudit = "CaseFieldAudit";

    public static readonly string WageType = "WageType";
    public static readonly string WageTypeAudit = "WageTypeAudit";

    public static readonly string Collector = "Collector";
    public static readonly string CollectorAudit = "CollectorAudit";

    public static readonly string Lookup = "Lookup";
    public static readonly string LookupAudit = "LookupAudit";
    public static readonly string LookupValue = "LookupValue";
    public static readonly string LookupValueAudit = "LookupValueAudit";

    public static readonly string Script = "Script";
    public static readonly string ScriptAudit = "ScriptAudit";

    #endregion

    #region Payroll

    public static readonly string Payroll = "Payroll";
    public static readonly string PayrollLayer = "PayrollLayer";

    #endregion

    #region Cases

    public static readonly string GlobalCaseChange = "GlobalCaseChange";
    public static readonly string GlobalCaseValue = "GlobalCaseValue";
    public static readonly string GlobalCaseDocument = "GlobalCaseDocument";
    public static readonly string GlobalCaseValueChange = "GlobalCaseValueChange";
    public static readonly string GlobalCaseValuePivot = "##GlobalCaseValuePivot";
    public static readonly string GlobalCaseChangeValuePivot = "##GlobalCaseChangeValuePivot";

    public static readonly string NationalCaseChange = "NationalCaseChange";
    public static readonly string NationalCaseValue = "NationalCaseValue";
    public static readonly string NationalCaseDocument = "NationalCaseDocument";
    public static readonly string NationalCaseValueChange = "NationalCaseValueChange";
    public static readonly string NationalCaseValuePivot = "##NationalCaseValuePivot";
    public static readonly string NationalCaseChangeValuePivot = "##NationalCaseChangeValuePivot";

    public static readonly string CompanyCaseChange = "CompanyCaseChange";
    public static readonly string CompanyCaseValue = "CompanyCaseValue";
    public static readonly string CompanyCaseDocument = "CompanyCaseDocument";
    public static readonly string CompanyCaseValueChange = "CompanyCaseValueChange";
    public static readonly string CompanyCaseValuePivot = "##CompanyCaseValuePivot";
    public static readonly string CompanyCaseChangeValuePivot = "##CompanyCaseChangeValuePivot";

    public static readonly string EmployeeCaseChange = "EmployeeCaseChange";
    public static readonly string EmployeeCaseValue = "EmployeeCaseValue";
    public static readonly string EmployeeCaseDocument = "EmployeeCaseDocument";
    public static readonly string EmployeeCaseValueChange = "EmployeeCaseValueChange";
    public static readonly string EmployeeCaseValuePivot = "##EmployeeCaseValuePivot";
    public static readonly string EmployeeCaseChangeValuePivot = "##EmployeeCaseChangeValuePivot";

    #endregion

    #region Payrun & Result

    public static readonly string Payrun = "Payrun";
    public static readonly string PayrunParameter = "PayrunParameter";

    public static readonly string PayrunJob = "PayrunJob";
    public static readonly string PayrunJobEmployee = "PayrunJobEmployee";

    public static readonly string PayrollResult = "PayrollResult";
    public static readonly string PayrollResultPivot = "##PayrollResultPivot";

    public static readonly string PayrunResult = "PayrunResult";
    public static readonly string CollectorResult = "CollectorResult";
    public static readonly string CollectorCustomResult = "CollectorCustomResult";
    public static readonly string WageTypeResult = "WageTypeResult";
    public static readonly string WageTypeCustomResult = "WageTypeCustomResult";


    #endregion

    #region Report

    public static readonly string Report = "Report";
    public static readonly string ReportAudit = "ReportAudit";
    public static readonly string ReportParameter = "ReportParameter";
    public static readonly string ReportParameterAudit = "ReportParameterAudit";
    public static readonly string ReportTemplate = "ReportTemplate";
    public static readonly string ReportTemplateAudit = "ReportTemplateAudit";

    #endregion
}