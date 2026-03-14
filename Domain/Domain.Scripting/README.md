# PayrollEngine.Domain.Scripting

Runtime scripting layer for the Payroll Engine backend. This assembly bridges the gap between user-authored C# expressions stored in the database and live .NET execution: it compiles script source code on demand, loads the resulting assembly into an isolated load context, caches it for reuse, and exposes a typed runtime API to every function type supported by the engine.

---

## Responsibilities

| Concern | Components |
|---|---|
| Script compilation | `CSharpCompiler`, `ScriptCompiler`, `ScriptSafetyAnalyzer` |
| Assembly loading & caching | `AssemblyCache`, `CollectibleAssemblyLoadContext` |
| Runtime execution | `FunctionHost`, `RuntimeBase` and all `Runtime/*` subclasses |
| Case value resolution | `CaseValueProvider`, `CaseValueProviderCalculation`, `CaseValueProviderSettings` |
| Script controller dispatch | `ScriptControllerBase`, `IScriptController`, `Controller/*` |
| No-Code action parsing | `Action/*`, `ActionParser`, `ActionReflector` |
| Code generation helpers | `CodeFactory` |

---

## Compilation pipeline

When a script object (wage type, case, collector, report, …) is executed for the first time, the following steps take place:

```
IScriptObject  (expression strings + action definitions, stored in DB)
      │
      ▼
ScriptCompiler
  ├─ BuildObjectCodes()      embedded runtime C# files from Client.Scripting
  ├─ BuildActionResults()    No-Code action expressions → ActionParser → ActionResult[]
  └─ BuildFunctionCodes()    inject user expression into FunctionType template
      │
      ▼
CSharpCompiler.CompileAssembly()
  ├─ Roslyn CSharpCompilation (Release, DynamicallyLinkedLibrary)
  ├─ Assembly name  =  "{tenantId}_{scriptHash}_{typeFullName}"
  ├─ ScriptSafetyAnalyzer   (optional; rejects banned APIs before emit)
  └─ MemoryStream  →  byte[]  (ScriptCompileResult.Binary)
      │
      ▼
AssemblyCache.GetObjectAssembly()
      │
      ▼
CollectibleAssemblyLoadContext.LoadFromBinary()
      │
      ▼
Loaded Assembly  →  RuntimeBase.CreateScript<T>()  →  script.Execute()
```

The compiled binary is stored on the `IScriptObject` (column `Binary`) so subsequent calls go directly to the cache without recompiling.

---

## AssemblyCache

The central component for assembly lifecycle management. Because `CollectibleAssemblyLoadContext` instances accumulate memory when created without reuse, the cache keeps one load context per tenant alive for the entire cache window, while still allowing full unloading when scripts are redeployed.

### Cache key

Every entry is keyed by a `(tenantId, CLR type, scriptHash)` triple:

- **tenantId** — prevents cross-tenant cache hits even when two tenants produce identical script hashes.
- **CLR type** — distinguishes between a `WageType` and a `Case` that happen to share the same hash.
- **scriptHash** — computed from the compiled binary (`IScriptObject.ScriptHash`). A new `ScriptPublish` produces a new hash and therefore a new cache entry, leaving the old one to expire naturally.

### Tenant isolation (two layers)

1. **Cache key** — tenant id is always part of the lookup; a hash collision across tenants can never return a foreign assembly.
2. **Load context** — every tenant owns a dedicated `TenantAssemblyLoadContext`. The CLR type resolver is scoped to that context, so types from tenant A are invisible to tenant B even within the same AppDomain.

### Thread safety

Loading is serialized per tenant via a `SemaphoreSlim(1,1)` stored in `TenantLoadLocks`. The pattern used is double-checked locking:

```
tenantLock.Wait()
  → double-check cache (another thread may have loaded while waiting)
  → LoadFromBinary() + TryAdd()
tenantLock.Release()
```

This prevents the CLR error _"Assembly with same name is already loaded"_ that occurs when two concurrent cache-miss threads both call `LoadFromBinary` for the same assembly into the same `TenantAssemblyLoadContext`.

### Cache disabled (timeout = zero)

When `AssemblyCacheTimeout` is `TimeSpan.Zero`, each invocation gets a fresh `TenantAssemblyLoadContext`. The context is kept alive by the returned assembly reference and collected by the GC once the assembly goes out of scope. No semaphore is used in this path.

### LRU eviction

A background `Timer` fires at `cacheTimeout / 4` intervals and removes all entries whose `LastUsed` timestamp is older than `CacheTimeout`. Orphaned `TenantAssemblyLoadContext` instances are disposed and removed from `TenantContexts` during the same sweep, which allows the CLR to collect the underlying collectible context.

### Fault recovery

If `LoadFromBinary` throws (e.g. after a `ScriptPublish` without a backend restart leaves the same assembly name already loaded in the context), the cache self-heals:

1. `CacheClearTenant(tenantId)` is called immediately — the broken `TenantAssemblyLoadContext` is disposed and removed.
2. A descriptive `PayrollException` is thrown with the message _"The tenant assembly cache has been cleared — retry the operation."_
3. The next request creates a fresh `TenantAssemblyLoadContext` and succeeds.

This means a backend restart is no longer required after redeploying scripts.

### Management API

| Method | Description |
|---|---|
| `CacheClear()` | Evicts all tenants and disposes all load contexts (process-wide reset). |
| `CacheClearTenant(tenantId)` | Evicts a single tenant and disposes its load context. Called automatically on load failure. |

### Configuration

`FunctionHostSettings.AssemblyCacheTimeout` (default: **30 minutes**). Pass `TimeSpan.Zero` to disable caching entirely, which is useful in test setups or single-tenant development environments.

---

## CollectibleAssemblyLoadContext

Thin wrapper around `AssemblyLoadContext` created with `isCollectible: true`. The `Load` override returns `null` so all dependency resolution falls back to the default context — scripts share the same runtime and PE references as the host process. `LoadFromBinary(byte[])` streams the compiled binary directly from memory without touching the file system.

---

## FunctionHost

Implements `IFunctionHost`. Acts as the single entry point used by all `RuntimeBase` subclasses to obtain an assembly:

```csharp
var assembly = FunctionHost.GetObjectAssembly(tenantId, typeof(T), item);
```

`FunctionHost` also persists `Log` and `Task` objects created by script code, bridging the synchronous scripting boundary to the async repository layer via `ConfigureAwait(false).GetAwaiter().GetResult()`.

---

## CaseValueProvider

Resolves case field values for scripts during payrun execution. It is the single authoritative source for all case value lookups regardless of scope (Global, National, Company, Employee) and time type (Timeless, Moment, Period, CalendarPeriod).

> **Threading:** `CaseValueProvider` is **not thread-safe**. One instance must only be used from one logical execution context at a time — typically one payrun job.

### Construction and scope repositories

```csharp
new CaseValueProvider(
    settings,
    globalCaseValueRepository,
    nationalCaseValueRepository,
    companyCaseValueRepository,
    employeeCaseValueRepository,   // null in non-employee contexts
    employee)
```

Each repository is optional at construction time. Accessing a scope for which no repository was provided throws an `InvalidOperationException` with a descriptive message identifying the missing scope. The settings object (`CaseValueProviderSettings`) carries the shared state:

| Setting | Purpose |
|---|---|
| `DbContext` | Database access for case field metadata |
| `Calculator` | Payroll calculator for pro-rata and period calculations |
| `CaseFieldProvider` | Resolves case field definitions and their case type |
| `EvaluationPeriod` | The active payrun period (UTC, pushed as bottom of period stack) |
| `EvaluationDate` | The exact evaluation moment within the period |
| `RetroDate` | When set, triggers retro detection for case value changes after this date |

### Period stack

The active evaluation period is managed as a `Stack<DatePeriod>`. The bottom entry is the payrun period from `CaseValueProviderSettings`. Callers push temporary sub-periods for split-period calculations using the internal `UseEvaluationPeriod(period)` scope helper, which returns an `IDisposable` that pops the period automatically:

```csharp
using (UseEvaluationPeriod(splitPeriod))
{
    var values = await GetCasePeriodValuesAsync(caseFieldName);
}
```

`EvaluationPeriod` always reads `evaluationPeriods.Peek()` — the topmost active period.

### Calculator stack

Analogous to the period stack, a `Stack<IPayrollCalculator>` allows wage types with a custom calendar to temporarily override the calculator. `PushCalculator` validates that the new calculator's cycle and period time units are compatible with the current one before pushing.

### Time type dispatch

The core resolution method `GetCasePeriodValuesAsync(string)` loads case values from the cache for the active evaluation period, then dispatches to one of four resolution strategies based on `CaseField.TimeType`:

| TimeType | Resolution strategy |
|---|---|
| `Timeless` | Most recently created value with `Created ≤ EvaluationDate`. No period boundaries apply; the evaluation period's start/end are used as the output period. |
| `Moment` | Latest value whose `Start ≤ EvaluationPeriod.End`. Decimal values are accumulated across all qualifying values. Non-decimals return the single latest. |
| `Period` | Case values are split into sub-periods via `SplitCaseValuePeriods`. The field's `PeriodAggregation` rule (`First`, `Last`, `Summary`) selects which sub-periods contribute. |
| `CalendarPeriod` | Same split as `Period`, but the calculator pro-rates each value according to the calendar fraction of the sub-period relative to the full payrun period. End dates are normalized to last-moment-of-day. |

### Period splitting algorithm (`CaseValueProviderCalculation`)

`SplitCaseValuePeriods` is the central algorithm for multi-value fields. It takes a list of case values ordered newest-first (by `Created`) and assigns each value the sub-periods it "owns" within the evaluation period, consuming available time from the front:

```
EvaluationPeriod:  |───────────────────────────────|
CaseValue A (newest, Created=T2): |────────|
CaseValue B (older,  Created=T1):          |────────|
                                  ↑        ↑        ↑
                               remaining taken  remaining
```

For each case value, the algorithm intersects its effective `[Start, End]` range with the remaining available periods, producing:

- **Value periods** — the portion of the case value's range that still has available time.
- **Remaining periods** — the gaps before and after the value that are passed to the next (older) case value.

The result is a `Dictionary<CaseValue, List<DatePeriod>>` mapping each case value to the sub-periods it covers within the evaluation period. Midnight boundaries are adjusted by one tick to prevent zero-length periods.

### Multi-field period splitting (`GetCasePeriodValuesAsync(DatePeriod, IEnumerable<string>)`)

When multiple case field names are requested for the same period:

1. All fields are loaded individually within the full evaluation period.
2. All `Start`/`End` boundaries from every returned `CaseFieldValue` are collected into a `SortedSet<DateTime>` together with the period's own boundaries.
3. Adjacent moment pairs become sub-periods (skipping zero-length pairs after tick correction).
4. If more than one sub-period results, each field is re-evaluated per sub-period using the period stack. Each field must return exactly one value per sub-period; a deviation throws `InvalidOperationException`.

For single-field requests the splitting step is skipped entirely.

### Retro detection

When `RetroDate` is set, every call to `CalculatePeriodCaseValuesAsync` also checks whether a qualifying retro trigger exists for the field. A trigger is a case value created after `RetroDate` whose `Start` falls before `EvaluationPeriod.End`. The earliest qualifying value across all fields is stored in `RetroCaseValue`. The property is only ever tightened — it never moves forward — so repeated calls across multiple fields accumulate to the earliest retro date found.

### `GetTimeCaseValuesAsync`

Returns a snapshot of all case values at a given point in time (`valueDate`) for an entire case type scope. Selection per `TimeType`:

| TimeType | Selection |
|---|---|
| `Timeless` | Most recently created value overall |
| `Moment` | Latest value with `Start ≤ valueDate` |
| `Period` / `CalendarPeriod` | Most recently created value whose period contains `valueDate` |

Unknown field names and type mismatches are silently skipped.

### `GetCaseValuesAsync`

Returns all case values for a single field whose `Created` timestamp falls within a given `evaluationPeriod`. An optional `caseSlot` parameter restricts results to one named slot. The slot filter skips values whose `CaseSlot` does not match (case-insensitive).

### `GetCaseValueSlotsAsync`

Returns the distinct slot names that exist for a given case field, delegating to the scope's cache repository.

---

## Runtime layer

`RuntimeBase` is the abstract base for all function runtimes. It provides tenant/user context, culture and calendar resolution, webhook invocation, and the `CreateScript<T>` factory method which resolves the function type from the loaded assembly and instantiates it via `Activator.CreateInstance`.

Concrete runtimes are organised by domain object:

| Domain object | Runtimes |
|---|---|
| Case | `CaseAvailableRuntime`, `CaseBuildRuntime`, `CaseValidateRuntime` |
| Case relation | `CaseRelationBuildRuntime`, `CaseRelationValidateRuntime` |
| Wage type | `WageTypeValueRuntime`, `WageTypeResultRuntime` |
| Collector | `CollectorStartRuntime`, `CollectorApplyRuntime`, `CollectorEndRuntime` |
| Payrun | `PayrunStartRuntime`, `PayrunEndRuntime`, `PayrunEmployeeAvailableRuntime`, `PayrunEmployeeStartRuntime`, `PayrunEmployeeEndRuntime`, `PayrunWageTypeAvailableRuntime` |
| Report | `ReportStartRuntime`, `ReportBuildRuntime`, `ReportEndRuntime` |

Script function timeout defaults are defined in `BackendScriptingSpecification`: **100 seconds** for all functions (overridable to 10,000 seconds via `#define MAX_SCRIPT_TIMEOUT` for debugging).

---

## Script controllers

`ScriptControllerBase<TDomain>` and the concrete controllers in `Controller/` orchestrate compilation for each domain object type. They hold the compile-side logic (calling `ScriptCompiler.Compile`) while the runtime subclasses hold the execution-side logic.

---

## No-Code action system

The `Action/` folder implements the expression-based No-Code layer. `ActionParser` translates action expressions (tokens such as `^$`, `^&`, `^^`) into C# code fragments (`ActionResult`) that are injected into the `#region Actions` and `#region ActionsInvoke` placeholders of the function template before compilation. Object names in actions must be PascalCase — spaces break the token parser.

---

## Debug switches

| Symbol | Effect |
|---|---|
| `ASSEMBLY_GET` | Logs binary fetch duration |
| `ASSEMBLY_CACHE` | Logs cache hit/miss ratio |
| `ASSEMBLY_LOAD` | Logs assembly load duration |
| `COMPILER_PERFORMANCE` | Logs Roslyn compile duration |
| `MAX_SCRIPT_TIMEOUT` | Extends function timeout to ~2.7 hours |
| `ScriptCompiler.DumpCompilerSources` | Writes all compiler input files to `ScriptDump/` |
| `ScriptCompiler.ScriptSafetyAnalysis` | Enables static banned-API analysis before emit |
