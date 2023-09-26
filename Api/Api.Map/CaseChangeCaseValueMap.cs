using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CaseChangeCaseValueMap : ApiMapBase<DomainObject.CaseChangeCaseValue, ApiObject.CaseChangeCaseValue>
{
    public override partial ApiObject.CaseChangeCaseValue ToApi(DomainObject.CaseChangeCaseValue domainObject);
    public override partial DomainObject.CaseChangeCaseValue ToDomain(ApiObject.CaseChangeCaseValue apiObject);
}