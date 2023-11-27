using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class ReportTemplateAuditRepository() : AuditChildDomainRepository<ReportTemplateAudit>(
        DbSchema.Tables.ReportTemplateAudit, DbSchema.ReportTemplateAuditColumn.ReportTemplateId),
    IReportTemplateAuditRepository
{
    protected override void GetObjectCreateData(ReportTemplateAudit audit, DbParameterCollection parameters)
    {
        parameters.Add(nameof(audit.Name), audit.Name);
        parameters.Add(nameof(audit.Culture), audit.Culture);
        parameters.Add(nameof(audit.Content), audit.Content);
        parameters.Add(nameof(audit.ContentType), audit.ContentType);
        parameters.Add(nameof(audit.Schema), audit.Schema);
        parameters.Add(nameof(audit.Resource), audit.Resource);
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType);
        parameters.Add(nameof(audit.Attributes), JsonSerializer.SerializeNamedDictionary(audit.Attributes));
        base.GetObjectCreateData(audit, parameters);
    }
}