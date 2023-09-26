﻿using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class PayrunJobInvocationMap : ApiMapBase<DomainObject.PayrunJobInvocation, ApiObject.PayrunJobInvocation>
{
    public override partial ApiObject.PayrunJobInvocation ToApi(DomainObject.PayrunJobInvocation domainObject);
    public override partial DomainObject.PayrunJobInvocation ToDomain(ApiObject.PayrunJobInvocation apiObject);
}