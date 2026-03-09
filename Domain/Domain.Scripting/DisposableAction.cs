using System;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Executes a delegate when disposed; used to implement scope-based cleanup patterns
/// such as push/pop stacks with <c>using</c> blocks.
/// </summary>
internal sealed class DisposableAction : IDisposable
{
    private System.Action action;

    /// <summary>
    /// Creates a new <see cref="DisposableAction"/> that will execute
    /// <paramref name="action"/> exactly once when <see cref="Dispose"/> is called.
    /// </summary>
    /// <param name="action">The cleanup action to run on dispose.</param>
    internal DisposableAction(System.Action action)
    {
        this.action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    /// Executes the cleanup action. Subsequent calls are no-ops.
    /// </summary>
    public void Dispose()
    {
        var a = action;
        action = null;
        a?.Invoke();
    }
}
