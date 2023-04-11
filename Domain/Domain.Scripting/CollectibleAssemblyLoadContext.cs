using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace PayrollEngine.Domain.Scripting;

/// <summary>Assembly memory load context.
/// See https://www.strathweb.com/2019/01/collectible-assemblies-in-net-core-3-0/
/// </summary>
public class CollectibleAssemblyLoadContext : AssemblyLoadContext, IDisposable
{
    /// <inheritdoc />
    public CollectibleAssemblyLoadContext() :
        base(true)
    {
    }

    /// <summary>
    /// Load assembly from binary
    /// </summary>
    /// <param name="binary">The assembly bytes</param>
    /// <returns>The assembly</returns>
    public Assembly LoadFromBinary(byte[] binary)
    {
        using var stream = new MemoryStream(binary);
        return LoadFromStream(stream);
    }

    /// <inheritdoc />
    protected override Assembly Load(AssemblyName assemblyName) => null;

    void IDisposable.Dispose()
    {
        Unload();
    }
}