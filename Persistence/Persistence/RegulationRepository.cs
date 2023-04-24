using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class RegulationRepository : ChildDomainRepository<Regulation>, IRegulationRepository
{
    public RegulationRepository() :
        base(DbSchema.Tables.Regulation, DbSchema.RegulationColumn.TenantId)
    {
    }


    protected override void GetObjectCreateData(Regulation regulation, DbParameterCollection parameters)
    {
        parameters.Add(nameof(regulation.SharedRegulation), regulation.SharedRegulation);
        base.GetObjectCreateData(regulation, parameters);
    }

    protected override void GetObjectData(Regulation regulation, DbParameterCollection parameters)
    {
        parameters.Add(nameof(regulation.Name), regulation.Name);
        parameters.Add(nameof(regulation.NameLocalizations), JsonSerializer.SerializeNamedDictionary(regulation.NameLocalizations));
        parameters.Add(nameof(regulation.Version), regulation.Version);
        parameters.Add(nameof(regulation.ValidFrom), regulation.ValidFrom);
        parameters.Add(nameof(regulation.Owner), regulation.Owner);
        parameters.Add(nameof(regulation.Description), regulation.Description);
        parameters.Add(nameof(regulation.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(regulation.DescriptionLocalizations));
        parameters.Add(nameof(regulation.BaseRegulations), JsonSerializer.SerializeList(regulation.BaseRegulations));
        parameters.Add(nameof(regulation.Attributes), JsonSerializer.SerializeNamedDictionary(regulation.Attributes));
        base.GetObjectData(regulation, parameters);
    }
}