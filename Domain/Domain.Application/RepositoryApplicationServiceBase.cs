using System;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public abstract class RepositoryApplicationServiceBase<TRepo> : IRepositoryApplicationService<TRepo>
    where TRepo : class, IDomainRepository
{
    public TRepo Repository { get; }

    protected RepositoryApplicationServiceBase(TRepo repository)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public virtual async Task<bool> ExistsAsync(int itemId) =>
        await Repository.ExistsAsync(itemId);

    #region Attributes

    public virtual async Task<string> GetAttributeAsync(int id, string attributeName) =>
        await Repository.GetAttributeAsync(id, attributeName);

    public virtual async Task<bool> ExistsAttributeAsync(int id, string attributeName) =>
        await Repository.ExistsAttributeAsync(id, attributeName);

    public virtual async Task<string> SetAttributeAsync(int id, string attributeName, string value) =>
        await Repository.SetAttributeAsync(id, attributeName, value);

    public virtual async Task<bool?> DeleteAttributeAsync(int id, string attributeName) =>
        await Repository.DeleteAttributeAsync(id, attributeName);

    #endregion
}