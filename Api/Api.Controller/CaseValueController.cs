using System;
using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using System.Collections.Generic;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the case value
/// </summary>
public abstract class CaseValueController<TParentService, TParentRepo, TRepo, TParent>(TParentService parentService,
        ICaseValueService<TRepo, DomainObject.CaseValue> caseValueService,
        IPayrollService payrollsService, IRegulationService regulationService, ILookupSetService lookupSetService,
        IControllerRuntime runtime)
    :
        RepositoryChildObjectController<TParentService, ICaseValueService<TRepo, DomainObject.CaseValue>, TParentRepo,
            TRepo, TParent, DomainObject.CaseValue, ApiObject.CaseValue>(parentService, caseValueService, runtime,
            new CaseValueMap())
    where TParentService : class, IRepositoryApplicationService<TParentRepo>
    where TParentRepo : class, IDomainRepository
    where TRepo : class, IChildDomainRepository<DomainObject.CaseValue>
    where TParent : class, DomainObject.IDomainObject, new()
{
    private IPayrollService PayrollsService { get; } = payrollsService ?? throw new ArgumentNullException(nameof(payrollsService));
    private IRegulationService RegulationService { get; } = regulationService ?? throw new ArgumentNullException(nameof(regulationService));
    private ILookupSetService LookupSetService { get; } = lookupSetService ?? throw new ArgumentNullException(nameof(lookupSetService));

    protected async Task<IEnumerable<string>> GetCaseValueSlotsAsync(int parentId, string caseFieldName) =>
        await Service.GetCaseValueSlotsAsync(Runtime.DbContext, parentId, caseFieldName);

    protected DomainObject.IRegulationLookupProvider NewLookupProvider(DomainObject.Tenant tenant,
        DomainObject.Payroll payroll, DateTime? regulationDate = null, DateTime? evaluationDate = null)
    {
        var currentEvaluationDate = CurrentEvaluationDate;
        regulationDate ??= currentEvaluationDate;
        evaluationDate ??= currentEvaluationDate;

        // new lookup provider
        return new Domain.Scripting.RegulationLookupProvider(
            dbContext: Runtime.DbContext,
            payrollRepository: PayrollsService.Repository,
            payrollQuery: new()
            {
                TenantId = tenant.Id,
                PayrollId = payroll.Id,
                RegulationDate = regulationDate.Value.ToUtc(),
                EvaluationDate = evaluationDate.Value.ToUtc()
            },
            regulationRepository: RegulationService.Repository,
            lookupSetRepository: LookupSetService.Repository);
    }
}