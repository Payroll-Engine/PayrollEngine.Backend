﻿using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CaseRelationAuditService(ICaseRelationAuditRepository repository) :
    ChildApplicationService<ICaseRelationAuditRepository, CaseRelationAudit>(repository), ICaseRelationAuditService;