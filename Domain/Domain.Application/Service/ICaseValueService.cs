using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using DomainCaseValue = PayrollEngine.Domain.Model.CaseValue;

namespace PayrollEngine.Domain.Application.Service;

public interface IGlobalCaseValueService : ICaseValueService<IGlobalCaseValueRepository, DomainCaseValue>
{
}

public interface INationalCaseValueService : ICaseValueService<INationalCaseValueRepository, DomainCaseValue>
{
}

public interface ICompanyCaseValueService : ICaseValueService<ICompanyCaseValueRepository, DomainCaseValue>
{
}

public interface IEmployeeCaseValueService : ICaseValueService<IEmployeeCaseValueRepository, DomainCaseValue>
{
}

public interface ICaseValueService<out TRepo, TDomain> : IChildApplicationService<TRepo, TDomain>
    where TRepo : class, IChildDomainRepository<TDomain>
    where TDomain : IDomainObject, new()
{
    /// <summary>
    /// Get all case slots from a specific case field
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The case value parent id</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case values</returns>
    Task<IEnumerable<string>> GetCaseValueSlotsAsync(IDbContext context, int parentId, string caseFieldName);
}