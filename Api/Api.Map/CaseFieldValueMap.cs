using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;
    
/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CaseFieldValueMap : ApiMapBase<DomainObject.CaseFieldValue, ApiObject.CaseFieldValue>
{
    public override partial ApiObject.CaseFieldValue ToApi(DomainObject.CaseFieldValue domainObject);
    public override partial DomainObject.CaseFieldValue ToDomain(ApiObject.CaseFieldValue apiObject);
}