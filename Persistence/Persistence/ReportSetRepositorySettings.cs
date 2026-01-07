using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class ReportSetRepositorySettings
{
    public IRegulationRepository RegulationRepository { get; init; }
    public IReportParameterRepository ReportParameterRepository { get; init; }
    public IReportTemplateRepository ReportTemplateRepository { get; init; }
    public IScriptRepository ScriptRepository { get; init; }
    public IReportAuditRepository AuditRepository { get; init; }
    public bool BulkInsert { get; init; }
}