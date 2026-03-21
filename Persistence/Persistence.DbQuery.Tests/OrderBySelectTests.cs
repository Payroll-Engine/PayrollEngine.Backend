using System;
using Xunit;
using PayrollEngine.Persistence.DbQuery.Tests.Infrastructure;
using System.Linq;

namespace PayrollEngine.Persistence.DbQuery.Tests;

/// <summary>
/// Tests for $orderby, $select, $top, $skip and count mode.
///
/// Top/Skip values land in .Bindings (OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY),
/// not literally in the SQL string.
/// </summary>
public class OrderBySelectTests : QueryTestBase
{
    // -------------------------------------------------------------------------
    // $orderby
    // -------------------------------------------------------------------------

    [Fact]
    public void OrderBy_Ascending()
    {
        var sql = Sql(orderBy: "Name asc");
        Assert.Contains("ORDER BY", sql);
        Assert.Contains("[Name]", sql);
        Assert.DoesNotContain("DESC", sql);
    }

    [Fact]
    public void OrderBy_Descending()
    {
        var sql = Sql(orderBy: "Name desc");
        Assert.Contains("ORDER BY", sql);
        Assert.Contains("[Name]", sql);
        Assert.Contains("DESC", sql);
    }

    [Fact]
    public void OrderBy_MultipleColumns()
    {
        var sql = Sql(orderBy: "Status asc, Created desc");
        Assert.Contains("[Status]", sql);
        Assert.Contains("[Created]", sql);
        Assert.Contains("DESC", sql);
    }

    [Fact]
    public void OrderBy_IgnoredInCountMode()
    {
        var sql = Sql(orderBy: "Name asc", queryMode: QueryMode.ItemCount);
        Assert.DoesNotContain("ORDER BY", sql);
    }

    // -------------------------------------------------------------------------
    // $select
    // -------------------------------------------------------------------------

    [Fact]
    public void Select_SingleColumn()
    {
        var sql = Sql(select: "Name");
        Assert.Contains("[Name]", sql);
    }

    [Fact]
    public void Select_MultipleColumns()
    {
        var sql = Sql(select: "Id, Name, Status");
        Assert.Contains("[Id]", sql);
        Assert.Contains("[Name]", sql);
        Assert.Contains("[Status]", sql);
    }

    [Fact]
    public void Select_UnknownColumn_ThrowsQueryException()
    {
        Assert.Throws<QueryException>(() => Sql(select: "NonExistentColumn"));
    }

    [Fact]
    public void Select_IgnoredInCountMode()
    {
        var sql = Sql(select: "Name", queryMode: QueryMode.ItemCount);
        Assert.Contains("count(*)", sql.ToLowerInvariant());
    }

    // -------------------------------------------------------------------------
    // $top / $skip — values in .Bindings (OFFSET/FETCH syntax), not in SQL text
    // -------------------------------------------------------------------------

    [Fact]
    public void Top_LimitsRows()
    {
        // SqlKata SQL Server compiler uses OFFSET/FETCH: value is in bindings
        var result = Result(top: 25);
        Assert.True(result.Bindings.Contains(25),
            $"Expected 25 in bindings but got: [{string.Join(", ", result.Bindings)}]");
    }

    [Fact]
    public void Skip_OffsetRows()
    {
        // Bindings may contain long or int depending on SqlKata version — compare via Convert
        var result = Result(skip: 50);
        Assert.True(result.Bindings.Any(b => Convert.ToInt64(b) == 50L),
            $"Expected 50 in bindings but got: [{string.Join(", ", result.Bindings)}]");
    }

    [Fact]
    public void Top_And_Skip_BothPresent()
    {
        var result = Result(top: 10, skip: 20);
        Assert.True(result.Bindings.Any(b => Convert.ToInt64(b) == 10L),
            $"Expected top=10 in bindings but got: [{string.Join(", ", result.Bindings)}]");
        Assert.True(result.Bindings.Any(b => Convert.ToInt64(b) == 20L),
            $"Expected skip=20 in bindings but got: [{string.Join(", ", result.Bindings)}]");
    }

    [Fact]
    public void Top_IgnoredInCountMode()
    {
        var sql = Sql(top: 5, queryMode: QueryMode.ItemCount);
        Assert.Contains("count(*)", sql.ToLowerInvariant());
        Assert.DoesNotContain("FETCH", sql);
    }

    [Fact]
    public void Skip_IgnoredInCountMode()
    {
        var sql = Sql(skip: 100, queryMode: QueryMode.ItemCount);
        Assert.Contains("count(*)", sql.ToLowerInvariant());
        Assert.DoesNotContain("OFFSET", sql);
    }

    // -------------------------------------------------------------------------
    // Count mode
    // -------------------------------------------------------------------------

    [Fact]
    public void CountMode_ProducesCountStar()
    {
        var sql = Sql(queryMode: QueryMode.ItemCount);
        Assert.Contains("count(*)", sql.ToLowerInvariant());
    }

    [Fact]
    public void CountMode_WithFilter()
    {
        var sql = Sql(filter: "Status eq 1", queryMode: QueryMode.ItemCount);
        Assert.Contains("count(*)", sql.ToLowerInvariant());
        Assert.Contains("WHERE", sql);
    }

    // -------------------------------------------------------------------------
    // Combined
    // -------------------------------------------------------------------------

    [Fact]
    public void Filter_OrderBy_Select_Combined()
    {
        var sql = Sql(
            filter: "Status eq 1",
            orderBy: "Name asc",
            select: "Id, Name");

        Assert.Contains("WHERE", sql);
        Assert.Contains("ORDER BY", sql);
        Assert.Contains("[Id]", sql);
        Assert.Contains("[Name]", sql);
    }

    [Fact]
    public void Filter_OrderBy_Select_Top_Combined()
    {
        var result = Result(
            filter: "Status eq 1",
            orderBy: "Name asc",
            select: "Id, Name",
            top: 10);

        Assert.Contains("WHERE", result.Sql);
        Assert.Contains("ORDER BY", result.Sql);
        Assert.Contains("[Id]", result.Sql);
        Assert.Contains("[Name]", result.Sql);
        Assert.True(result.Bindings.Contains(10),
            $"Expected top=10 in bindings but got: [{string.Join(", ", result.Bindings)}]");
    }
}
