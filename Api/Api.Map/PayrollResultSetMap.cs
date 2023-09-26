using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class PayrollResultSetMap : ApiMapBase<DomainObject.PayrollResultSet, ApiObject.PayrollResultSet>
{
    public override partial ApiObject.PayrollResultSet ToApi(DomainObject.PayrollResultSet domainObject);
    public override partial DomainObject.PayrollResultSet ToDomain(ApiObject.PayrollResultSet apiObject);
}