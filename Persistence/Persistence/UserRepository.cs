using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class UserRepository() : ChildDomainRepository<User>(DbSchema.Tables.User, DbSchema.UserColumn.TenantId),
    IUserRepository
{
    public async Task<bool> ExistsAnyAsync(IDbContext context, int tenantId, string identifier) =>
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
        parameters.Add(nameof(user.UserType), user.UserType, DbType.Int32);
        parameters.Add(nameof(user.Attributes), JsonSerializer.SerializeNamedDictionary(user.Attributes));
        base.GetObjectData(user, parameters);
    }

    /// <inheritdoc />
    public async Task<bool> TestPasswordAsync(IDbContext context, int tenantId, int userId, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(nameof(password));
        }
        if (!UserPassword.IsValid(password))
        {
            return false;
        }

        var user = await GetAsync(context, tenantId, userId);
        if (user == null)
        {
            throw new PayrollException($"Unknown user with id {userId}.");
        }

        // compare password
        return VerifyPassword(user, password);
    }

    /// <inheritdoc />
    public async System.Threading.Tasks.Task UpdatePasswordAsync(IDbContext context, int tenantId, int userId, PasswordChangeRequest changeRequest)
    {
        if (changeRequest == null)
        {
            throw new ArgumentNullException(nameof(changeRequest));
        }
        if (!UserPassword.IsValid(changeRequest.NewPassword))
        {
            throw new PayrollException("Invalid new user password.");
        }

        var user = await GetAsync(context, tenantId, userId);
        if (user == null)
        {
            throw new PayrollException($"Unknown user with id {userId}.");
        }

        // existing password
        if (!string.IsNullOrWhiteSpace(user.Password))
        {
            // test existing password
            if (string.IsNullOrWhiteSpace(changeRequest.ExistingPassword))
            {
                throw new PayrollException("Missing existing password.");
            }

            // validate existing password
            if (!VerifyPassword(user, changeRequest.ExistingPassword))
            {
                throw new PayrollException("Invalid password.");
            }

            // password reset
            if (string.IsNullOrWhiteSpace(changeRequest.NewPassword))
            {
                user.Password = null;
                await UpdateAsync(context, tenantId, user);
                return;
            }
        }

        // password init or change
        if (string.IsNullOrWhiteSpace(changeRequest.NewPassword))
        {
            return;
        }
        var hashSalt = changeRequest.NewPassword.ToHashSalt();
        user.Password = hashSalt.Hash;
        user.StoredSalt = hashSalt.Salt;

        // update user password
        user.Updated = Date.Now;
        var parameters = new DbParameterCollection();
        parameters.Add(nameof(user.Updated), user.Updated, DbType.DateTime2);
        parameters.Add(nameof(user.Password), user.Password);
        parameters.Add(nameof(user.StoredSalt), user.StoredSalt, DbType.Binary);
        var queryBuilder = new StringBuilder();
        queryBuilder.AppendDbUpdate(TableName, parameters.GetNames(), userId);
        var sql = queryBuilder.ToString();

        // transaction
        using var txScope = TransactionFactory.NewTransactionScope();
        // sql update
        await ExecuteAsync(context, sql, parameters);
        txScope.Complete();
    }

    private static bool VerifyPassword(User user, string verifyPassword) =>
        UserPassword.VerifyPassword(user.Password, user.StoredSalt, verifyPassword);
}