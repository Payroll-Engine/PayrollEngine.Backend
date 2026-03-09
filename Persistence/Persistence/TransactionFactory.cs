using System;
using System.Transactions;

namespace PayrollEngine.Persistence;

public static class TransactionFactory
{
    /// <summary>Transaction timeout, set once during startup via <see cref="Initialize"/>.</summary>
    private static TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Sets the transaction timeout. May only be called once (typically during startup).
    /// </summary>
    /// <param name="timeout">The desired transaction timeout.</param>
    /// <exception cref="InvalidOperationException">Thrown when called more than once.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timeout"/> is zero or negative.</exception>
    public static void Initialize(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Transaction timeout must be positive.");
        }
        if (Timeout.HasValue)
        {
            throw new InvalidOperationException(
                "TransactionFactory.Timeout has already been initialized. " +
                "The timeout can only be set once during application startup.");
        }
        Timeout = timeout;
    }

    /// <summary>Creates a new transaction scope.
    /// Use for orchestrating methods that own the transaction boundary.
    /// see https://www.joshthecoder.com/2020/07/27/transactionscope-considered-annoying.html </summary>
    public static TransactionScope NewTransactionScope()
    {
        var options = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = Timeout ?? TransactionManager.DefaultTimeout
        };
        return new(TransactionScopeOption.Required,
            options, TransactionScopeAsyncFlowOption.Enabled);
    }

    /// <summary>Creates a transaction guard for leaf/base operations.
    /// If an ambient transaction already exists, returns a no-op guard (zero overhead).
    /// If no ambient transaction exists, creates a real <see cref="TransactionScope"/>.
    /// Use for base CRUD methods that may be called standalone or within an existing scope.</summary>
    public static TransactionGuard NewTransactionGuard() =>
        new(Transaction.Current != null ? null : NewTransactionScope());
}