using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class WageTypeResultRepository : ChildDomainRepository<WageTypeResult>, IWageTypeResultRepository
{
    public WageTypeResultRepository(IDbContext context) :
        base(DbSchema.Tables.WageTypeResult, DbSchema.WageTypeResultColumn.PayrollResultId, context)
    {
    }

    protected override void GetObjectCreateData(WageTypeResult result, DbParameterCollection parameters)
    {
        parameters.Add(nameof(result.WageTypeId), result.WageTypeId);
        parameters.Add(nameof(result.WageTypeNumber), result.WageTypeNumber);
        parameters.Add(nameof(result.WageTypeName), result.WageTypeName);
        parameters.Add(nameof(result.WageTypeNameLocalizations), JsonSerializer.SerializeNamedDictionary(result.WageTypeNameLocalizations));
        parameters.Add(nameof(result.ValueType), result.ValueType);
        parameters.Add(nameof(result.Value), result.Value);
        parameters.Add(nameof(result.Start), result.Start);
        parameters.Add(nameof(result.StartHash), result.StartHash);
        parameters.Add(nameof(result.End), result.End);
        parameters.Add(nameof(result.Tags), JsonSerializer.SerializeList(result.Tags));
        parameters.Add(nameof(result.Attributes), JsonSerializer.SerializeNamedDictionary(result.Attributes));
        base.GetObjectCreateData(result, parameters);
    }
}