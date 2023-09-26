using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class EmployeeMap : ApiMapBase<DomainObject.Employee, ApiObject.Employee>
{
    public override partial ApiObject.Employee ToApi(DomainObject.Employee domainObject);
    public override partial DomainObject.Employee ToDomain(ApiObject.Employee apiObject);
}