using System;
using Xunit;
using PayrollEngine.Persistence.DbQuery.Tests.Infrastructure;
// ReSharper disable StringLiteralTypo

namespace PayrollEngine.Persistence.DbQuery.Tests;

/// <summary>
/// Tests for the OData $apply clause via Query.Apply.
///
/// Supported transformations (see ApplyClauseBuilder):
///   groupby((Field))
///   groupby((Field), aggregate(Field with sum as Alias))
///   aggregate(Field with sum as Alias)
///   aggregate($count as Count)
///   filter(Field eq Value)/groupby(...)
/// </summary>
public class ApplyTests : QueryTestBase
{
    // -------------------------------------------------------------------------
    // Aggregate functions
    // -------------------------------------------------------------------------

    [Fact]
    public void Aggregate_Count_ProducesCountSql()
    {
        var sql = Sql(apply: "aggregate($count as Count)");
        Assert.Contains("COUNT(1)", sql);
        Assert.Contains("Count", sql);
    }

    [Fact]
    public void Aggregate_Sum_ProducesSumSql()
    {
        // SelectRaw preserves the OData-parser casing (Sum, not SUM) — compare case-insensitive
        var sql = Sql(apply: "aggregate(Amount with sum as TotalAmount)");
        Assert.Contains("sum(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("TotalAmount", sql);
    }

    [Fact]
    public void Aggregate_Average_ProducesAvgSql()
    {
        var sql = Sql(apply: "aggregate(Amount with average as AvgAmount)");
        Assert.Contains("avg(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AvgAmount", sql);
    }

    [Fact]
    public void Aggregate_Min_ProducesMinSql()
    {
        var sql = Sql(apply: "aggregate(Amount with min as MinAmount)");
        Assert.Contains("min(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("MinAmount", sql);
    }

    [Fact]
    public void Aggregate_Max_ProducesMaxSql()
    {
        var sql = Sql(apply: "aggregate(Amount with max as MaxAmount)");
        Assert.Contains("max(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("MaxAmount", sql);
    }

    [Fact]
    public void Aggregate_CountDistinct_ProducesCountDistinctSql()
    {
        var sql = Sql(apply: "aggregate(Status with countdistinct as UniqueStatuses)");
        Assert.Contains("COUNT(DISTINCT", sql);
        Assert.Contains("UniqueStatuses", sql);
    }

    // -------------------------------------------------------------------------
    // GroupBy
    // -------------------------------------------------------------------------

    [Fact]
    public void GroupBy_ProducesGroupBySql()
    {
        var sql = Sql(apply: "groupby((Status))");
        Assert.Contains("GROUP BY", sql);
        Assert.Contains("[Status]", sql);
    }

    [Fact]
    public void GroupBy_WithAggregate_ProducesGroupByAndAggregate()
    {
        var sql = Sql(apply: "groupby((Status), aggregate($count as Count))");
        Assert.Contains("GROUP BY", sql);
        Assert.Contains("[Status]", sql);
        Assert.Contains("COUNT(1)", sql);
        Assert.Contains("Count", sql);
    }

    [Fact]
    public void GroupBy_MultipleFields_ProducesMultipleGroupByColumns()
    {
        var sql = Sql(apply: "groupby((Status, Active))");
        Assert.Contains("GROUP BY", sql);
        Assert.Contains("[Status]", sql);
        Assert.Contains("[Active]", sql);
    }

    // -------------------------------------------------------------------------
    // Filter + GroupBy / Aggregate
    // -------------------------------------------------------------------------

    [Fact]
    public void Filter_Then_Aggregate_FilterAppliedBeforeAggregate()
    {
        var sql = Sql(apply: "filter(Status eq 1)/aggregate($count as Count)");
        Assert.Contains("WHERE", sql);
        Assert.Contains("[Status]", sql);
        Assert.Contains("COUNT(1)", sql);
    }

    [Fact]
    public void Filter_Then_GroupBy_FilterAppliedBeforeGroupBy()
    {
        var sql = Sql(apply: "filter(Status eq 1)/groupby((Active))");
        Assert.Contains("WHERE", sql);
        Assert.Contains("GROUP BY", sql);
    }

    // -------------------------------------------------------------------------
    // Apply does not interfere with Count mode
    // -------------------------------------------------------------------------

    [Fact]
    public void CountAggregation_ViaCountMode_StillWorks()
    {
        var sql = Sql(filter: "Status eq 1", queryMode: QueryMode.ItemCount);
        Assert.Contains("count(*)", sql.ToLowerInvariant());
    }

    // -------------------------------------------------------------------------
    // Apply with additional filter (post-apply)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Post-apply $filter on a column that was part of the groupby works:
    /// OData only allows filtering on grouped columns after the apply clause.
    /// </summary>
    [Fact]
    public void Apply_WithGroupedColumnFilter_BothPresent()
    {
        // filter on a column that was grouped — OData allows this
        var sql = Sql(
            apply: "groupby((Status, Active))",
            filter: "Active eq true");
        Assert.Contains("GROUP BY", sql);
        Assert.Contains("WHERE", sql);
    }

    /// <summary>
    /// Post-apply filters on aggregate aliases (e.g. Count) are NOT supported.
    /// TypeQueryBuilder validates filter columns against the entity schema —
    /// aggregate aliases produced by $apply are not part of the entity schema
    /// and therefore cause a QueryException.
    /// To filter on aggregate results, use a separate query or
    /// the HAVING clause pattern (not currently supported).
    /// </summary>
    [Fact]
    public void Apply_WithAggregateAliasFilter_ThrowsQueryException()
    {
        Assert.Throws<QueryException>(() => Sql(
            apply: "groupby((Status), aggregate($count as Count))",
            filter: "Count gt 0"));
    }

    // -------------------------------------------------------------------------
    // SqlKata aggregate unit tests (independent of OData layer)
    // -------------------------------------------------------------------------

    [Fact]
    public void SqlKata_GroupByAggregate_ProducesExpectedSql()
    {
        var query = new SqlKata.Query(TableName)
            .Select("Status")
            .GroupBy("Status")
            .SelectRaw("COUNT(1) AS Count");

        var sql = Compiler.Compile(query).Sql;
        Assert.Contains("GROUP BY", sql);
        Assert.Contains("[Status]", sql);
        Assert.Contains("COUNT(1)", sql);
    }

    [Fact]
    public void SqlKata_SubQuery_WrapsProperly()
    {
        var inner = new SqlKata.Query(TableName)
            .Select("Status")
            .GroupBy("Status");

        var outer = new SqlKata.Query().From(inner)
            .SelectRaw("COUNT(1) AS Count");

        var sql = Compiler.Compile(outer).Sql;
        Assert.Contains("GROUP BY", sql);
        Assert.Contains("COUNT(1)", sql);
    }
}
