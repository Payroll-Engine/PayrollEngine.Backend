using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public abstract class CaseValueRepository(string tableName, string parentFieldName,
        ICaseFieldRepository caseFieldRepository)
    : CaseValueRepositoryBase<CaseValue>(tableName, parentFieldName, caseFieldRepository);