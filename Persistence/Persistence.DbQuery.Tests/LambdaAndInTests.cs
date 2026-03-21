using Xunit;
using PayrollEngine.Persistence.DbQuery.Tests.Infrastructure;

namespace PayrollEngine.Persistence.DbQuery.Tests;

/// <summary>
/// Tests for OData lambda operators (any()) and the in-operator.
///
/// any() translates to a correlated EXISTS (SELECT 1 FROM OPENJSON(...)) sub-query.
/// in    translates to WHERE col IN (@p0, @p1, ...) with values in Bindings.
///
/// SqlKata parametrizes ALL literal values:
/// - Structural assertions (EXISTS, OPENJSON, IN, column names) → Sql()
/// - Value assertions → Result().Bindings
/// </summary>
public class LambdaAndInTests : QueryTestBase
{
    // =========================================================================
    // any() — scalar string array  (e.g. Divisions, Tags)
    // =========================================================================

    [Fact]
    public void Any_ScalarArray_ProducesExists()
    {
        var sql = Sql(filter: "Divisions/any(d: d eq 'HR')");
        Assert.Contains("EXISTS", sql);
    }

    [Fact]
    public void Any_ScalarArray_ProducesOpenjson()
    {
        var sql = Sql(filter: "Divisions/any(d: d eq 'HR')");
        Assert.Contains("OPENJSON", sql.ToUpperInvariant());
    }

    [Fact]
    public void Any_ScalarArray_ReferencesCollectionColumn()
    {
        var sql = Sql(filter: "Divisions/any(d: d eq 'HR')");
        Assert.Contains("[Divisions]", sql);
    }

    [Fact]
    public void Any_ScalarArray_ValueInBindings()
    {
        var result = Result(filter: "Divisions/any(d: d eq 'HR')");
        Assert.Contains("HR", result.Bindings);
    }

    [Fact]
    public void Any_ScalarArray_ValueColumn_IsLower()
    {
        // Scalar OPENJSON uses the built-in [value] column
        var sql = Sql(filter: "Divisions/any(d: d eq 'HR')");
        Assert.Contains("[value]", sql.ToLowerInvariant());
    }

    [Fact]
    public void Any_Tags_ScalarArray_Works()
    {
        var sql = Sql(filter: "Tags/any(t: t eq 'payroll')");
        Assert.Contains("EXISTS", sql);
        Assert.Contains("[Tags]", sql);
    }

    // =========================================================================
    // any() — combined with other filters (and)
    // =========================================================================

    [Fact]
    public void Any_CombinedWithScalarFilter_BothInSql()
    {
        var sql = Sql(filter: "Status eq 1 and Divisions/any(d: d eq 'HR')");
        Assert.Contains("[Status]", sql);
        Assert.Contains("EXISTS", sql);
        Assert.Contains("[Divisions]", sql);
    }

    [Fact]
    public void Any_CombinedWithScalarFilter_ValuesInBindings()
    {
        var result = Result(filter: "Status eq 1 and Divisions/any(d: d eq 'Finance')");
        Assert.Contains(1, result.Bindings);
        Assert.Contains("Finance", result.Bindings);
    }

    // =========================================================================
    // any() — error cases
    // =========================================================================

    [Fact]
    public void Any_OnNonCollectionColumn_ThrowsQueryException()
    {
        // Name is a string (scalar), not a List<string> — must reject any()
        Assert.Throws<QueryException>(() => Sql(filter: "Name/any(n: n eq 'test')"));
    }

    [Fact]
    public void Any_OnUnknownColumn_ThrowsQueryException()
    {
        Assert.Throws<QueryException>(() => Sql(filter: "UnknownCollection/any(x: x eq 'y')"));
    }

    // =========================================================================
    // in — single value (degenerate but valid)
    // =========================================================================

    [Fact]
    public void In_SingleValue_ProducesIn()
    {
        var sql = Sql(filter: "Status in (1)");
        Assert.Contains("IN", sql.ToUpperInvariant());
        Assert.Contains("[Status]", sql);
    }

    [Fact]
    public void In_SingleValue_ValueInBindings()
    {
        var result = Result(filter: "Status in (1)");
        Assert.Contains(1, result.Bindings);
    }

    // =========================================================================
    // in — multiple values
    // =========================================================================

    [Fact]
    public void In_MultipleInts_ProducesIn()
    {
        var sql = Sql(filter: "Status in (1,2,3)");
        Assert.Contains("IN", sql.ToUpperInvariant());
        Assert.Contains("[Status]", sql);
    }

    [Fact]
    public void In_MultipleInts_AllValuesInBindings()
    {
        var result = Result(filter: "Status in (1,2,3)");
        Assert.Contains(1, result.Bindings);
        Assert.Contains(2, result.Bindings);
        Assert.Contains(3, result.Bindings);
    }

    [Fact]
    public void In_Strings_AllValuesInBindings()
    {
        var result = Result(filter: "Name in ('Alice','Bob','Carol')");
        Assert.Contains("Alice", result.Bindings);
        Assert.Contains("Bob", result.Bindings);
        Assert.Contains("Carol", result.Bindings);
    }

    [Fact]
    public void In_Strings_ProducesWhereClause()
    {
        var sql = Sql(filter: "Name in ('Alice','Bob')");
        Assert.Contains("WHERE", sql);
        Assert.Contains("[Name]", sql);
    }

    // =========================================================================
    // in — combined with other filters
    // =========================================================================

    [Fact]
    public void In_CombinedWithComparison_BothInSql()
    {
        var sql = Sql(filter: "Status in (1,2) and Amount gt 0");
        Assert.Contains("IN", sql.ToUpperInvariant());
        Assert.Contains("[Amount]", sql);
    }

    [Fact]
    public void In_CombinedWithComparison_ValuesInBindings()
    {
        var result = Result(filter: "Status in (1,2) and Amount gt 0");
        Assert.Contains(1, result.Bindings);
        Assert.Contains(2, result.Bindings);
    }

    // =========================================================================
    // in — error cases
    // =========================================================================

    [Fact]
    public void In_UnknownField_ThrowsQueryException()
    {
        Assert.Throws<QueryException>(() => Sql(filter: "UnknownField in (1,2)"));
    }
}
