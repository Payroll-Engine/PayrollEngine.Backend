using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class ReportAuditRepository() : AuditChildDomainRepository<ReportAudit>(DbSchema.Tables.ReportAudit,
    DbSchema.ReportAuditColumn.ReportId), IReportAuditRepository
{
    protected override void GetObjectCreateData(ReportAudit audit, DbParameterCollection parameters)
    {
        parameters.Add(nameof(audit.Name), audit.Name);
        parameters.Add(nameof(audit.NameLocalizations), JsonSerializer.SerializeNamedDictionary(audit.NameLocalizations));
        parameters.Add(nameof(audit.Description), audit.Description);
        parameters.Add(nameof(audit.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(audit.DescriptionLocalizations));
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType, DbType.Int32);
        parameters.Add(nameof(audit.Category), audit.Category);
        parameters.Add(nameof(audit.AttributeMode), audit.AttributeMode, DbType.Int32);
        parameters.Add(nameof(audit.UserType), audit.UserType, DbType.Int32);
        parameters.Add(nameof(audit.Queries), JsonSerializer.SerializeNamedDictionary(audit.Queries));
        parameters.Add(nameof(audit.Relations), DefaultJsonSerializer.Serialize(audit.Relations));
        parameters.Add(nameof(audit.BuildExpression), audit.BuildExpression);
        parameters.Add(nameof(audit.StartExpression), audit.StartExpression);
        parameters.Add(nameof(audit.EndExpression), audit.EndExpression);
        parameters.Add(nameof(audit.Script), audit.Script);
        parameters.Add(nameof(audit.ScriptVersion), audit.ScriptVersion);
        parameters.Add(nameof(audit.Binary), audit.Binary, DbType.Binary);
        parameters.Add(nameof(audit.ScriptHash), audit.ScriptHash, DbType.Int32);
        parameters.Add(nameof(audit.Attributes), JsonSerializer.SerializeNamedDictionary(audit.Attributes));
        parameters.Add(nameof(audit.Clusters), JsonSerializer.SerializeList(audit.Clusters));
        base.GetObjectCreateData(audit, parameters);
    }
}