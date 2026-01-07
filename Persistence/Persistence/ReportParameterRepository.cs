using System;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public class ReportParameterRepository(IRegulationRepository regulationRepository,
    IReportRepository reportRepository, IReportParameterAuditRepository auditRepository, bool auditDisabled) :
    TrackChildDomainRepository<ReportParameter, ReportParameterAudit>(regulationRepository,
        DbSchema.Tables.ReportParameter, DbSchema.ReportParameterColumn.ReportId,
        auditRepository, auditDisabled), IReportParameterRepository
{
    private IReportRepository ReportRepository { get; } = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));

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
        parameters.Add(nameof(parameter.Hidden), parameter.Hidden);
        parameters.Add(nameof(parameter.Value), parameter.Value);
        parameters.Add(nameof(parameter.ValueType), parameter.ValueType);
        parameters.Add(nameof(parameter.ParameterType), parameter.ParameterType);
        parameters.Add(nameof(parameter.OverrideType), parameter.OverrideType);
        parameters.Add(nameof(parameter.Attributes), JsonSerializer.SerializeNamedDictionary(parameter.Attributes));
        base.GetObjectData(parameter, parameters);
    }

    public override async Task<ReportParameter> CreateAsync(IDbContext context, int reportId, ReportParameter parameter)
    {
        await EnsureNamespaceAsync(context, reportId, parameter);
        return await base.CreateAsync(context, reportId, parameter);
    }

    public override async Task<ReportParameter> UpdateAsync(IDbContext context, int reportId, ReportParameter parameter)
    {
        await EnsureNamespaceAsync(context, reportId, parameter);
        return await base.UpdateAsync(context, reportId, parameter);
    }

    private async Task EnsureNamespaceAsync(IDbContext context, int reportId, ReportParameter parameter)
    {
        var regulationId = await ReportRepository.GetParentIdAsync(context, reportId);
        if (!regulationId.HasValue)
        {
            return;
        }
        await ApplyNamespaceAsync(context, regulationId.Value, parameter);
    }
}