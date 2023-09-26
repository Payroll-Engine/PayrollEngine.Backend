using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class ReportParameterMap : ApiMapBase<DomainObject.ReportParameter, ApiObject.ReportParameter>
{
    public override partial ApiObject.ReportParameter ToApi(DomainObject.ReportParameter domainObject);
    public override partial DomainObject.ReportParameter ToDomain(ApiObject.ReportParameter apiObject);
}