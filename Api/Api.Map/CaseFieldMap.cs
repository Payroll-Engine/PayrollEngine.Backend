using Riok.Mapperly.Abstractions;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CaseFieldMap : ApiMapBase<DomainObject.CaseField, ApiObject.CaseField>
{
    public override partial ApiObject.CaseField ToApi(DomainObject.CaseField domainObject);
    public override partial DomainObject.CaseField ToDomain(ApiObject.CaseField apiObject);
}