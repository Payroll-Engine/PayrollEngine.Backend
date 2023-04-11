using System;
using System.Transactions;

namespace PayrollEngine.Persistence;

public static class TransactionFactory
{
    public static TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>Creates a new transaction scope
    /// see https://www.joshthecoder.com/2020/07/27/transactionscope-considered-annoying.html </summary>
    public static TransactionScope NewTransactionScope()
    {
        var options = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = Timeout
        };
        return new(TransactionScopeOption.Required,
            options, TransactionScopeAsyncFlowOption.Enabled);
    }
}