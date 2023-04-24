using System;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public abstract class CaseValueSetupRepository : CaseValueRepositoryBase<CaseValueSetup>, ICaseValueSetupRepository
{
    public ICaseDocumentRepository CaseDocumentRepository { get; }

    protected CaseValueSetupRepository(string tableName, string parentFieldName,
        ICaseFieldRepository caseFieldRepository,
        ICaseDocumentRepository caseDocumentRepository) :
        base(tableName, parentFieldName, caseFieldRepository)
    {
        CaseDocumentRepository = caseDocumentRepository ?? throw new ArgumentNullException(nameof(caseDocumentRepository));
    }

    protected override async Task OnRetrieved(IDbContext context, int parentId, CaseValueSetup caseValueSetup)
    {
        // documents
        caseValueSetup.Documents = (await CaseDocumentRepository.QueryAsync(context, caseValueSetup.Id)).ToList();
    }

    protected override async Task OnCreatedAsync(IDbContext context, int parentId, CaseValueSetup caseValueSetup)
    {
        // documents
        if (caseValueSetup.Documents != null && caseValueSetup.Documents.Any())
        {
            await CaseDocumentRepository.CreateAsync(context, caseValueSetup.Id, caseValueSetup.Documents);
        }

        await base.OnCreatedAsync(context, parentId, caseValueSetup);
    }

    protected override Task OnUpdatedAsync(IDbContext context, int parentId, CaseValueSetup caseValueSetup)
    {
        throw new NotSupportedException("Update of case value setup is not supported");
    }

    protected override async Task<bool> OnDeletingAsync(IDbContext context, int parentId, int caseValueSetupId)
    {
        // documents
        var documents = (await CaseDocumentRepository.QueryAsync(context, caseValueSetupId)).ToList();
        foreach (var document in documents)
        {
            await CaseDocumentRepository.DeleteAsync(context, caseValueSetupId, document.Id);
        }

        return await base.OnDeletingAsync(context, parentId, caseValueSetupId);
    }
}