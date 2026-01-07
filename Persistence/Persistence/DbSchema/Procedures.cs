
namespace PayrollEngine.Persistence.DbSchema;

public static class Procedures
{
    // tenant
    public static readonly string DeleteTenant = "DeleteTenant";

    // employee
    public static readonly string DeleteEmployee = "DeleteEmployee";

    // regulation
    public static readonly string GetDerivedPayrollRegulations = "GetDerivedPayrollRegulations";
    // collector
    public static readonly string GetDerivedCollectors = "GetDerivedCollectors";
    // wage type
    public static readonly string GetDerivedWageTypes = "GetDerivedWageTypes";
    // case
    public static readonly string GetDerivedCases = "GetDerivedCases";
    // case field
    public static readonly string GetDerivedCaseFields = "GetDerivedCaseFields";
    public static readonly string GetDerivedCaseFieldsOfCase = "GetDerivedCaseFieldsOfCase";
    // case relation
    public static readonly string GetDerivedCaseRelations = "GetDerivedCaseRelations";
    // reports
    public static readonly string GetDerivedReports = "GetDerivedReports";
    public static readonly string GetDerivedReportParameters = "GetDerivedReportParameters";
    public static readonly string GetDerivedReportTemplates = "GetDerivedReportTemplates";
    // lookup
    public static readonly string GetDerivedLookups = "GetDerivedLookups";
    public static readonly string GetDerivedLookupValues = "GetDerivedLookupValues";
    public static readonly string GetLookupRangeValue = "GetLookupRangeValue";
    public static readonly string DeleteLookup = "DeleteLookup";
    // script
    public static readonly string GetDerivedScripts = "GetDerivedScripts";

    // case values and changes
    public static readonly string GetGlobalCaseValues = "GetGlobalCaseValues";
    public static readonly string GetGlobalCaseChangeValues = "GetGlobalCaseChangeValues";

    public static readonly string GetNationalCaseValues = "GetNationalCaseValues";
    public static readonly string GetNationalCaseChangeValues = "GetNationalCaseChangeValues";

    public static readonly string GetCompanyCaseValues = "GetCompanyCaseValues";
    public static readonly string GetCompanyCaseChangeValues = "GetCompanyCaseChangeValues";

    public static readonly string GetEmployeeCaseValues = "GetEmployeeCaseValues";
    public static readonly string GetEmployeeCaseChangeValues = "GetEmployeeCaseChangeValues";

    // payrun job
    public static readonly string DeletePayrunJob = "DeletePayrunJob";

    // payrun results
    public static readonly string GetConsolidatedPayrunResults = "GetConsolidatedPayrunResults";
    // wage type results
    public static readonly string GetWageTypeResults = "GetWageTypeResults";
    public static readonly string GetConsolidatedWageTypeResults = "GetConsolidatedWageTypeResults";
    public static readonly string GetWageTypeCustomResults = "GetWageTypeCustomResults";
    public static readonly string GetConsolidatedWageTypeCustomResults = "GetConsolidatedWageTypeCustomResults";
    // collector results
    public static readonly string GetCollectorResults = "GetCollectorResults";
    public static readonly string GetConsolidatedCollectorResults = "GetConsolidatedCollectorResults";
    public static readonly string GetCollectorCustomResults = "GetCollectorCustomResults";
    public static readonly string GetConsolidatedCollectorCustomResults = "GetConsolidatedCollectorCustomResults";
    // payroll result values
    public static readonly string GetPayrollResultValues = "GetPayrollResultValues";
}