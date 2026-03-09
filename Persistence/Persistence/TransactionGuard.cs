using System;
using System.Transactions;

namespace PayrollEngine.Persistence;

/// <summary>Lightweight transaction wrapper that is either a real <see cref="TransactionScope"/> or a no-op.
/// Designed as a struct to avoid heap allocation in the no-op case.</summary>
public readonly record struct TransactionGuard(TransactionScope scope) : IDisposable
{
    /// <summary>Marks the transaction as successfully completed (no-op if no scope is owned).</summary>
    public void Complete() => scope?.Complete();

    /// <summary>Disposes the underlying scope (no-op if no scope is owned).</summary>
    public void Dispose() => scope?.Dispose();
}