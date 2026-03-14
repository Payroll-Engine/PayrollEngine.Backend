# DbQuery — OData Implementation Notes

This document consolidates findings from the OData implementation review and
the subsequent test suite development. It serves as the authoritative reference
for anyone maintaining or extending the `DbQuery` layer.

## Architecture Overview

```
OData string (from API)
    ↓
ODataQueryOptionParser (Microsoft.OData)
    ↓
QueryBuilderBase / FilterClauseBuilder / ApplyClauseBuilder
    ↓
SqlKata.Query  (SqlKata 4.0.1)
    ↓
SqlServerCompiler → parameterized SQL string + Bindings
    ↓
Dapper (IDbContext.QueryAsync)
```

Key design principle: the OData string is parsed **before** SqlKata sees it.
All validation (field names, operators) happens at OData-parse time or in
`ValidateColumn`. SqlKata only receives already-validated, structured clauses.

---

## File Map

| File | Responsibility |
|---|---|
| `QueryBuilderBase.cs` | Entry point — parses OData options, builds SqlKata query |
| `FilterClauseBuilder.cs` | Translates OData filter AST nodes into SqlKata `Where*` calls |
| `ApplyClauseBuilder.cs` | Translates OData `$apply` (aggregation/groupby) into SqlKata |
| `TypeQueryBuilder.cs` | Provides column type info for typed entities via reflection |
| `DynamicTypeQueryBuilder.cs` | Extends `TypeQueryBuilder` for attribute columns (OPENJSON) |
| `IQueryContext.cs` | Contract for column validation and type resolution |
| `QueryNodeExtensions.cs` | Extension methods for OData AST node traversal |
| `QueryMode.cs` | Enum: `Item` (SELECT) vs `ItemCount` (COUNT) |

---

## Supported OData Features

See `OData.md` in the repository root for the end-user-facing feature list.
The following documents the **implementation-level** behaviour.

### Filter operators
All comparison operators (`eq`, `ne`, `gt`, `ge`, `lt`, `le`) and boolean
combinators (`and`, `or`, `not`) are implemented in `FilterClauseBuilder.Visit(BinaryOperatorNode)`.

### String functions
`contains`, `startswith`, `endswith` map to SqlKata `WhereContains`,
`WhereStarts`, `WhereEnds` with `caseSensitive: false`.

**Case-insensitivity:** SqlKata 4.x does not inject `LOWER()` into the SQL.
Instead, the column reference is lowercased in the rendered SQL (e.g.
`[name]` instead of `[Name]`), and case-insensitivity relies on the
SQL Server collation `SQL_Latin1_General_CP1_CS_AS`.

### Datetime functions
`year`, `month`, `day`, `hour`, `minute` map to SqlKata `WhereDatePart`,
which renders as `DATEPART(FUNC, [col]) = @p0` in SQL Server — **not** as
`FUNC([col])`. Tests must assert on `DATEPART(YEAR, ...)`, not `YEAR(...)`.

`date` maps to `WhereDate` → `CAST([col] AS date) = @p0`.
`time` maps to `WhereTime` → `CAST([col] AS time) = @p0`.

### NOT operator
`not` on a function call or comparison works correctly:
`Visit(UnaryOperatorNode)` calls `query.Not()` and then `Accept()` on the
operand when `operand.Kind` is `BinaryOperator` or `SingleValueFunctionCall`.
This covers all valid OData NOT expressions.

### `notcontains` — PE custom extension
`notcontains` is handled in `FilterClauseBuilder.Visit(SingleValueFunctionCallNode)`
via `WhereNotContains`, but it is **not a standard OData function** and is
rejected by the `Microsoft.OData` parser before `FilterClauseBuilder` is reached.

**Use `not contains(Field, 'value')` instead.**

---

## Attribute Columns (OPENJSON)

For entities implementing `IDomainAttributeObject`, the `DynamicTypeQueryBuilder`
recognises attribute-prefixed columns (e.g. `textAttributes.Department`) and
`DbQueryFactory.NewQuery<T>` injects `OPENJSON` sub-selects:

```sql
(SELECT value FROM OPENJSON(Attributes) WHERE [key] = 'Department') AS textAttributes.Department
```

Typed columns (text / date / numeric) receive an explicit `CAST` using
`IDbContext.DateTimeType` and `IDbContext.DecimalType` respectively.

### OPENJSON and SQL injection
SQL injection via attribute names is **effectively blocked by the OData parser**:
the `Microsoft.OData` parser only accepts valid OData identifiers
(`[A-Za-z_][A-Za-z0-9_]*`) — characters like `'`, `;`, `--` cause a parse
error before `BuildAttributeQuery` is ever reached.

Residual risk: a direct internal call to `BuildAttributeQuery` bypassing the
OData layer. `BuildAttributeQuery` has no explicit whitelist validation today.

---

## Known Limitations

### Arithmetic operators throw `QueryException`
Arithmetic filter expressions (`Amount add 10 eq 110`) are syntactically
accepted by the OData parser but cannot be processed by `FilterClauseBuilder`:
`GetColumnName` receives a `BinaryOperator` node (not a property access) and
returns an empty string, causing `ValidateColumn` to throw `ArgumentException`.

This is caught and wrapped by `QueryBuilderBase.BuildQuery` and surfaces as
`QueryException` with message `Query filter error: ...`.
Covered by the test `ArithmeticOperator_Add_IsNotSupported_ThrowsQueryException`.

### `$apply` — groupby and aggregation
Use `Query.Apply` to pass an OData `$apply` expression. Supported transformations:
- `aggregate(Field with sum/average/min/max/countdistinct as Alias)`
- `aggregate($count as Alias)`
- `groupby((Field))`
- `groupby((Field), aggregate(...))`
- `filter(condition)/groupby(...)` — filter before groupby
- `filter(condition)/aggregate(...)` — filter before aggregate

The `Apply` expression is mapped to the OData parser via `QuerySpecification.ApplyOperation`
and executed by `ApplyClauseBuilder`.

**Limitation:** Post-apply `$filter` is restricted by the OData parser to columns that were
part of the `groupby` clause. Filtering on non-grouped entity columns or on aggregate aliases
(e.g. `Count`) throws `QueryException`. Only columns explicitly listed in `groupby((Col1, Col2))`
can be used in a subsequent `filter`.

**Note:** `SelectRaw` in `ApplyClauseBuilder` preserves the casing from the OData parser
(`Sum`, `Min`, `Max`) — not uppercase. String comparisons on SQL output must be case-insensitive.

### Direct `Clauses` manipulation
`DbQueryFactory.NewQuery<T>` uses `dbQuery.Clauses.Add/Remove` directly to
transfer clauses between the inner and outer query for the attribute path.
This bypasses the SqlKata API and is fragile against SqlKata version changes.
`Query.Clone()` does not help here — the goal is a selective clause transfer,
not a full clone.

### `top: 0` is silently ignored
Passing `top: 0` to `QueryBuilderBase` is treated identically to no top —
the `Take(0)` call results in no `FETCH` clause in the compiled SQL.

---

## SqlKata 4.0 Features Used

| Feature | Where used |
|---|---|
| `Query.When(condition, callback)` | `DbQueryFactory.BuildQuery` — status filter |
| `Query.Take/Skip` | `QueryBuilderBase.BuildQuery` — pagination |
| `WhereDatePart` | `FilterClauseBuilder.ApplyFunction` — date functions |
| `WhereContains/WhereStarts/WhereEnds` | `FilterClauseBuilder.Visit(SingleValueFunctionCallNode)` |
| `WhereNotContains` | `FilterClauseBuilder.Visit(SingleValueFunctionCallNode)` |
| `SelectRaw` | `DbQueryFactory` — OPENJSON injection |
| `AsCount` | `QueryBuilderBase.BuildQuery` — count mode |
| `Query.From(subQuery)` | `ApplyClauseBuilder`, `DbQueryFactory` — sub-query wrapping |

### `Query.Clone()` — not applicable
`Clone()` would help for template-based query patterns (one base query,
multiple variants). PE builds each query fresh per request — no use case exists.

---

## SqlKata Value Parameterization

SqlKata parametrizes **all** literal values. The compiled SQL contains only
`@p0`, `@p1` etc. — literal values are in `SqlResult.Bindings`.

Tests must therefore:
- Assert column names, operators, SQL keywords on `result.Sql`
- Assert literal values on `result.Bindings`

For `top`/`skip`, SqlKata may store bindings as `long` — use
`Convert.ToInt64(b)` for reliable comparisons.

---

## Test Coverage

Tests are located in `Persistence.DbQuery.Tests`:

| Test class | What it covers |
|---|---|
| `FilterTests` | All $filter operators, string/datetime functions, NOT, AND/OR |
| `OrderBySelectTests` | $orderby, $select, $top, $skip, count mode |
| `StatusFilterTests` | `Query.Status` filter path in `DbQueryFactory` (regression guard for `When()` refactoring) |
| `AttributeQueryTests` | OPENJSON path for `IDomainAttributeObject` entities |
| `ApplyTests` | $apply API gap documentation + SqlKata aggregate unit tests |
| `ErrorCaseTests` | Invalid fields, malformed OData, NOT behaviour, boundary values |
