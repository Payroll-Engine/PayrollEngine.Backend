﻿using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CaseRelationAuditMap : ApiMapBase<DomainObject.CaseRelationAudit, ApiObject.CaseRelationAudit>
{
    public override partial ApiObject.CaseRelationAudit ToApi(DomainObject.CaseRelationAudit domainObject);
    public override partial DomainObject.CaseRelationAudit ToDomain(ApiObject.CaseRelationAudit apiObject);
}