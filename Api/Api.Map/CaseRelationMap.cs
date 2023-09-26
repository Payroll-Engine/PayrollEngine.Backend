using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CaseRelationMap : ApiMapBase<DomainObject.CaseRelation, ApiObject.CaseRelation>
{
    public override partial ApiObject.CaseRelation ToApi(DomainObject.CaseRelation domainObject);
    public override partial DomainObject.CaseRelation ToDomain(ApiObject.CaseRelation apiObject);
}