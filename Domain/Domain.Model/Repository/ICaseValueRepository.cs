using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for case values
/// </summary>
public interface ICaseValueRepository<TDomain> : IChildDomainRepository<TDomain>
    where TDomain : CaseValue, new()
{
    /// <summary>
    /// Get all case slots from a specific case field
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The case value parent id</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case values</returns>
    Task<IEnumerable<string>> GetCaseValueSlotsAsync(IDbContext context, int parentId, string caseFieldName);

    /// <summary>
    /// Get all case values from a specific case field
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The case value query</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>The case values</returns>
    Task<IEnumerable<CaseValue>> GetCaseValuesAsync(IDbContext context, DomainCaseValueQuery query,
        string caseFieldName = null, DateTime? evaluationDate = null);

    /// <summary>
    /// Get all case values from a specific case field restricted to a time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The case value query</param>
    /// <param name="period">The period</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>The case values</returns>
    Task<IEnumerable<CaseValue>> GetPeriodCaseValuesAsync(IDbContext context, DomainCaseValueQuery query, DatePeriod period,
        string caseFieldName = null, DateTime? evaluationDate = null);

    /// <summary>
    /// Get retro case values from a certain time period
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The case value query</param>
    /// <param name="period">The date period</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The retro case value</returns>
    Task<CaseValue> GetRetroCaseValueAsync(IDbContext context, DomainCaseValueQuery query, DatePeriod period, string caseFieldName);
}

/// <summary>
/// Repository for global case values
/// </summary>
public interface IGlobalCaseValueRepository : ICaseValueRepository;

/// <summary>
/// Repository for national case values
/// </summary>
public interface INationalCaseValueRepository : ICaseValueRepository;

/// <summary>
/// Repository for company case values
/// </summary>
public interface ICompanyCaseValueRepository : ICaseValueRepository;

/// <summary>
/// Repository for employee case values
/// </summary>
public interface IEmployeeCaseValueRepository : ICaseValueRepository;

/// <summary>
/// Repository for case values
/// </summary>
public interface ICaseValueRepository : ICaseValueRepository<CaseValue>;