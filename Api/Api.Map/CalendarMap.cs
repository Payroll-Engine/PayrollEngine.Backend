using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CalendarMap : ApiMapBase<DomainObject.Calendar, ApiObject.Calendar>
{
    public override partial ApiObject.Calendar ToApi(DomainObject.Calendar domainObject);
    public override partial DomainObject.Calendar ToDomain(ApiObject.Calendar apiObject);
}