using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class ReportParameterAuditRepository() : AuditChildDomainRepository<ReportParameterAudit>(
        DbSchema.Tables.ReportParameterAudit, DbSchema.ReportParameterAuditColumn.ReportParameterId),
    IReportParameterAuditRepository
{
    protected override void GetObjectCreateData(ReportParameterAudit audit, DbParameterCollection parameters)
    {
        parameters.Add(nameof(audit.Name), audit.Name);
        parameters.Add(nameof(audit.NameLocalizations), JsonSerializer.SerializeNamedDictionary(audit.NameLocalizations));
        parameters.Add(nameof(audit.Description), audit.Description);
        parameters.Add(nameof(audit.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(audit.DescriptionLocalizations));
        parameters.Add(nameof(audit.Mandatory), audit.Mandatory);
        parameters.Add(nameof(audit.Hidden), audit.Hidden);
        parameters.Add(nameof(audit.Value), audit.Value);
        parameters.Add(nameof(audit.ValueType), audit.ValueType);
        parameters.Add(nameof(audit.ParameterType), audit.ParameterType);
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType);
        parameters.Add(nameof(audit.Attributes), JsonSerializer.SerializeNamedDictionary(audit.Attributes));
        base.GetObjectCreateData(audit, parameters);
    }
}