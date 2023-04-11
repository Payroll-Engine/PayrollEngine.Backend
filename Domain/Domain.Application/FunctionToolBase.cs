using System;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Domain.Application;

public abstract class FunctionToolBase : IDisposable
{
    protected FunctionToolSettings Settings { get; }
    protected FunctionHost FunctionHost { get; }
    private bool disposed;

    protected FunctionToolBase(FunctionToolSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        FunctionHost = new(settings);
    }

    #region Dispose

    /// <inheritdoc />
    /// <summary>
    /// Public implementation of Dispose pattern
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                FunctionHost.Dispose();
            }
            disposed = true;
        }
    }

    #endregion
}