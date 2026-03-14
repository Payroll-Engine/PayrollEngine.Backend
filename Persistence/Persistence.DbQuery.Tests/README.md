# Persistence.DbQuery.Tests

Unit tests for the `DbQuery` OData-to-SQL layer. Tests run without a database —
the `SqlServerCompiler` from SqlKata compiles queries to SQL strings in memory.

## Design Principle

```
OData string  →  DbQueryFactory.NewQuery<T>  →  SqlKata.Query  →  SqlServerCompiler  →  SQL string  →  Assert
```

No mocks, no HTTP, no database connection required.

## Infrastructure

### `QueryTestBase`
Base class for all test classes. Provides two query helpers:

| Helper | Returns | Use for |
|---|---|---|
| `Sql(...)` | `string` | Structural assertions — column names, operators, SQL keywords |
| `Result(...)` | `SqlResult` | Value assertions via `.Bindings` |

**Why two helpers?** SqlKata parametrizes all literal values into `@p0`, `@p1` etc.
The SQL string never contains the actual values — they are in `SqlResult.Bindings`.

```csharp
// Structural assertion — correct
var sql = Sql(filter: "Status eq 1");
Assert.Contains("[Status]", sql);
Assert.Contains("=", sql);

// Value assertion — correct
var result = Result(filter: "Status eq 1");
Assert.Contains(1, result.Bindings);

// Wrong — value is not in the SQL string
Assert.Contains("= 1", sql); // fails
```

**Bindings type note:** `$top` and `$skip` may be stored as `long` by SqlKata.
Use `Convert.ToInt64(b)` for reliable comparisons.

#### Parameters

| Parameter | Type | Description |
|---|---|---|
| `filter` | `string` | OData `$filter` expression |
| `orderBy` | `string` | OData `$orderby` expression |
| `select` | `string` | OData `$select` expression |
| `top` | `int?` | OData `$top` value |
| `skip` | `int?` | OData `$skip` value |
| `queryMode` | `QueryMode` | `Item` (default) or `ItemCount` |
| `status` | `ObjectStatus?` | `Query.Status` — adds typed WHERE on Status column |
| `apply` | `string` | OData `$apply` expression for aggregation/groupby |

For attribute-column tests (`IDomainAttributeObject`), use the generic overloads
`Sql<T>(dbContext, ...)` and `Result<T>(dbContext, ...)` with a `StubDbContext`.

### `TestEntity`
Simple in-memory entity with typed columns: `Id`, `Name`, `Status`, `Amount`,
`Created`, `Active`. Does **not** implement `IDomainAttributeObject` — stays on
the `TypeQueryBuilder` path (no OPENJSON). Used by all non-attribute tests.

### `StubDbContext`
Minimal `IDbContext` stub. Only `DateTimeType` and `DecimalType` are implemented —
these are needed when generating OPENJSON sub-selects for attribute columns.
All other members throw `NotSupportedException`.

---

## Test Classes

### `FilterTests` — `$filter`

Covers all OData filter operators, string functions, datetime functions, boolean
logic and field-name case-insensitivity.

| Group | Tests |
|---|---|
| Comparison operators | `eq`, `ne`, `gt`, `ge`, `lt`, `le` — structural (SQL) and value (Bindings) |
| String functions | `contains`, `startswith`, `endswith` — LIKE pattern in Bindings |
| Datetime functions | `year`, `month`, `day`, `hour`, `minute`, `date` → `DATEPART(...)` |
| Boolean logic | `and`, `or`, `not`, nested grouping with `()` |
| Field names | Case-insensitive: `status`, `STATUS`, `Status` all resolve to `[Status]` |
| Error cases | Unknown field → `QueryException` |

**Key behaviour:** `notcontains` is rejected by the OData parser (`QueryException`).
Use `not contains(Field, 'value')` instead.

**Datetime rendering:** SqlKata 4.x renders `year()` as `DATEPART(YEAR, [col])`,
not as `YEAR([col])`. Tests assert on `DATEPART(YEAR`, not `YEAR(`.

---

### `OrderBySelectTests` — `$orderby`, `$select`, `$top`, `$skip`

| Group | Tests |
|---|---|
| `$orderby` | Ascending, descending, multiple columns, ignored in count mode |
| `$select` | Single column, multiple columns, unknown column → `QueryException`, ignored in count mode |
| `$top` / `$skip` | Values in Bindings (OFFSET/FETCH), ignored in count mode |
| Count mode | `count(*)` produced, TOP/SKIP/ORDERBY suppressed |
| Combined | filter + orderby + select + top together |

---

### `StatusFilterTests` — `Query.Status`

Regression guard for the `DbQueryFactory.BuildQuery` status filter path,
refactored from `switch` to `Query.When()`.

| Test | Verifies |
|---|---|
| `NoStatus_ProducesNoStatusWhere` | `null` status → no WHERE |
| `NoStatus_WithOtherFilter_StatusColumnNotDoubled` | OData filter on Status not duplicated |
| `ActiveStatus_ProducesStatusWhereClause` | Active → WHERE `[Status]` |
| `ActiveStatus_BindsActiveValue` | Active value (0) in Bindings |
| `InactiveStatus_ProducesStatusWhereClause` | Inactive → WHERE `[Status]` |
| `InactiveStatus_BindsInactiveValue` | Inactive value (1) in Bindings |
| `ActiveAndInactive_ProduceDifferentBindings` | Active ≠ Inactive binding |
| `ActiveStatus_CombinedWithODataFilter_BothConditionsPresent` | Status + OData filter both in WHERE |
| `ActiveStatus_CombinedWithODataFilter_CorrectBindingCount` | Exactly 2 bindings |
| `ActiveStatus_CountMode_StillFiltersStatus` | Status filter survives count mode |

---

### `ApplyTests` — `$apply` (aggregation / groupby)

Tests for `Query.Apply` which maps to the OData `$apply` clause via
`ApplyClauseBuilder`.

**Aggregate functions:**

| Test | Expression | Asserts |
|---|---|---|
| `Aggregate_Count` | `aggregate($count as Count)` | `COUNT(1)` |
| `Aggregate_Sum` | `aggregate(Amount with sum as TotalAmount)` | `sum(` (case-insensitive) |
| `Aggregate_Average` | `aggregate(Amount with average as AvgAmount)` | `avg(` |
| `Aggregate_Min` | `aggregate(Amount with min as MinAmount)` | `min(` |
| `Aggregate_Max` | `aggregate(Amount with max as MaxAmount)` | `max(` |
| `Aggregate_CountDistinct` | `aggregate(Status with countdistinct as UniqueStatuses)` | `COUNT(DISTINCT` |

> **Note:** `SelectRaw` preserves the OData-parser casing (`Sum`, not `SUM`).
> All aggregate assertions use `StringComparison.OrdinalIgnoreCase`.

**GroupBy:**

| Test | Expression | Asserts |
|---|---|---|
| `GroupBy_ProducesGroupBySql` | `groupby((Status))` | `GROUP BY [Status]` |
| `GroupBy_WithAggregate` | `groupby((Status), aggregate($count as Count))` | `GROUP BY` + `COUNT(1)` |
| `GroupBy_MultipleFields` | `groupby((Status, Active))` | `[Status]`, `[Active]` |

**Filter within apply:**

| Test | Expression | Asserts |
|---|---|---|
| `Filter_Then_Aggregate` | `filter(Status eq 1)/aggregate($count as Count)` | `WHERE` + `COUNT(1)` |
| `Filter_Then_GroupBy` | `filter(Status eq 1)/groupby((Active))` | `WHERE` + `GROUP BY` |

**Post-apply filter (OData restriction):**

Post-apply `$filter` is restricted by the OData standard — only columns listed
in `groupby((...))` can be filtered afterwards. Non-grouped columns and
aggregate aliases throw `QueryException`.

| Test | Behaviour |
|---|---|
| `Apply_WithGroupedColumnFilter_BothPresent` | Filter on grouped column → works |
| `Apply_WithAggregateAliasFilter_ThrowsQueryException` | Filter on aggregate alias → `QueryException` |

**SqlKata unit tests** (independent of OData layer — verify SQL generation directly):
`SqlKata_GroupByAggregate_ProducesExpectedSql`, `SqlKata_SubQuery_WrapsProperly`

---

### `AttributeQueryTests` — OPENJSON / `IDomainAttributeObject`

Uses `Employee` (implements `IDomainAttributeObject`) to test the attribute-column
path that generates `OPENJSON` sub-selects.

| Group | Tests |
|---|---|
| Typed fields | `Identifier`, `FirstName`, `Id` — standard WHERE, no OPENJSON |
| Text attributes | `textAttributes.Department` → OPENJSON in filter and select |
| Date attributes | `dateAttributes.HireDate` → OPENJSON + `DATETIME2(7)` CAST |
| Numeric attributes | `numericAttributes.Salary` → OPENJSON + `DECIMAL(28, 6)` CAST |
| SQL injection | Malicious attribute name → blocked by OData parser (`QueryException`) |
| Fast path | No attribute columns → no OPENJSON |
| Count mode | Attribute query in count mode → wrapped in count sub-query |

---

### `ErrorCaseTests` — Boundary and regression cases

| Group | Tests |
|---|---|
| WHERE presence | Valid filter always produces WHERE |
| AND filter | Both columns in WHERE, both values in Bindings |
| NOT operator | `not contains(...)`, `not (a and b)`, `not (field eq val)` all negate |
| Invalid fields | Unknown field/select → `QueryException` |
| Malformed OData | Incomplete/invalid syntax → `QueryException` |
| Arithmetic operators | `Amount add 10 eq 110` → `QueryException` (unsupported) |
| Null/empty query | No filter → no WHERE, table name present |
| `top: 0` | Treated as no limit — FETCH clause omitted |
| `top: int.MaxValue` | Value in Bindings, no overflow |

---

## Running the Tests

```
dotnet test Persistence\Persistence.DbQuery.Tests\PayrollEngine.Persistence.DbQuery.Tests.csproj
```

Or via Visual Studio Test Explorer — all tests appear under
`PayrollEngine.Persistence.DbQuery.Tests`.
