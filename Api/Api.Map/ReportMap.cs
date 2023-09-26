using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class ReportMap : ApiMapBase<DomainObject.Report, ApiObject.Report>
{
    public override partial ApiObject.Report ToApi(DomainObject.Report domainObject);
    public override partial DomainObject.Report ToDomain(ApiObject.Report apiObject);
}
