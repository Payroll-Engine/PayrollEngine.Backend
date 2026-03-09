using System;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Factory for creating <see cref="CaseValueCache"/> instances with shared context parameters.
/// Eliminates repeated constructor arguments (dbContext, divisionId, evaluationDate, forecast)
/// when creating global, national, company and employee case value caches.
/// </summary>
internal sealed class CaseValueCacheFactory
{
    private IDbContext DbContext { get; }
    private int DivisionId { get; }
    private DateTime EvaluationDate { get; }
    private string Forecast { get; }

    /// <summary>
    /// Initializes a new factory with the shared context parameters.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="divisionId">The division id for all caches.</param>
    /// <param name="evaluationDate">The evaluation date for all caches.</param>
    /// <param name="forecast">The optional forecast name.</param>
    internal CaseValueCacheFactory(IDbContext dbContext, int divisionId,
        DateTime evaluationDate, string forecast = null)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        DivisionId = divisionId;
        EvaluationDate = evaluationDate;
        Forecast = forecast;
    }

    /// <summary>
    /// Creates a new <see cref="CaseValueCache"/> for the given repository and parent id.
    /// </summary>
    /// <param name="repository">The case value repository (global, national, company, or employee).</param>
    /// <param name="parentId">The parent entity id (tenant id for global/national/company, employee id for employee).</param>
    internal CaseValueCache Create(ICaseValueRepository repository, int parentId) =>
        new(DbContext, repository, parentId, DivisionId, EvaluationDate, Forecast);
}
