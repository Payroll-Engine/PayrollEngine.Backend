//#define ASSEMBLY_GET
//#define ASSEMBLY_CACHE
//#define ASSEMBLY_LOAD
#if ASSEMBLY_LOAD
#define LOG_STOPWATCH
#endif
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Timers;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Represents a cache of script assemblies by the object type and id.
/// The cache is necessary because the collectible AssemblyLoadContext
/// produces memory leaks.
/// </summary>
public class AssemblyCache
{
    /// <summary>
    /// key fo the assembly cache by the clr type and the script hash code
    /// </summary>
    private sealed class AssemblyKey : Tuple<Type, int>
    {
        internal AssemblyKey(Type type, int scriptHash) :
            base(type, scriptHash)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (scriptHash == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scriptHash));
            }
        }
    }

    /// <summary>
    /// assembly including his loader to unload the assembly
    /// Use the binary hash code, to detect changes binary changes.
    /// An audit object has the same id as the tracking object, but may have a different binary
    /// </summary>
    private sealed class AssemblyRuntime
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        // ReSharper disable once MemberCanBePrivate.Local
        private CollectibleAssemblyLoadContext LoadContext { get; }
        internal Assembly Assembly { get; }
        internal DateTime LastUsed { get; set; } = Date.Now;

        internal AssemblyRuntime(CollectibleAssemblyLoadContext context, Assembly assembly)
        {
            LoadContext = context;
            Assembly = assembly;
        }
    }

#if ASSEMBLY_CACHE
        private readonly CacheRatio cacheRatio = new("Script assembly cache");
#endif

    // thread-safe assembly cache
    private static readonly ConcurrentDictionary<AssemblyKey, AssemblyRuntime> Assemblies =
        new();

    private static Timer UpdateTimer { get; set; }

    /// <summary>
    /// The cache timeout
    /// </summary>
    public static TimeSpan? CacheTimeout { get; private set; }

    /// <summary>
    /// Enable disable the cache
    /// </summary>
    public static bool CacheEnabled => CacheTimeout.HasValue &&
                                       CacheTimeout.Value != TimeSpan.Zero;

    /// <summary>
    /// The script provider
    /// </summary>
    public IDbContext DbContext { get; set; }

    /// <summary>
    /// The script provider
    /// </summary>
    public IScriptProvider ScriptProvider { get; set; }

    /// <summary>
    /// Assembly cache ctor
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="cacheTimeout">The cache timeout, use <see cref="TimeSpan.Zero"/> to disable the cache</param>
    /// <param name="scriptProvider">The script provider</param>
    public AssemblyCache(IDbContext dbContext, TimeSpan cacheTimeout, IScriptProvider scriptProvider = null)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        // initialize cleanup timer
        if (cacheTimeout != TimeSpan.Zero && UpdateTimer == null)
        {
            CacheTimeout = cacheTimeout;
            UpdateTimer = new(CacheTimeout.Value.TotalMilliseconds);
            UpdateTimer.Elapsed += delegate { CacheUpdate(); };
            UpdateTimer.Start();
        }
        ScriptProvider = scriptProvider;
    }

    /// <summary>
    /// Get object script assembly
    /// </summary>
    /// <param name="type">The object type</param>
    /// <param name="scriptObject">The scripting object</param>
    /// <returns>The assembly</returns>
    public Assembly GetObjectAssembly(Type type, IScriptObject scriptObject)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var key = new AssemblyKey(type, scriptObject.ScriptHash);
        Assembly assembly = null;
        if (CacheEnabled)
        {
            var cached = Assemblies.TryGetValue(key, out var assemblyRuntime);
#if ASSEMBLY_CACHE
                cacheRatio.UpdateWithLog(cached);
#endif
            if (cached)
            {
                assembly = assemblyRuntime.Assembly;
                // update lst usage, used to cleanup outdated assemblies
                assemblyRuntime.LastUsed = Date.Now;
            }
        }

        if (assembly == null)
        {
            var binary = scriptObject.Binary;
            if (binary == null && ScriptProvider != null)
            {
#if ASSEMBLY_GET
                    var stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
#endif
                binary = ScriptProvider.GetBinaryAsync(DbContext, scriptObject).Result;
#if ASSEMBLY_GET
                    stopwatch.Stop();
                    Log.Information($"Assembly load {type.Name} [{scriptObject.Id}]: {stopwatch.ElapsedMilliseconds} ms");
#endif
            }
            if (binary == null)
            {
                throw new ArgumentOutOfRangeException(nameof(scriptObject), $"Script object without binary {type.Namespace} with id {scriptObject.Id}");
            }

            LogStopwatch.Start(nameof(GetObjectAssembly));
            // load assembly from binary
            using var loadContext = new CollectibleAssemblyLoadContext();
            assembly = loadContext.LoadFromBinary(binary);
            LogStopwatch.Stop(nameof(GetObjectAssembly));

            // if add fails, we will try the next time to add
            if (CacheEnabled)
            {
                Assemblies.TryAdd(key, new(loadContext, assembly));
            }
        }
        return assembly;
    }

    /// <summary>
    /// Clears the assembly cache
    /// </summary>
    public static void CacheClear()
    {
        if (!Assemblies.Any())
        {
            Log.Information("Empty assembly cache");
            return;
        }

        var count = Assemblies.Count;
        Assemblies.Clear();
        Log.Information($"Assembly cache successfully cleared ({count} assemblies)");
    }

    private static void CacheUpdate()
    {
        if (!CacheTimeout.HasValue || !Assemblies.Any())
        {
            return;
        }

        var outdatedDate = Date.Now.Subtract(CacheTimeout.Value);
        var assemblies = Assemblies.Where(x => x.Value.LastUsed < outdatedDate).ToList();
        if (assemblies.Any())
        {
            var removed = 0;
            foreach (var assembly in assemblies)
            {
                try
                {
                    if (Assemblies.TryRemove(assembly.Key, out _))
                    {
                        removed++;
                        Log.Trace($"Removed {assembly.Value.Assembly.GetName().Name} outdated assemblies");
                    }
                }
                catch (Exception exception)
                {
                    Log.Error($"Error removing assembly: {exception.GetBaseMessage()}", exception);
                }
            }
            Log.Information($"Removed {removed} assemblies, outdated since {outdatedDate}");
        }
    }
}