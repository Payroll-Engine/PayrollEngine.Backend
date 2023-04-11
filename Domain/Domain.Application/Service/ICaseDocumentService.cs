using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IGlobalCaseDocumentService : ICaseDocumentService<IGlobalCaseDocumentRepository, CaseDocument>
{
}

public interface INationalCaseDocumentService : ICaseDocumentService<INationalCaseDocumentRepository, CaseDocument>
{
}

public interface ICompanyCaseDocumentService : ICaseDocumentService<ICompanyCaseDocumentRepository, CaseDocument>
{
}

public interface IEmployeeCaseDocumentService : ICaseDocumentService<IEmployeeCaseDocumentRepository, CaseDocument>
{
}

public interface ICaseDocumentService<out TRepo, TDomain> : IChildApplicationService<TRepo, TDomain>
    where TRepo : class, IChildDomainRepository<TDomain>
    where TDomain : IDomainObject, new()
{
}