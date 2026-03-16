using System;
using Xunit;
using PayrollEngine.Persistence.DbQuery.Tests.Infrastructure;
// ReSharper disable StringLiteralTypo

namespace PayrollEngine.Persistence.DbQuery.Tests;

/// <summary>
/// Tests for the OData $filter clause.
///
/// Key insight: SqlKata parametrizes ALL literal values into @p0, @p1 etc.
/// - Structural assertions (column names, operators, clauses) → Sql()
/// - Value assertions (the actual numbers/strings) → Result().Bindings
/// </summary>
public class FilterTests : QueryTestBase
{
    // -------------------------------------------------------------------------
    // Comparison operators — operator in SQL, value in Bindings
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("Status eq 1",   "[Status]", "=")]
    [InlineData("Status ne 0",   "[Status]", "<>")]
    [InlineData("Amount gt 100", "[Amount]", ">")]
    [InlineData("Amount ge 100", "[Amount]", ">=")]
    [InlineData("Amount lt 50",  "[Amount]", "<")]
    [InlineData("Amount le 50",  "[Amount]", "<=")]
    public void Comparison_Operators_Structural(string filter, string expectedColumn, string expectedOp)
    {
        var sql = Sql(filter: filter);
        Assert.Contains("WHERE", sql);
        Assert.Contains(expectedColumn, sql);
        Assert.Contains(expectedOp, sql);
    }

    [Theory]
    [InlineData("Id eq 42",      "[Id]",     42)]
    [InlineData("Status eq 1",   "[Status]",  1)]
    [InlineData("Amount gt 100", "[Amount]", 100)]
    public void Comparison_Values_InBindings(string filter, string expectedColumn, object expectedValue)
    {
        var result = Result(filter: filter);
        Assert.Contains(expectedColumn, result.Sql);
        Assert.Contains(expectedValue, result.Bindings);
    }

    // -------------------------------------------------------------------------
    // String functions — LIKE pattern is in Bindings, not in SQL text
    // -------------------------------------------------------------------------

    [Fact]
    public void Contains_ProducesLike()
    {
        var result = Result(filter: "contains(Name, 'Müller')");
        Assert.Contains("like", result.Sql.ToLowerInvariant());
        // The wildcard pattern is in bindings
        var pattern = result.Bindings[0]?.ToString() ?? string.Empty;
        Assert.Contains("müller", pattern.ToLowerInvariant());
        Assert.StartsWith("%", pattern);
        Assert.EndsWith("%", pattern);
    }

    [Fact]
    public void StartsWith_ProducesLikePrefix()
    {
        var result = Result(filter: "startswith(Name, 'Hans')");
        Assert.Contains("like", result.Sql.ToLowerInvariant());
        var pattern = result.Bindings[0]?.ToString() ?? string.Empty;
        Assert.Contains("hans", pattern.ToLowerInvariant());
        Assert.EndsWith("%", pattern);
        Assert.False(pattern.StartsWith("%"), "startswith must not have leading %");
    }

    [Fact]
    public void EndsWith_ProducesLikeSuffix()
    {
        var result = Result(filter: "endswith(Name, 'AG')");
        Assert.Contains("like", result.Sql.ToLowerInvariant());
        var pattern = result.Bindings[0]?.ToString() ?? string.Empty;
        Assert.Contains("ag", pattern.ToLowerInvariant());
        Assert.StartsWith("%", pattern);
        Assert.False(pattern.EndsWith("%"), "endswith must not have trailing %");
    }

    [Fact]
    public void StringFunctions_CaseSensitiveFalse_PatternInBindings()
    {
        // WhereContains(caseSensitive:false): SqlKata 4.x passes the pattern into bindings.
        // The pattern preserves original casing — case-insensitivity is handled at DB level
        // via the SQL_Latin1_General_CP1_CS_AS collation (CI variant) or column lowering in SQL.
        var result = Result(filter: "contains(Name, 'Test')");
        Assert.Contains("like", result.Sql.ToLowerInvariant());
        // Pattern must be in bindings and contain the search term (case-insensitive)
        var pattern = result.Bindings[0]?.ToString() ?? string.Empty;
        Assert.Contains("test", pattern, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("%", pattern);
        Assert.EndsWith("%", pattern);
    }

    // -------------------------------------------------------------------------
    // notcontains: PE custom extension — NOT a standard OData function.
    // The OData parser (Microsoft.OData) rejects it at parse time.
    // It is only handled AFTER OData parsing, inside FilterClauseBuilder —
    // which is never reached. This is a documented PE API inconsistency.
    // -------------------------------------------------------------------------

    [Fact]
    public void NotContains_IsNotODataStandard_ThrowsQueryException()
    {
        // Documents that notcontains cannot be used in the OData filter string
        // because the Microsoft.OData parser rejects it before FilterClauseBuilder runs.
        // Use "not contains(...)" instead.
        Assert.Throws<QueryException>(() => Sql(filter: "notcontains(Name, 'excluded')"));
    }

    [Fact]
    public void Not_Contains_ODataStandardSyntax_ProducesNotLike()
    {
        // Correct OData syntax for "not contains"
        var withNot = Sql(filter: "not contains(Name, 'test')");
        var without = Sql(filter: "contains(Name, 'test')");
        Assert.NotEqual(withNot, without);
    }

    // -------------------------------------------------------------------------
    // Datetime functions — SqlKata 4.x renders DATEPART(FUNC, col)
    // -------------------------------------------------------------------------

    [Fact]
    public void Year_Function()
    {
        var result = Result(filter: "year(Created) eq 2024");
        Assert.Contains("DATEPART(YEAR", result.Sql);
        Assert.Contains("[Created]", result.Sql);
        Assert.Contains(2024, result.Bindings);
    }

    [Fact]
    public void Month_Function()
    {
        var result = Result(filter: "month(Created) eq 3");
        Assert.Contains("DATEPART(MONTH", result.Sql);
        Assert.Contains(3, result.Bindings);
    }

    [Fact]
    public void Day_Function()
    {
        var result = Result(filter: "day(Created) eq 15");
        Assert.Contains("DATEPART(DAY", result.Sql);
        Assert.Contains(15, result.Bindings);
    }

    [Fact]
    public void Hour_Function()
    {
        var result = Result(filter: "hour(Created) eq 9");
        Assert.Contains("DATEPART(HOUR", result.Sql);
        Assert.Contains(9, result.Bindings);
    }

    [Fact]
    public void Minute_Function()
    {
        var result = Result(filter: "minute(Created) eq 30");
        Assert.Contains("DATEPART(MINUTE", result.Sql);
        Assert.Contains(30, result.Bindings);
    }

    [Fact]
    public void Date_Function()
    {
        // WhereDate renders CAST([col] AS date) = @p0; value in bindings
        var result = Result(filter: "date(Created) eq 2024-03-15");
        Assert.Contains("[Created]", result.Sql);
        // Value is in bindings as string
        Assert.True(result.Bindings.Count > 0, "date value must be in bindings");
    }

    // -------------------------------------------------------------------------
    // Boolean logic
    // -------------------------------------------------------------------------

    [Fact]
    public void And_BothConditionsPresent()
    {
        var sql = Sql(filter: "Status eq 1 and Amount gt 100");
        Assert.Contains("[Status]", sql);
        Assert.Contains("[Amount]", sql);
        Assert.Contains("AND", sql);
        Assert.DoesNotContain(" OR ", sql);
    }

    [Fact]
    public void Or_KeywordPresent()
    {
        var sql = Sql(filter: "Status eq 1 or Status eq 2");
        Assert.Contains("OR", sql);
        Assert.Contains("[Status]", sql);
    }

    [Fact]
    public void And_Or_NestedGrouping()
    {
        var sql = Sql(filter: "(Status eq 1 or Status eq 2) and Amount gt 0");
        Assert.Contains("[Amount]", sql);
        Assert.Contains("OR", sql);
    }

    [Fact]
    public void Not_ContainsAppliesNegation()
    {
        var withNot = Sql(filter: "not contains(Name, 'test')");
        var withoutNot = Sql(filter: "contains(Name, 'test')");
        Assert.NotEqual(withNot, withoutNot);
    }

    // -------------------------------------------------------------------------
    // WHERE clause presence
    // -------------------------------------------------------------------------

    [Fact]
    public void Filter_AlwaysProducesWhereClause()
    {
        var sql = Sql(filter: "Status eq 1");
        Assert.Contains("WHERE", sql);
    }

    [Fact]
    public void NoFilter_ProducesNoWhereClause()
    {
        var sql = Sql();
        Assert.DoesNotContain("WHERE", sql);
    }

    [Fact]
    public void EmptyFilter_ProducesNoWhereClause()
    {
        var sql = Sql(filter: "");
        Assert.DoesNotContain("WHERE", sql);
    }

    // -------------------------------------------------------------------------
    // Case-insensitive field names
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("status eq 1")]
    [InlineData("STATUS eq 1")]
    [InlineData("Status eq 1")]
    public void FieldName_CaseInsensitive(string filter)
    {
        var sql = Sql(filter: filter);
        Assert.Contains("WHERE", sql);
        Assert.Contains("[Status]", sql);
    }

    // -------------------------------------------------------------------------
    // Error cases
    // -------------------------------------------------------------------------

    [Fact]
    public void UnknownField_ThrowsQueryException()
    {
        Assert.Throws<QueryException>(() => Sql(filter: "UnknownFieldXyz eq 1"));
    }
}
