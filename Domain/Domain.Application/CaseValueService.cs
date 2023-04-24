using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public abstract class CaseValueService<TRepo> :
    ChildApplicationService<TRepo, CaseValue>, ICaseValueService<TRepo, CaseValue>
    where TRepo : class, ICaseValueRepository
{
    public IWebhookDispatchService WebhookDispatcher { get; }
    public ILookupSetRepository LookupSetRepository { get; }
    public ICaseRepository CaseRepository { get; }
    public IPayrollRepository PayrollRepository { get; }

    protected CaseValueService(IWebhookDispatchService webhookDispatcher, ILookupSetRepository lookupSetRepository,
        IPayrollRepository payrollRepository,
        ICaseRepository caseRepository, TRepo caseValueRepository) :
        base(caseValueRepository)
    {
        WebhookDispatcher = webhookDispatcher ?? throw new ArgumentNullException(nameof(webhookDispatcher));
        LookupSetRepository = lookupSetRepository ?? throw new ArgumentNullException(nameof(lookupSetRepository));
        PayrollRepository = payrollRepository ?? throw new ArgumentNullException(nameof(payrollRepository));
        CaseRepository = caseRepository ?? throw new ArgumentNullException(nameof(caseRepository));
    }

    public Task<IEnumerable<string>> GetCaseValueSlotsAsync(IDbContext context, int parentId, string caseFieldName) =>
        Repository.GetCaseValueSlotsAsync(context, parentId, caseFieldName);
}