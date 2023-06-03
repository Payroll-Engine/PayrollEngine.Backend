using System.Text;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class UserRepository : ChildDomainRepository<User>, IUserRepository
{
    public UserRepository() :
        base(DbSchema.Tables.User, DbSchema.UserColumn.TenantId)
    {
    }

    public virtual async Task<bool> ExistsAnyAsync(IDbContext context, int tenantId, string identifier) =>
        await ExistsAnyAsync(context, DbSchema.UserColumn.TenantId, tenantId, DbSchema.UserColumn.Identifier, identifier);

    protected override void GetObjectCreateData(User user, DbParameterCollection parameters)
    {
        parameters.Add(nameof(user.Identifier), user.Identifier);
        base.GetObjectCreateData(user, parameters);
    }

    protected override void GetObjectData(User user, DbParameterCollection parameters)
    {
        parameters.Add(nameof(user.FirstName), user.FirstName);
        parameters.Add(nameof(user.LastName), user.LastName);
        parameters.Add(nameof(user.Culture), user.Culture);
        parameters.Add(nameof(user.Language), user.Language);
        parameters.Add(nameof(user.UserType), user.UserType);
        parameters.Add(nameof(user.Attributes), JsonSerializer.SerializeNamedDictionary(user.Attributes));
        base.GetObjectCreateData(user, parameters);
    }

    /// <inheritdoc />
    public virtual async System.Threading.Tasks.Task UpdatePasswordAsync(IDbContext context, int tenantId, int userId, string password)
    {
        var user = await GetAsync(context, tenantId, userId);
        if (user == null)
        {
            throw new PayrollException($"Unknown user with id {userId}");
        }

        // password reset
        if (password == null)
        {
            user.Password = null;
            await UpdateAsync(context, tenantId, user);
            return;
        }

        // setup password
        var hashSalt = password.ToHashSalt();
        user.Password = hashSalt.Hash;
        user.StoredSalt = hashSalt.Salt;

        // update user password
        user.Updated = Date.Now;
        var parameters = new DbParameterCollection();
        parameters.Add(nameof(user.Updated), user.Updated);
        parameters.Add(nameof(user.Password), user.Password);
        parameters.Add(nameof(user.StoredSalt), user.StoredSalt);
        var queryBuilder = new StringBuilder();
        queryBuilder.AppendDbUpdate(TableName, parameters.GetNames(), userId);
        var sql = queryBuilder.ToString();

        // transaction
        using var txScope = TransactionFactory.NewTransactionScope();
        // sql update
        await ExecuteAsync(context, sql, parameters);
        txScope.Complete();
    }
}