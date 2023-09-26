using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class ConsolidatedPayrollResultMap : ApiMapBase<DomainObject.ConsolidatedPayrollResult, ApiObject.ConsolidatedPayrollResult>
{
    public override partial ApiObject.ConsolidatedPayrollResult ToApi(DomainObject.ConsolidatedPayrollResult domainObject);
    public override partial DomainObject.ConsolidatedPayrollResult ToDomain(ApiObject.ConsolidatedPayrollResult apiObject);
}