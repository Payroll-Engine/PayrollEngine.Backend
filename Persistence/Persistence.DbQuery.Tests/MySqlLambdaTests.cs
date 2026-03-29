using Xunit;
using PayrollEngine.Persistence.DbQuery.Tests.Infrastructure;
// ReSharper disable StringLiteralTypo

namespace PayrollEngine.Persistence.DbQuery.Tests;

/// <summary>
/// Tests for OData any() on the MySQL backend.
///
/// MySQL uses JSON_TABLE instead of SQL Server's OPENJSON.
/// The FROM fragment is provided by MySqlStubDbContext.BuildCollectionFromRaw
/// and compiled with MySqlCompiler — backtick quoting, no square brackets.
///
/// Compiled SQL pattern (scalar):
///   EXISTS (SELECT * FROM JSON_TABLE(`Divisions`, ...) jt WHERE `value` = ?)
///
/// Note on $[*] path syntax: MySqlCompiler processes single-quoted content in
/// FromRaw fragments — '$[*]' and PATH expressions are not preserved verbatim
/// in the compiled SQL string. The JSON path is verified directly on the stub
/// (BuildCollectionFromRaw_Tests) rather than on the compiled output.
///
/// SqlKata parametrizes ALL literal values:
/// - Structural assertions (EXISTS, JSON_TABLE, column names) → SqlMySql()
/// - Value assertions → ResultMySql().Bindings
/// </summary>
public class MySqlLambdaTests : QueryTestBase
{
    private static readonly MySqlStubDbContext MySqlStub = new();

    // =========================================================================
    // BuildCollectionFromRaw — fragment content (independent of compiler)
    // =========================================================================

    [Fact]
    public void BuildCollectionFromRaw_Scalar_ContainsJsonTable()
    {
        var fragment = MySqlStub.BuildCollectionFromRaw("Divisions", isScalar: true, []);
        Assert.Contains("JSON_TABLE", fragment);
    }

    [Fact]
    public void BuildCollectionFromRaw_Scalar_ContainsArrayPath()
    {
        // The JSON_TABLE expression must use the '$[*]' path to iterate the array
        var fragment = MySqlStub.BuildCollectionFromRaw("Divisions", isScalar: true, []);
        Assert.Contains("$[*]", fragment);
    }

    [Fact]
    public void BuildCollectionFromRaw_Scalar_ContainsValueColumn()
    {
        // Scalar arrays expose a single 'value' column (mirrors SQL Server OPENJSON)
        var fragment = MySqlStub.BuildCollectionFromRaw("Divisions", isScalar: true, []);
        Assert.Contains("value VARCHAR(255) PATH '$'", fragment);
    }

    [Fact]
    public void BuildCollectionFromRaw_Scalar_ContainsJtAlias()
    {
        var fragment = MySqlStub.BuildCollectionFromRaw("Divisions", isScalar: true, []);
        Assert.EndsWith(") jt", fragment);
    }

    [Fact]
    public void BuildCollectionFromRaw_Scalar_UsesBacktickQuoting()
    {
        var fragment = MySqlStub.BuildCollectionFromRaw("Divisions", isScalar: true, []);
        Assert.Contains("`Divisions`", fragment);
    }

    [Fact]
    public void BuildCollectionFromRaw_KeyValue_ContainsNamedColumns()
    {
        var fragment = MySqlStub.BuildCollectionFromRaw("Attributes", isScalar: false, ["Key", "Value"]);
        Assert.Contains("`Key`", fragment);
        Assert.Contains("`Value`", fragment);
    }

    [Fact]
    public void BuildCollectionFromRaw_KeyValue_ContainsJsonPaths()
    {
        var fragment = MySqlStub.BuildCollectionFromRaw("Attributes", isScalar: false, ["Key", "Value"]);
        Assert.Contains("PATH '$.key'", fragment);
        Assert.Contains("PATH '$.value'", fragment);
    }

    [Fact]
    public void BuildCollectionFromRaw_KeyValue_DoesNotUseValueColumn()
    {
        // Key/value mode must not use the scalar 'value' column schema
        var fragment = MySqlStub.BuildCollectionFromRaw("Attributes", isScalar: false, ["Key", "Value"]);
        Assert.DoesNotContain("PATH '$'", fragment);
    }

    // =========================================================================
    // any() — scalar array — compiled SQL structure
    // =========================================================================

    [Fact]
    public void MySql_Any_ScalarArray_ProducesExists()
    {
        var sql = SqlMySql(filter: "Divisions/any(d: d eq 'HR')");
        Assert.Contains("EXISTS", sql);
    }

    [Fact]
    public void MySql_Any_ScalarArray_ProducesJsonTable()
    {
        var sql = SqlMySql(filter: "Divisions/any(d: d eq 'HR')");
        Assert.Contains("JSON_TABLE", sql);
    }

    [Fact]
    public void MySql_Any_ScalarArray_DoesNotContainOpenjson()
    {
        // SQL Server OPENJSON must not appear in MySQL output
        var sql = SqlMySql(filter: "Divisions/any(d: d eq 'HR')");
        Assert.DoesNotContain("OPENJSON", sql.ToUpperInvariant());
    }

    [Fact]
    public void MySql_Any_ScalarArray_UsesBacktickQuoting()
    {
        // MySQL compiler uses backtick-quoted identifiers
        var sql = SqlMySql(filter: "Divisions/any(d: d eq 'HR')");
        Assert.Contains("`Divisions`", sql);
    }

    [Fact]
    public void MySql_Any_ScalarArray_ContainsValueColumn()
    {
        // The COLUMNS clause must expose a 'value' column (matches SQL Server [value])
        var sql = SqlMySql(filter: "Divisions/any(d: d eq 'HR')");
        Assert.Contains("value", sql.ToLowerInvariant());
    }

    [Fact]
    public void MySql_Any_ScalarArray_ContainsJtAlias()
    {
        // JSON_TABLE requires a table alias in MySQL syntax
        var sql = SqlMySql(filter: "Divisions/any(d: d eq 'HR')");
        Assert.Contains(") jt", sql);
    }

    [Fact]
    public void MySql_Any_ScalarArray_ValueInBindings()
    {
        var result = ResultMySql(filter: "Divisions/any(d: d eq 'HR')");
        Assert.Contains("HR", result.Bindings);
    }

    [Fact]
    public void MySql_Any_Tags_ScalarArray_Works()
    {
        var sql = SqlMySql(filter: "Tags/any(t: t eq 'payroll')");
        Assert.Contains("EXISTS", sql);
        Assert.Contains("JSON_TABLE", sql);
        Assert.Contains("`Tags`", sql);
    }

    // =========================================================================
    // any() — combined with scalar filter
    // =========================================================================

    [Fact]
    public void MySql_Any_CombinedWithScalarFilter_BothInSql()
    {
        var sql = SqlMySql(filter: "Status eq 1 and Divisions/any(d: d eq 'HR')");
        Assert.Contains("`Status`", sql);
        Assert.Contains("EXISTS", sql);
        Assert.Contains("JSON_TABLE", sql);
    }

    [Fact]
    public void MySql_Any_CombinedWithScalarFilter_ValuesInBindings()
    {
        var result = ResultMySql(filter: "Status eq 1 and Divisions/any(d: d eq 'Finance')");
        Assert.Contains(1, result.Bindings);
        Assert.Contains("Finance", result.Bindings);
    }

    // =========================================================================
    // SQL Server vs MySQL — structural diff
    // =========================================================================

    [Fact]
    public void SqlServer_And_MySql_ProduceDifferentFromFragments()
    {
        const string filter = "Divisions/any(d: d eq 'HR')";
        var sqlServer = Sql(filter: filter);
        var mysql = SqlMySql(filter: filter);

        Assert.NotEqual(sqlServer, mysql);
        Assert.Contains("OPENJSON", sqlServer.ToUpperInvariant());
        Assert.Contains("JSON_TABLE", mysql);
    }

    [Fact]
    public void SqlServer_UsesSquareBrackets_MySql_UsesBackticks()
    {
        const string filter = "Divisions/any(d: d eq 'HR')";
        var sqlServer = Sql(filter: filter);
        var mysql = SqlMySql(filter: filter);

        Assert.Contains("[Divisions]", sqlServer);
        Assert.Contains("`Divisions`", mysql);
    }

    // =========================================================================
    // in — MySQL compiler (backtick quoting)
    // =========================================================================

    [Fact]
    public void MySql_In_MultipleInts_ProducesIn()
    {
        var sql = SqlMySql(filter: "Status in (1,2,3)");
        Assert.Contains("IN", sql.ToUpperInvariant());
        Assert.Contains("`Status`", sql);
    }

    [Fact]
    public void MySql_In_MultipleInts_AllValuesInBindings()
    {
        var result = ResultMySql(filter: "Status in (1,2,3)");
        Assert.Contains(1, result.Bindings);
        Assert.Contains(2, result.Bindings);
        Assert.Contains(3, result.Bindings);
    }

    // =========================================================================
    // any() — key/value Dictionary (Attributes) — MySQL
    // =========================================================================

    // =========================================================================
    // any() — key/value Dictionary (Attributes) — MySQL
    // MySQL uses JSON_CONTAINS_PATH / JSON_UNQUOTE(JSON_EXTRACT(...)) directly
    // rather than an EXISTS subquery — no JSON_TABLE involved.
    // =========================================================================

    [Fact]
    public void MySql_Any_Attributes_SingleKey_UsesJsonContainsPath()
    {
        // Key-only: JSON_CONTAINS_PATH(`Attributes`, 'one', CONCAT('$.', ?))
        var sql = SqlMySql(filter: "Attributes/any(a: a/Key eq 'Department')");
        Assert.Contains("JSON_CONTAINS_PATH", sql);
    }

    [Fact]
    public void MySql_Any_Attributes_SingleKey_ReferencesColumn()
    {
        var sql = SqlMySql(filter: "Attributes/any(a: a/Key eq 'Department')");
        Assert.Contains("`Attributes`", sql);
    }

    [Fact]
    public void MySql_Any_Attributes_SingleKey_DoesNotUseExists()
    {
        // Flat object path uses WhereRaw — not a correlated EXISTS subquery
        var sql = SqlMySql(filter: "Attributes/any(a: a/Key eq 'Department')");
        Assert.DoesNotContain("EXISTS", sql);
    }

    [Fact]
    public void MySql_Any_Attributes_SingleKey_DoesNotUseJsonTable()
    {
        var sql = SqlMySql(filter: "Attributes/any(a: a/Key eq 'Department')");
        Assert.DoesNotContain("JSON_TABLE", sql);
    }

    [Fact]
    public void MySql_Any_Attributes_SingleKey_ValueInBindings()
    {
        var result = ResultMySql(filter: "Attributes/any(a: a/Key eq 'Department')");
        Assert.Contains("Department", result.Bindings);
    }

    [Fact]
    public void MySql_Any_Attributes_KeyAndValue_UsesJsonExtract()
    {
        // Key+Value: JSON_UNQUOTE(JSON_EXTRACT(`Attributes`, CONCAT('$.', ?))) = ?
        var sql = SqlMySql(filter: "Attributes/any(a: a/Key eq 'Department' and a/Value eq 'HR')");
        Assert.Contains("JSON_UNQUOTE", sql);
        Assert.Contains("JSON_EXTRACT", sql);
    }

    [Fact]
    public void MySql_Any_Attributes_KeyAndValue_BothValuesInBindings()
    {
        var result = ResultMySql(filter: "Attributes/any(a: a/Key eq 'Department' and a/Value eq 'HR')");
        Assert.Contains("Department", result.Bindings);
        Assert.Contains("HR", result.Bindings);
    }

    [Fact]
    public void MySql_Any_Attributes_DoesNotUseOpenjson()
    {
        var sql = SqlMySql(filter: "Attributes/any(a: a/Key eq 'Department')");
        Assert.DoesNotContain("OPENJSON", sql.ToUpperInvariant());
    }

    // =========================================================================
    // any() — localization Dictionary<string, string> (NameLocalizations) — MySQL
    // Same flat-object path as Attributes: JSON_CONTAINS_PATH / JSON_UNQUOTE(JSON_EXTRACT(...))
    // =========================================================================

    [Fact]
    public void MySql_Any_Localizations_ByCultureKey_UsesJsonContainsPath()
    {
        var sql = SqlMySql(filter: "NameLocalizations/any(n: n/Key eq 'de-CH')");
        Assert.Contains("JSON_CONTAINS_PATH", sql);
    }

    [Fact]
    public void MySql_Any_Localizations_ByCultureKey_ReferencesColumn()
    {
        var sql = SqlMySql(filter: "NameLocalizations/any(n: n/Key eq 'de-CH')");
        Assert.Contains("`NameLocalizations`", sql);
    }

    [Fact]
    public void MySql_Any_Localizations_ByCultureKey_ValueInBindings()
    {
        var result = ResultMySql(filter: "NameLocalizations/any(n: n/Key eq 'de-CH')");
        Assert.Contains("de-CH", result.Bindings);
    }

    [Fact]
    public void MySql_Any_Localizations_ByCultureKeyAndValue_UsesJsonExtract()
    {
        var sql = SqlMySql(filter: "NameLocalizations/any(n: n/Key eq 'de-CH' and n/Value eq 'Lohnart')");
        Assert.Contains("JSON_UNQUOTE", sql);
        Assert.Contains("JSON_EXTRACT", sql);
    }

    [Fact]
    public void MySql_Any_Localizations_ByCultureKeyAndValue_BothValuesInBindings()
    {
        var result = ResultMySql(filter: "NameLocalizations/any(n: n/Key eq 'de-CH' and n/Value eq 'Lohnart')");
        Assert.Contains("de-CH", result.Bindings);
        Assert.Contains("Lohnart", result.Bindings);
    }

    [Fact]
    public void MySql_Any_Localizations_DoesNotUseExists()
    {
        var sql = SqlMySql(filter: "NameLocalizations/any(n: n/Key eq 'de-CH')");
        Assert.DoesNotContain("EXISTS", sql);
    }

    // =========================================================================
    // error cases — same behaviour on both backends
    // =========================================================================

    [Fact]
    public void MySql_Any_OnNonCollectionColumn_ThrowsQueryException()
    {
        Assert.Throws<QueryException>(() => SqlMySql(filter: "Name/any(n: n eq 'test')"));
    }

    [Fact]
    public void MySql_Any_OnUnknownColumn_ThrowsQueryException()
    {
        Assert.Throws<QueryException>(() => SqlMySql(filter: "UnknownCollection/any(x: x eq 'y')"));
    }
}
