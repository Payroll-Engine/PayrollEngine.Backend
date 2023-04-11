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
        ICaseDocumentRepository caseDocumentRepository, IDbContext context) :
        base(tableName, parentFieldName, caseFieldRepository, context)
    {
        CaseDocumentRepository = caseDocumentRepository ?? throw new ArgumentNullException(nameof(caseDocumentRepository));
    }

    protected override async Task OnRetrieved(int parentId, CaseValueSetup caseValueSetup)
    {
        // documents
        caseValueSetup.Documents = (await CaseDocumentRepository.QueryAsync(caseValueSetup.Id)).ToList();
    }

    protected override async Task OnCreatedAsync(int parentId, CaseValueSetup caseValueSetup)
    {
        // documents
        if (caseValueSetup.Documents != null && caseValueSetup.Documents.Any())
        {
            await CaseDocumentRepository.CreateAsync(caseValueSetup.Id, caseValueSetup.Documents);
        }

        await base.OnCreatedAsync(parentId, caseValueSetup);
    }

    protected override Task OnUpdatedAsync(int parentId, CaseValueSetup caseValueSetup)
    {
        throw new NotSupportedException("Update of case value setup is not supported");
    }

    protected override async Task<bool> OnDeletingAsync(int parentId, int caseValueSetupId)
    {
        // documents
        var documents = (await CaseDocumentRepository.QueryAsync(caseValueSetupId)).ToList();
        foreach (var document in documents)
        {
            await CaseDocumentRepository.DeleteAsync(caseValueSetupId, document.Id);
        }

        return await base.OnDeletingAsync(parentId, caseValueSetupId);
    }
}