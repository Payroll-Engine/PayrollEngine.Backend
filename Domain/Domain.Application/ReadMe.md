# PayrunProcessor

The `PayrunProcessor` orchestrates the execution of a payrun job for a single payroll period across one or more employees. It is the central entry point for both regular and preview payroll calculations.

## Processing Pipeline

1. **Context setup** — resolve user, payroll, division, culture, calendar and calculator
2. **Job creation** — create or load a pre-created job, resolve parent job for retro invocations
3. **Regulation initialization** — load derived regulations, case field/lookup providers, case value caches, collectors and wage types
4. **Employee resolution** — resolve employees by identifier or load all active employees from the division, optionally filtered by the `EmployeeAvailableExpression` script
5. **Job start** — persist the job with start timestamp and employee count
6. **Regulation validation** — validate the regulation derivation chain
7. **Employee processing** — iterate employees sequentially or in parallel, calculate wage types and collectors, handle retro payrun jobs, persist results

## Architecture

```
PayrunProcessor                          Orchestration, pipeline phases 1-7, job lifecycle
│
├── PayrunContextBuilder                 Phase 1-2: context and job setup
│                                        (user, payroll, division, culture, calendar, calculator,
│                                         job creation/loading, parent job resolution)
│
├── PayrunRegulationInitializer          Phase 3: regulation initialization
│                                        (derived regulations, case field/lookup providers,
│                                         case value caches, derived collectors and wage types)
│
├── EmployeeResolver                     Phase 4: employee resolution and filtering
│                                        (resolve by identifier, load active division employees,
│                                         EmployeeAvailableExpression script filtering)
│
├── PayrunEmployeesProcessor             Phase 7: employee iteration
│   │                                    (sequential/parallel mode, deadlock retry,
│   │                                     progress tracking, payrun start/end scripts)
│   │
│   └── PayrunEmployeeProcessor          Per-employee calculation
│                                        (wage type loop with restart support, collector
│                                         start/apply/end, retro handling, result persistence)
│
├── PayrunRetroProcessor                 Retro payrun processing (one level deep)
│                                        (retro date resolution, RetroTimeType restrictions,
│                                         period iteration, cleanup on failure)
│
├── PayrollCalculatorCache               Thread-safe IPayrollCalculator cache
│                                        (ConcurrentDictionary + Lazy<T>, keyed by calendar|culture)
│
├── CaseValueCacheFactory                Factory for CaseValueCache instances
│                                        (shared parameters: DbContext, DivisionId, EvaluationDate, Forecast)
│
├── IncrementalResultPruner              Removes unchanged collector/wage type results
│                                        (compares current vs. consolidated results in incremental mode)
│
├── PayrunProcessorRegulation            Collector and wage type calculation via assembly cache
│                                        (CollectorStart/Apply/End, WageTypeValue/Result scripts)
│
├── PayrunProcessorScripts               Payrun and employee lifecycle scripts
│                                        (PayrunStart/End, EmployeeStart/End)
│
├── PayrunProcessorRepositories          Database access facade
│                                        (user, payroll, division, regulation loading and validation)
│
├── PayrunProcessorSettings              Configuration and repository references
│                                        (MaxParallelEmployees, FunctionLogTimeout, Mode, repositories)
│
├── PayrunContext                         Shared immutable state across all employees
│                                        (User, Payroll, Division, Calculator, CaseValueCaches,
│                                         DerivedCollectors, DerivedWageTypes, EvaluationPeriod)
│
├── PayrunEmployeeScope                  Per-employee mutable state (isolated per thread)
│                                        (Calculator, Culture stack, DerivedCollectors, ExecutionPhase)
│
├── PayrunProcessorMode                  Enum: Persist | Preview
│
└── PayrunProcessResult                  Preview result DTO (PayrunJob + ResultSet)
```

## Processing Modes

**Persist** (default) — results are written to the database. Supports sequential and parallel employee processing, retro payrun jobs, and incremental result pruning.

**Preview** — results are collected in memory without any database writes. Limited to a single employee. Throws `PayrunPreviewRetroException` if retroactive calculation would be required.

## Employee Processing

`PayrunEmployeesProcessor` iterates employees either sequentially (`MaxParallelEmployees = 0`) or in parallel using `Parallel.ForEachAsync`. Each employee gets its own `PayrunEmployeeScope` for mutable state isolation.

In parallel mode, SQL Server deadlocks (error 1205) are retried up to 3 times with randomized backoff.

### Per-Employee Calculation

`PayrunEmployeeProcessor` handles the calculation for a single employee:

1. Creates an employee-level `CaseValueCache` via `CaseValueCacheFactory`
2. Runs the `EmployeeStart` script
3. Calculates all wage types (with execution restart support) and applies collectors
4. Resolves retro payrun jobs if `RetroPayMode` is enabled
5. Re-evaluates the current period after retro jobs (reevaluation phase)
6. Persists the `PayrollResultSet` (or collects it in memory for preview)
7. Runs the `EmployeeEnd` script

### Wage Type Loop

The wage type calculation supports execution restarts: when a wage type script sets the restart flag, all collector and wage type results are cleared and the entire iteration restarts. This is bounded by `SystemSpecification.PayrunMaxExecutionCount`.

Within each wage type iteration, `CollectorApply` is called for all applicable collectors — this tight coupling between wage types and collectors keeps the calculation logic in a single pass.

## Retro Processing

When retro pay is enabled (`RetroPayMode != None`), `PayrunRetroProcessor`:

1. Determines the retro date from the most recent case value change or script-triggered retro jobs
2. Applies `RetroTimeType` restrictions (`Anytime` or `Cycle`)
3. Creates a new `PayrunProcessor` per retro period with `RetroPayMode.None` (one level deep)
4. Passes preloaded context objects (`PayrunSetup`) to avoid redundant database roundtrips
5. After all retro periods, `PayrunEmployeeProcessor` re-evaluates the current period in `PayrunExecutionPhase.Reevaluation`

On failure, all retro jobs created for the current employee are cleaned up (deleted from the database).

## Cache Strategy

| Cache | Scope | Lifetime |
|---|---|---|
| `PayrollCalculatorCache` | Per PayrunProcessor instance | Shared across all employees and retro jobs |
| Global/National/Company `CaseValueCache` | Per payrun job | Created once, passed to retro jobs via `PayrunSetup` |
| Employee `CaseValueCache` | Per employee | Created fresh for each employee (or reused from `PayrunSetup` in retro) |

## Thread Safety

- `PayrunContext` is shared and read-only after setup
- `PayrunEmployeeScope` is created per employee (isolated per thread in parallel mode)
- `PayrollCalculatorCache` uses `ConcurrentDictionary` with `Lazy<T>` (`ExecutionAndPublication`)
- `ConcurrentDictionary<Employee, Exception>` aggregates errors across parallel threads
- Progress updates use `Interlocked.Increment` in parallel mode
