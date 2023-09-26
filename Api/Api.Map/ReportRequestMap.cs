using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class ReportRequestMap : ApiMapBase<DomainObject.ReportRequest, ApiObject.ReportRequest>
{
    public override partial ApiObject.ReportRequest ToApi(DomainObject.ReportRequest domainObject);
    public override partial DomainObject.ReportRequest ToDomain(ApiObject.ReportRequest apiObject);
}