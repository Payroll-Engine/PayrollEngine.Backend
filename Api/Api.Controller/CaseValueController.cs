using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the case value
/// </summary>
public abstract class CaseValueController<TParentService, TParentRepo, TRepo, TParent> :
    RepositoryChildObjectController<TParentService, ICaseValueService<TRepo, DomainObject.CaseValue>, TParentRepo, TRepo, TParent, DomainObject.CaseValue, ApiObject.CaseValue>
    where TParentService : class, IRepositoryApplicationService<TParentRepo>
    where TParentRepo : class, IDomainRepository
    where TRepo : class, IChildDomainRepository<DomainObject.CaseValue>
    where TParent : class, DomainObject.IDomainObject, new()
{
    protected IPayrollService PayrollsService { get; }
    protected IRegulationService RegulationService { get; }
    protected ILookupSetService LookupSetService { get; }

    protected CaseValueController(TParentService parentService, ICaseValueService<TRepo, DomainObject.CaseValue> caseValueService,
        IPayrollService payrollsService, IRegulationService regulationService, ILookupSetService lookupSetService,
        IControllerRuntime runtime) :
        base(parentService, caseValueService, runtime, new CaseValueMap())
    {
        PayrollsService = payrollsService ?? throw new ArgumentNullException(nameof(payrollsService));
        RegulationService = regulationService ?? throw new ArgumentNullException(nameof(regulationService));
        LookupSetService = lookupSetService ?? throw new ArgumentNullException(nameof(lookupSetService));
    }

    protected async Task<IEnumerable<string>> GetCaseValueSlotsAsync(int parentId, string caseFieldName) =>
        await Service.GetCaseValueSlotsAsync(parentId, caseFieldName);

    protected async Task<Domain.Scripting.RegulationLookupProvider> NewLookupProviderAsync(DomainObject.Tenant tenant,
        DomainObject.Payroll payroll, DateTime? regulationDate = null, DateTime? evaluationDate = null)
    {
        var currentEvaluationDate = CurrentEvaluationDate;
        regulationDate ??= currentEvaluationDate;
        evaluationDate ??= currentEvaluationDate;

        // retrieve lookups
        var lookups = (await PayrollsService.Repository.GetDerivedLookupsAsync(
            new()
            {
                TenantId = tenant.Id,
                PayrollId = payroll.Id,
                RegulationDate = regulationDate.Value.ToUtc(),
                EvaluationDate = evaluationDate.Value.ToUtc()
            },
            overrideType: OverrideType.Active)).ToList();

        // new lookup provider
        return new(lookups, RegulationService.Repository, LookupSetService.Repository);
    }
}