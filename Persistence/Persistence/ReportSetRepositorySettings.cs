using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting.Controller;

namespace PayrollEngine.Persistence;

public class ReportSetRepositorySettings
{
    public IReportParameterRepository ReportParameterRepository { get; set; }
    public IReportTemplateRepository ReportTemplateRepository { get; set; }
    public IReportScriptController<ReportSet> ScriptController { get; set; }
    public IScriptRepository ScriptRepository { get; set; }
    public IReportAuditRepository AuditRepository { get; set; }
    public bool BulkInsert { get; set; }
}