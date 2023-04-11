using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class ReportParameterRepository : TrackChildDomainRepository<ReportParameter, ReportParameterAudit>, IReportParameterRepository
{
    public ReportParameterRepository(IReportParameterAuditRepository auditRepository, IDbContext context) :
        base(DbSchema.Tables.ReportParameter, DbSchema.ReportParameterColumn.ReportId, auditRepository, context)
    {
    }

    protected override void GetObjectCreateData(ReportParameter parameter, DbParameterCollection parameters)
    {
        parameters.Add(nameof(parameter.Name), parameter.Name);
        base.GetObjectCreateData(parameter, parameters);
    }

    protected override void GetObjectData(ReportParameter parameter, DbParameterCollection parameters)
    {
        parameters.Add(nameof(parameter.NameLocalizations), JsonSerializer.SerializeNamedDictionary(parameter.NameLocalizations));
        parameters.Add(nameof(parameter.Description), parameter.Description);
        parameters.Add(nameof(parameter.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(parameter.DescriptionLocalizations));
        parameters.Add(nameof(parameter.Mandatory), parameter.Mandatory);
        parameters.Add(nameof(parameter.Value), parameter.Value);
        parameters.Add(nameof(parameter.ValueType), parameter.ValueType);
        parameters.Add(nameof(parameter.ParameterType), parameter.ParameterType);
        parameters.Add(nameof(parameter.Attributes), JsonSerializer.SerializeNamedDictionary(parameter.Attributes));
        base.GetObjectData(parameter, parameters);
    }
}