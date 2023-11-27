using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public abstract class CaseDocumentRepository
    (string tableName, string parentFieldName) : ChildDomainRepository<CaseDocument>(tableName, parentFieldName),
        ICaseDocumentRepository
{
    /// <inheritdoc />
    protected override void GetObjectData(CaseDocument parameter, DbParameterCollection parameters)
    {
        parameters.Add(nameof(parameter.Name), parameter.Name);
        parameters.Add(nameof(parameter.Content), parameter.Content);
        parameters.Add(nameof(parameter.ContentType), parameter.ContentType);
        base.GetObjectData(parameter, parameters);
    }
}