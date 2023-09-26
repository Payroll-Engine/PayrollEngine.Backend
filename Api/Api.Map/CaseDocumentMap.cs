using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CaseDocumentMap : ApiMapBase<DomainObject.CaseDocument, ApiObject.CaseDocument>
{
    public override partial ApiObject.CaseDocument ToApi(DomainObject.CaseDocument domainObject);
    public override partial DomainObject.CaseDocument ToDomain(ApiObject.CaseDocument apiObject);
}