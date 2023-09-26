using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class ReportSetMap : ApiMapBase<DomainObject.ReportSet, ApiObject.ReportSet>
{
    public override partial ApiObject.ReportSet ToApi(DomainObject.ReportSet domainObject);
    public override partial DomainObject.ReportSet ToDomain(ApiObject.ReportSet apiObject);
}