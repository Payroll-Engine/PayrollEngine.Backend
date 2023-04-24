using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public abstract class CaseValueRepository : CaseValueRepositoryBase<CaseValue>
{
    protected CaseValueRepository(string tableName, string parentFieldName,
        ICaseFieldRepository caseFieldRepository) :
        base(tableName, parentFieldName, caseFieldRepository)
    {
    }
}