using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class ReportTemplateAuditRepository : AuditChildDomainRepository<ReportTemplateAudit>, IReportTemplateAuditRepository
{
    public ReportTemplateAuditRepository() : 
        base(DbSchema.Tables.ReportTemplateAudit, DbSchema.ReportTemplateAuditColumn.ReportTemplateId)
    {
    }

    protected override void GetObjectCreateData(ReportTemplateAudit audit, DbParameterCollection parameters)
    {
        parameters.Add(nameof(audit.Language), audit.Language);
        parameters.Add(nameof(audit.Content), audit.Content);
        parameters.Add(nameof(audit.ContentType), audit.ContentType);
        parameters.Add(nameof(audit.Schema), audit.Schema);
        parameters.Add(nameof(audit.Resource), audit.Resource);
        parameters.Add(nameof(audit.Attributes), JsonSerializer.SerializeNamedDictionary(audit.Attributes));
        base.GetObjectCreateData(audit, parameters);
    }
}