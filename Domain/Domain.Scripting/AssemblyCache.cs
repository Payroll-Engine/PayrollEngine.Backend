//#define ASSEMBLY_GET
//#define ASSEMBLY_CACHE
//#define ASSEMBLY_LOAD

#if ASSEMBLY_LOAD
#define LOG_STOPWATCH
#endif

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using PayrollEngine.Domain.Model;
using Timer = System.Timers.Timer;
using System.Collections.Concurrent;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Cache of script assemblies keyed by tenant, object type and script hash.
/// The cache is necessary because collectible AssemblyLoadContexts produce
/// memory leaks when created without reuse.
///
/// Tenant isolation is enforced at two levels:
///   1. Cache key  – tenant id is part of every lookup key, so a hash
///      collision across tenants can never return a foreign assembly.
///   2. Load context – every tenant owns a dedicated
///      <see cref="TenantAssemblyLoadContext"/> so the CLR type-resolver
///      never bridges tenant boundaries.
/// </summary>
public class AssemblyCache
{
    // -------------------------------------------------------------------------
    // Cache key
    // -------------------------------------------------------------------------

    /// <summary>
    /// Composite cache key: tenant id + CLR type + script hash.
    /// Including the tenant id prevents cross-tenant cache hits even when
    /// two tenants happen to produce the same script hash for the same type.
    /// </summary>
    private sealed class AssemblyKey : Tuple<int, Type, int>
    {
        /// <param name="tenantId">Identifier of the owning tenant.</param>
        /// <param name="type">CLR type of the scripted domain object.</param>
        /// <param name="scriptHash">Hash of the compiled script binary.</param>
        internal AssemblyKey(int tenantId, Type type, int scriptHash)
            : base(tenantId, type, scriptHash)
        {
            if (tenantId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tenantId));
            }
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

    // -------------------------------------------------------------------------
    // Per-tenant load context
    // -------------------------------------------------------------------------

    /// <summary>
    /// Dedicated <see cref="CollectibleAssemblyLoadContext"/> for a single
    /// tenant.  Keeping one context per tenant ensures that the CLR type
    /// resolver never resolves types across tenant boundaries and that
    /// unloading one tenant's scripts does not affect other tenants.
    /// </summary>
    private sealed class TenantAssemblyLoadContext : IDisposable
    {
        private readonly CollectibleAssemblyLoadContext loadContext = new();
        private bool disposed;

        /// <summary>Loads an assembly from a raw binary image.</summary>
        internal Assembly LoadFromBinary(byte[] binary) =>
            loadContext.LoadFromBinary(binary);

        /// <summary>
        /// Unloads the underlying collectible context, releasing all
        /// assemblies that belong to this tenant.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            loadContext.Unload();
        }
    }

    // -------------------------------------------------------------------------
    // Cached entry
    // -------------------------------------------------------------------------

    /// <summary>
    /// Wraps a loaded assembly together with its owning load context and
    /// last-used timestamp for LRU eviction.
    /// </summary>
    private sealed class AssemblyRuntime
    {
        internal Assembly Assembly { get; }
        internal DateTime LastUsed { get; set; } = Date.Now;

        internal AssemblyRuntime(Assembly assembly)
        {
            Assembly = assembly;
        }
    }

    // -------------------------------------------------------------------------
    // Static shared state  (process-wide, but tenant-isolated via keys/contexts)
    // -------------------------------------------------------------------------

#if ASSEMBLY_CACHE
    private readonly CacheRatio cacheRatio = new("Script assembly cache");
#endif

    /// <summary>
    /// Process-wide assembly cache.  Thread-safe.
    /// Keys always include the tenant id, so entries are never shared across
    /// tenant boundaries.
    /// </summary>
    private static readonly ConcurrentDictionary<AssemblyKey, AssemblyRuntime> Assemblies = new();

    /// <summary>
    /// One load context per tenant, kept alive for the lifetime of the cached
    /// assemblies.  Disposing a tenant context unloads all its assemblies.
    /// </summary>
    private static readonly ConcurrentDictionary<int, TenantAssemblyLoadContext> TenantContexts = new();

    /// <summary>
    /// Per-tenant semaphore used to serialize assembly loading and prevent two
    /// concurrent threads from loading the same binary into the same
    /// <see cref="TenantAssemblyLoadContext"/>, which would throw
    /// "Assembly with same name is already loaded".
    /// </summary>
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> TenantLoadLocks = new();

    /// <summary>
    /// Holder for the shared eviction timer. The outer field is
    /// <c>static readonly</c> (satisfies NDepend ND1901); the inner
    /// timer is set exactly once via <see cref="TryInitialize"/>.
    /// </summary>
    private sealed class TimerHolder
    {
        private Timer instance;

        /// <summary>Returns <c>true</c> when the timer has been initialized.</summary>
        internal bool IsInitialized => instance != null;

        /// <summary>
        /// Attempts to set the timer exactly once. Returns <c>true</c> if this
        /// call won the race; <c>false</c> if another thread already initialized.
        /// The losing timer is disposed by the caller.
        /// </summary>
        internal bool TryInitialize(Timer timer)
        {
            return Interlocked.CompareExchange(ref instance, timer, null) == null;
        }

        /// <summary>Returns the initialized timer instance.</summary>
        internal Timer Instance => instance;
    }

    private static readonly TimerHolder TimerState = new();

    /// <summary>Sliding window after which an unused assembly is evicted.</summary>
    private static TimeSpan? CacheTimeout { get; set; }

    /// <summary>Returns <c>true</c> when the cache is active.</summary>
    private static bool CacheEnabled =>
        CacheTimeout.HasValue && CacheTimeout.Value != TimeSpan.Zero;

    // -------------------------------------------------------------------------
    // Instance state
    // -------------------------------------------------------------------------

    /// <summary>Database context used to fetch script binaries on cache miss.</summary>
    private IDbContext DbContext { get; }

    /// <summary>Optional provider for resolving script binaries from the database.</summary>
    private IScriptProvider ScriptProvider { get; }

    // -------------------------------------------------------------------------
    // Construction
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="AssemblyCache"/>.
    /// </summary>
    /// <param name="dbContext">Database context (required).</param>
    /// <param name="cacheTimeout">
    /// Sliding eviction window.  Pass <see cref="TimeSpan.Zero"/> to disable
    /// caching entirely (useful in tests or single-tenant dev setups).
    /// </param>
    /// <param name="scriptProvider">
    /// Optional provider that resolves script binaries from the database when
    /// the <see cref="IScriptObject"/> does not carry an in-memory binary.
    /// </param>
    public AssemblyCache(IDbContext dbContext, TimeSpan cacheTimeout, IScriptProvider scriptProvider = null)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        // Thread-safe one-time timer initialization.
        // Interlocked.CompareExchange ensures only one thread wins the race;
        // the loser disposes its timer instance immediately.
        if (cacheTimeout != TimeSpan.Zero && !TimerState.IsInitialized)
        {
            CacheTimeout = cacheTimeout;
            var timer = new Timer(cacheTimeout.TotalMilliseconds / 4);
            if (TimerState.TryInitialize(timer))
            {
                TimerState.Instance.Elapsed += delegate { CacheUpdate(); };
                TimerState.Instance.Start();
            }
            else
            {
                // Another thread already initialized the timer – discard ours.
                timer.Dispose();
            }
        }

        ScriptProvider = scriptProvider;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the compiled assembly for the given scripted domain object,
    /// loading and caching it on first access.
    /// </summary>
    /// <param name="tenantId">
    /// Identifier of the tenant that owns <paramref name="scriptObject"/>.
    /// Used as the first segment of the cache key to enforce isolation.
    /// </param>
    /// <param name="type">CLR type of the domain object.</param>
    /// <param name="scriptObject">The scripted domain object.</param>
    /// <returns>The loaded <see cref="Assembly"/>.</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when no binary can be resolved for <paramref name="scriptObject"/>.
    /// </exception>
    public Assembly GetObjectAssembly(int tenantId, Type type, IScriptObject scriptObject)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenantId));
        }
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }
        if (scriptObject == null)
        {
            throw new ArgumentNullException(nameof(scriptObject));
        }

        var key = new AssemblyKey(tenantId, type, scriptObject.ScriptHash);

        // --- cache lookup ---------------------------------------------------
        if (CacheEnabled && Assemblies.TryGetValue(key, out var cached))
        {
#if ASSEMBLY_CACHE
            cacheRatio.UpdateWithLog(hit: true);
#endif
            cached.LastUsed = Date.Now;
            return cached.Assembly;
        }

#if ASSEMBLY_CACHE
        cacheRatio.UpdateWithLog(hit: false);
#endif

        // --- cache miss: resolve binary -------------------------------------
        var binary = scriptObject.Binary;
        if (binary == null && ScriptProvider != null)
        {
#if ASSEMBLY_GET
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            binary = ScriptProvider.GetBinaryAsync(DbContext, scriptObject).GetAwaiter().GetResult();
#if ASSEMBLY_GET
            sw.Stop();
            Log.Information($"Assembly load {type.Name} [{scriptObject.Id}]: {sw.ElapsedMilliseconds} ms");
#endif
        }

        if (binary == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(scriptObject),
                $"Script object without binary: {type.Namespace}, id={scriptObject.Id}.");
        }

        // --- load into the tenant-scoped context ----------------------------
        // Serialize loading per tenant: two concurrent threads racing on the
        // same cache miss would both call LoadFromBinary with the same assembly
        // name into the same TenantAssemblyLoadContext, causing the CLR to
        // throw "Assembly with same name is already loaded".
        var tenantLock = TenantLoadLocks.GetOrAdd(tenantId, _ => new SemaphoreSlim(1, 1));
        tenantLock.Wait();
        try
        {
            // Double-check: another thread may have loaded while we were waiting.
            if (CacheEnabled && Assemblies.TryGetValue(key, out cached))
            {
#if ASSEMBLY_CACHE
                cacheRatio.UpdateWithLog(hit: true);
#endif
                cached.LastUsed = Date.Now;
                return cached.Assembly;
            }

            LogStopwatch.Start(nameof(GetObjectAssembly));

            Assembly assembly;
            if (CacheEnabled)
            {
                // Retrieve or create the dedicated load context for this tenant.
                // Using a per-tenant context means the CLR type resolver is scoped
                // to that tenant; assemblies from different tenants can never see
                // each other's types even if they share the same AppDomain.
                var tenantContext = TenantContexts.GetOrAdd(tenantId, _ => new TenantAssemblyLoadContext());
                try
                {
                    assembly = tenantContext.LoadFromBinary(binary);
                    Assemblies.TryAdd(key, new AssemblyRuntime(assembly));
                }
                catch (Exception ex)
                {
                    // The tenant load context is in an undefined state (e.g. the same
                    // assembly name was already loaded into it after a ScriptPublish
                    // without a backend restart). Evict the broken context so the next
                    // request gets a fresh one and can recover automatically.
                    CacheClearTenant(tenantId);
                    throw new PayrollException(
                        $"Failed to load script assembly for tenant {tenantId}, type {type.Name}. " +
                        $"The tenant assembly cache has been cleared — retry the operation. " +
                        $"({ex.GetBaseMessage()})", ex);
                }
            }
            else
            {
                // Cache is disabled: use a fresh, per-invocation load context so
                // that repeated calls never attempt to load the same assembly name
                // into a shared context. The context is kept alive by the assembly
                // reference and collected once the assembly is no longer in use.
                var freshContext = new TenantAssemblyLoadContext();
                assembly = freshContext.LoadFromBinary(binary);
            }

            LogStopwatch.Stop(nameof(GetObjectAssembly));

            return assembly;
        }
        finally
        {
            tenantLock.Release();
        }
    }

    // -------------------------------------------------------------------------
    // Cache management
    // -------------------------------------------------------------------------

    /// <summary>
    /// Removes all cached assemblies and disposes all tenant load contexts,
    /// effectively unloading every tenant script from the process.
    /// </summary>
    public static void CacheClear()
    {
        if (!Assemblies.Any())
        {
            Log.Information("Assembly cache is empty – nothing to clear.");
            return;
        }

        var count = Assemblies.Count;
        Assemblies.Clear();
        DisposeTenantContexts();
        Log.Information($"Assembly cache cleared ({count} assemblies, all tenant contexts unloaded).");
    }

    /// <summary>
    /// Removes all cached assemblies that belong to a specific tenant and
    /// disposes the corresponding load context.  Call this when a tenant is
    /// deprovisioned or when its scripts are redeployed.
    /// </summary>
    /// <param name="tenantId">Tenant whose assemblies should be evicted.</param>
    public static void CacheClearTenant(int tenantId)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenantId));
        }

        var removed = 0;
        foreach (var key in Assemblies.Keys.Where(k => k.Item1 == tenantId).ToList())
        {
            if (Assemblies.TryRemove(key, out _))
            {
                removed++;
            }
        }
        if (TenantContexts.TryRemove(tenantId, out var context))
        {
            context.Dispose();
        }

        Log.Information($"Tenant {tenantId}: evicted {removed} assemblies and unloaded load context.");
    }

    /// <summary>
    /// Timer callback – evicts assemblies that have not been used within the
    /// configured <see cref="CacheTimeout"/> window and disposes orphaned
    /// tenant load contexts.
    /// </summary>
    private static void CacheUpdate()
    {
        if (!CacheTimeout.HasValue || !Assemblies.Any())
        {
            return;
        }

        var threshold = Date.Now.Subtract(CacheTimeout.Value);
        var expired = Assemblies.Where(x => x.Value.LastUsed < threshold).ToList();

        if (!expired.Any())
        {
            return;
        }

        var removed = 0;
        foreach (var entry in expired)
        {
            try
            {
                // re-check: the assembly may have been used since the snapshot
                if (entry.Value.LastUsed >= threshold)
                    continue;

                if (Assemblies.TryRemove(entry.Key, out _))
                {
                    removed++;
                }
            }
            catch (Exception exception)
            {
                Log.Error($"Error removing assembly from cache: {exception.GetBaseMessage()}", exception);
            }
        }

        Log.Information($"Cache update: removed {removed} expired assemblies (threshold={threshold:O}).");
    }

    /// <summary>Disposes and removes all tenant load contexts.</summary>
    private static void DisposeTenantContexts()
    {
        foreach (var tenantId in TenantContexts.Keys.ToList())
        {
            if (TenantContexts.TryRemove(tenantId, out var context))
            {
                context.Dispose();
            }
        }
    }
}
