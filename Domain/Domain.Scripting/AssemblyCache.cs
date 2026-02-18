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

    private static Timer UpdateTimer { get; set; }

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

        if (cacheTimeout != TimeSpan.Zero && UpdateTimer == null)
        {
            CacheTimeout = cacheTimeout;
            UpdateTimer = new(CacheTimeout.Value.TotalMilliseconds);
            UpdateTimer.Elapsed += delegate { CacheUpdate(); };
            UpdateTimer.Start();
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
            binary = ScriptProvider.GetBinaryAsync(DbContext, scriptObject).Result;
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
        LogStopwatch.Start(nameof(GetObjectAssembly));

        // Retrieve or create the dedicated load context for this tenant.
        // Using a per-tenant context means the CLR type resolver is scoped to
        // that tenant; assemblies from different tenants can never see each
        // other's types even if they share the same AppDomain.
        var tenantContext = TenantContexts.GetOrAdd(tenantId, _ => new TenantAssemblyLoadContext());
        var assembly = tenantContext.LoadFromBinary(binary);

        LogStopwatch.Stop(nameof(GetObjectAssembly));

        if (CacheEnabled)
        {
            // TryAdd is intentionally non-blocking; a concurrent thread may
            // win the race, and we simply discard our copy on the next call.
            Assemblies.TryAdd(key, new AssemblyRuntime(assembly));
        }

        return assembly;
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

        // dispose load contexts for tenants that no longer have any cached entry.
        var activeTenants = Assemblies.Keys.Select(k => k.Item1).ToHashSet();
        foreach (var tenantId in TenantContexts.Keys.Except(activeTenants).ToList())
        {
            if (TenantContexts.TryRemove(tenantId, out var context))
            {
                context.Dispose();
                Log.Trace($"Disposed load context for tenant {tenantId} (no remaining cached assemblies).");
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