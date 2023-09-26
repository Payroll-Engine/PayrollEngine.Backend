using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class ReportLogMap : ApiMapBase<DomainObject.ReportLog, ApiObject.ReportLog>
{
    public override partial ApiObject.ReportLog ToApi(DomainObject.ReportLog domainObject);
    public override partial DomainObject.ReportLog ToDomain(ApiObject.ReportLog apiObject);
}