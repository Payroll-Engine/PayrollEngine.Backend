using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class ReportResponseMap : ApiMapBase<DomainObject.ReportResponse, ApiObject.ReportResponse>
{
    public override partial ApiObject.ReportResponse ToApi(DomainObject.ReportResponse domainObject);
    public override partial DomainObject.ReportResponse ToDomain(ApiObject.ReportResponse apiObject);
}