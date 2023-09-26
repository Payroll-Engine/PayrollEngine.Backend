using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CaseChangeSetupMap : ApiMapBase<DomainObject.CaseChangeSetup, ApiObject.CaseChangeSetup>
{
    public override partial ApiObject.CaseChangeSetup ToApi(DomainObject.CaseChangeSetup domainObject);
    public override partial DomainObject.CaseChangeSetup ToDomain(ApiObject.CaseChangeSetup apiObject);
}