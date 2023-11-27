using System;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public abstract class RepositoryApplicationServiceBase<TRepo>(TRepo repository) : IRepositoryApplicationService<TRepo>
    where TRepo : class, IDomainRepository
{
    public TRepo Repository { get; } = repository ?? throw new ArgumentNullException(nameof(repository));

    public virtual async Task<bool> ExistsAsync(IDbContext context, int itemId) =>
        await Repository.ExistsAsync(context, itemId);

    #region Attributes

    public virtual async Task<string> GetAttributeAsync(IDbContext context, int id, string attributeName) =>
        await Repository.GetAttributeAsync(context, id, attributeName);

    public virtual async Task<bool> ExistsAttributeAsync(IDbContext context, int id, string attributeName) =>
        await Repository.ExistsAttributeAsync(context, id, attributeName);

    public virtual async Task<string> SetAttributeAsync(IDbContext context, int id, string attributeName, string value) =>
        await Repository.SetAttributeAsync(context, id, attributeName, value);

    public virtual async Task<bool?> DeleteAttributeAsync(IDbContext context, int id, string attributeName) =>
        await Repository.DeleteAttributeAsync(context, id, attributeName);

    #endregion
}