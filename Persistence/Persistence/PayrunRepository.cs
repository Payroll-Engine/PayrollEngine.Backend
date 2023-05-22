using System;
using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting.Controller;
using PayrollEngine.Serialization;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public class PayrunRepository : ScriptChildDomainRepository<Payrun, Payrun>, IPayrunRepository
{
    public PayrunRepository(IPayrunScriptController scriptController,
        IScriptRepository scriptRepository) :
        base(DbSchema.Tables.Payrun, DbSchema.PayrunColumn.TenantId, scriptController, scriptRepository)
    {
    }

    // duplicated in ScriptTrackChildDomainRepository!
    public virtual async Task RebuildAsync(IDbContext context, int tenantId, int payrunId)
    {
        if (tenantId == default)
        {
            throw new ArgumentException(nameof(tenantId));
        }
        if (payrunId == default)
        {
            throw new ArgumentNullException(nameof(payrunId));
        }

        // read item
        var payrun = await GetAsync(context, tenantId, payrunId);
        if (payrun == null)
        {
            throw new PayrollException($"Unknown payrun with id {payrunId}");
        }
        var scriptObject = (IScriptObject)payrun;
        if (scriptObject == null)
        {
            throw new PayrollException($"Invalid payrun with id {payrunId}");
        }

        // create transaction
        using var txScope = TransactionFactory.NewTransactionScope();

        // rebuild script binary
        await SetupBinaryAsync(context, tenantId, payrun);

        // update item
        await UpdateAsync(context, tenantId, payrun);

        // commit transaction
        txScope.Complete();
    }

    protected override void GetObjectCreateData(Payrun payrun, DbParameterCollection parameters)
    {
        parameters.Add(nameof(payrun.Name), payrun.Name);
        base.GetObjectCreateData(payrun, parameters);
    }

    protected override void GetObjectData(Payrun payrun, DbParameterCollection parameters)
    {
        parameters.Add(nameof(payrun.PayrollId), payrun.PayrollId);
        parameters.Add(nameof(payrun.NameLocalizations), JsonSerializer.SerializeNamedDictionary(payrun.NameLocalizations));
        parameters.Add(nameof(payrun.DefaultReason), payrun.DefaultReason);
        parameters.Add(nameof(payrun.DefaultReasonLocalizations), JsonSerializer.SerializeNamedDictionary(payrun.DefaultReasonLocalizations));
        parameters.Add(nameof(payrun.StartExpression), payrun.StartExpression);
        parameters.Add(nameof(payrun.EmployeeAvailableExpression), payrun.EmployeeAvailableExpression);
        parameters.Add(nameof(payrun.EmployeeStartExpression), payrun.EmployeeStartExpression);
        parameters.Add(nameof(payrun.EmployeeEndExpression), payrun.EmployeeEndExpression);
        parameters.Add(nameof(payrun.WageTypeAvailableExpression), payrun.WageTypeAvailableExpression);
        parameters.Add(nameof(payrun.EndExpression), payrun.EndExpression);
        parameters.Add(nameof(payrun.RetroTimeType), payrun.RetroTimeType);
        parameters.Add(nameof(payrun.Calendar), payrun.Calendar);
        parameters.Add(nameof(payrun.Script), payrun.Script);
        parameters.Add(nameof(payrun.ScriptVersion), payrun.ScriptVersion);
        parameters.Add(nameof(payrun.Binary), payrun.Binary, DbType.Binary);
        parameters.Add(nameof(payrun.ScriptHash), payrun.ScriptHash);
        base.GetObjectData(payrun, parameters);
    }
}