using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public abstract class ReportRepositoryBase<T>(IRegulationRepository regulationRepository,
    IScriptRepository scriptRepository, IReportAuditRepository auditRepository, bool auditDisabled)
    : ScriptTrackChildDomainRepository<T, ReportAudit>(DbSchema.Tables.Report, DbSchema.ReportColumn.RegulationId,
        regulationRepository, scriptRepository, auditRepository, auditDisabled), IReportRepository<T>
    where T : Report, new()
{
    protected override void GetObjectCreateData(T report, DbParameterCollection parameters)
    {
        parameters.Add(nameof(report.Name), report.Name);
        base.GetObjectCreateData(report, parameters);
    }

    protected override void GetObjectData(T report, DbParameterCollection parameters)
    {
        parameters.Add(nameof(report.NameLocalizations), JsonSerializer.SerializeNamedDictionary(report.NameLocalizations));
        parameters.Add(nameof(report.Description), report.Description);
        parameters.Add(nameof(report.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(report.DescriptionLocalizations));
        parameters.Add(nameof(report.OverrideType), report.OverrideType, DbType.Int32);
        parameters.Add(nameof(report.Category), report.Category);
        parameters.Add(nameof(report.AttributeMode), report.AttributeMode, DbType.Int32);
        parameters.Add(nameof(report.UserType), report.UserType, DbType.Int32);
        parameters.Add(nameof(report.Queries), JsonSerializer.SerializeNamedDictionary(report.Queries));
        parameters.Add(nameof(report.Relations), DefaultJsonSerializer.Serialize(report.Relations));
        parameters.Add(nameof(report.BuildExpression), report.BuildExpression);
        parameters.Add(nameof(report.StartExpression), report.StartExpression);
        parameters.Add(nameof(report.EndExpression), report.EndExpression);
        parameters.Add(nameof(report.Script), report.Script);
        parameters.Add(nameof(report.ScriptVersion), report.ScriptVersion);
        parameters.Add(nameof(report.Binary), report.Binary, DbType.Binary);
        parameters.Add(nameof(report.ScriptHash), report.ScriptHash, DbType.Int32);
        parameters.Add(nameof(report.Attributes), JsonSerializer.SerializeNamedDictionary(report.Attributes));
        parameters.Add(nameof(report.Clusters), JsonSerializer.SerializeList(report.Clusters));
        base.GetObjectData(report, parameters);
    }
}