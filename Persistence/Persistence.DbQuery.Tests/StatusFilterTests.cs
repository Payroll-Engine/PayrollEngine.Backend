using System;
using System.Linq;
using Xunit;
using PayrollEngine.Persistence.DbQuery.Tests.Infrastructure;

namespace PayrollEngine.Persistence.DbQuery.Tests;

/// <summary>
/// Tests for the Query.Status filter path in DbQueryFactory.BuildQuery.
///
/// This is the code that Query.When() would replace:
///
///   if (query?.Status != null)
///   {
///       switch (query.Status)
///       {
///           case ObjectStatus.Active:
///               dbQuery.Where(DbSchema.ObjectColumn.Status, (int)ObjectStatus.Active);
///               break;
///           case ObjectStatus.Inactive:
///               dbQuery.Where(DbSchema.ObjectColumn.Status, (int)ObjectStatus.Inactive);
///               break;
///       }
///   }
///
/// These tests serve as the safety net before refactoring to When().
/// After the refactor, all tests must remain green.
/// </summary>
public class StatusFilterTests : QueryTestBase
{
    // -------------------------------------------------------------------------
    // No status — no Status WHERE clause
    // -------------------------------------------------------------------------

    [Fact]
    public void NoStatus_ProducesNoStatusWhere()
    {
        var sql = Sql(); // status = null (default)
        Assert.DoesNotContain("[Status]", sql);
        Assert.DoesNotContain("WHERE", sql);
    }

    [Fact]
    public void NoStatus_WithOtherFilter_StatusColumnNotDoubled()
    {
        // When an OData filter contains Status but Query.Status is null,
        // the Status column appears exactly once (from the OData filter only)
        var sql = Sql(filter: "Status eq 1");
        Assert.Contains("[Status]", sql);
        // Query.Status=null means no additional WHERE [Status] = @p added by DbQueryFactory
        // — the OData filter is the only source
        var statusCount = CountOccurrences(sql, "[Status]");
        Assert.Equal(1, statusCount);
    }

    // -------------------------------------------------------------------------
    // Active status
    // -------------------------------------------------------------------------

    [Fact]
    public void ActiveStatus_ProducesStatusWhereClause()
    {
        var sql = Sql(status: ObjectStatus.Active);
        Assert.Contains("WHERE", sql);
        Assert.Contains("[Status]", sql);
    }

    [Fact]
    public void ActiveStatus_BindsActiveValue()
    {
        var result = Result(status: ObjectStatus.Active);
        var activeValue = (int)ObjectStatus.Active; // = 0
        Assert.True(
            result.Bindings.Any(b => Convert.ToInt32(b) == activeValue),
            $"Expected Active={activeValue} in bindings. Got: [{string.Join(", ", result.Bindings)}]");
    }

    // -------------------------------------------------------------------------
    // Inactive status
    // -------------------------------------------------------------------------

    [Fact]
    public void InactiveStatus_ProducesStatusWhereClause()
    {
        var sql = Sql(status: ObjectStatus.Inactive);
        Assert.Contains("WHERE", sql);
        Assert.Contains("[Status]", sql);
    }

    [Fact]
    public void InactiveStatus_BindsInactiveValue()
    {
        var result = Result(status: ObjectStatus.Inactive);
        var inactiveValue = (int)ObjectStatus.Inactive; // = 1
        Assert.True(
            result.Bindings.Any(b => Convert.ToInt32(b) == inactiveValue),
            $"Expected Inactive={inactiveValue} in bindings. Got: [{string.Join(", ", result.Bindings)}]");
    }

    // -------------------------------------------------------------------------
    // Active vs Inactive produce different SQL
    // -------------------------------------------------------------------------

    [Fact]
    public void ActiveAndInactive_ProduceDifferentBindings()
    {
        var activeResult   = Result(status: ObjectStatus.Active);
        var inactiveResult = Result(status: ObjectStatus.Inactive);

        var activeBinding   = activeResult.Bindings.Select(Convert.ToInt32).First();
        var inactiveBinding = inactiveResult.Bindings.Select(Convert.ToInt32).First();

        Assert.NotEqual(activeBinding, inactiveBinding);
    }

    // -------------------------------------------------------------------------
    // Status combined with OData filter
    // -------------------------------------------------------------------------

    [Fact]
    public void ActiveStatus_CombinedWithODataFilter_BothConditionsPresent()
    {
        // Query.Status=Active adds a second WHERE condition on top of the OData filter
        var sql = Sql(filter: "Name eq 'test'", status: ObjectStatus.Active);
        Assert.Contains("[Status]", sql);
        Assert.Contains("[Name]", sql);
        Assert.Contains("AND", sql);
    }

    [Fact]
    public void ActiveStatus_CombinedWithODataFilter_CorrectBindingCount()
    {
        // OData filter binds 'test', Status binds 0 → 2 bindings total
        var result = Result(filter: "Name eq 'test'", status: ObjectStatus.Active);
        Assert.Equal(2, result.Bindings.Count);
    }

    // -------------------------------------------------------------------------
    // Status in count mode
    // -------------------------------------------------------------------------

    [Fact]
    public void ActiveStatus_CountMode_StillFiltersStatus()
    {
        var sql = Sql(status: ObjectStatus.Active, queryMode: QueryMode.ItemCount);
        Assert.Contains("count(*)", sql.ToLowerInvariant());
        Assert.Contains("[Status]", sql);
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private static int CountOccurrences(string text, string pattern) =>
        (text.Length - text.Replace(pattern, string.Empty).Length) / pattern.Length;
}
