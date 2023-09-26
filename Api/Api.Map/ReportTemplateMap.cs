using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class ReportTemplateMap : ApiMapBase<DomainObject.ReportTemplate, ApiObject.ReportTemplate>
{
    public override partial ApiObject.ReportTemplate ToApi(DomainObject.ReportTemplate domainObject);
    public override partial DomainObject.ReportTemplate ToDomain(ApiObject.ReportTemplate apiObject);
}