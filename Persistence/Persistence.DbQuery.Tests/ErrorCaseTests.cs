using System;
using System.Linq;
using Xunit;
using PayrollEngine.Persistence.DbQuery.Tests.Infrastructure;
// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace PayrollEngine.Persistence.DbQuery.Tests;

/// <summary>
/// Regression tests for the bugs identified in the OData Review.
///
/// SqlKata parametrizes all literal values — use Result().Bindings for value assertions.
/// </summary>
public class ErrorCaseTests : QueryTestBase
{
    // -------------------------------------------------------------------------
    // Filter — WHERE clause presence (regression guard)
    // -------------------------------------------------------------------------

    [Fact]
    public void ValidFilter_ProducesWhereClause()
    {
        var sql = Sql(filter: "Status eq 1");
        Assert.Contains("WHERE", sql);
        Assert.Contains("[Status]", sql);
    }

    [Fact]
    public void AndFilter_BothColumnsInWhere()
    {
        var sql = Sql(filter: "Status eq 1 and Amount gt 0");
        Assert.Contains("[Status]", sql);
        Assert.Contains("[Amount]", sql);
        Assert.Contains("AND", sql);
        var result = Result(filter: "Status eq 1 and Amount gt 0");
        Assert.True(result.Bindings.Any(b => Convert.ToInt64(b) == 1L), "Status=1 must be in bindings");
        Assert.True(result.Bindings.Any(b => Convert.ToInt64(b) == 0L), "Amount=0 must be in bindings");
    }

    // -------------------------------------------------------------------------
    // NOT operator
    // -------------------------------------------------------------------------

    [Fact]
    public void Not_On_FunctionCall_Negates()
    {
        var withNot    = Sql(filter: "not contains(Name, 'test')");
        var withoutNot = Sql(filter: "contains(Name, 'test')");
        Assert.NotEqual(withNot, withoutNot);
    }

    [Fact]
    public void Not_On_AndExpression_Negates()
    {
        var withNot    = Sql(filter: "not (Status eq 1 and Amount gt 0)");
        var withoutNot = Sql(filter: "Status eq 1 and Amount gt 0");
        Assert.NotEqual(withNot, withoutNot);
    }

    [Fact]
    public void Not_On_Comparison_Negates()
    {
        // Visit(UnaryOperatorNode): operand.Kind == BinaryOperator → Accept() is called → NOT wirkt
        var withNot    = Sql(filter: "not (Status eq 1)");
        var withoutNot = Sql(filter: "Status eq 1");
        Assert.NotEqual(withNot, withoutNot);
    }

    // -------------------------------------------------------------------------
    // Invalid fields / syntax
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("NonExistentField eq 1")]
    [InlineData("__badfield eq 'x'")]
    [InlineData("123field eq 'x'")]
    public void InvalidField_ThrowsQueryException(string filter)
    {
        Assert.Throws<QueryException>(() => Sql(filter: filter));
    }

    [Theory]
    [InlineData("NonExistent")]
    [InlineData("__bad")]
    public void InvalidSelect_ThrowsQueryException(string select)
    {
        Assert.Throws<QueryException>(() => Sql(select: select));
    }

    [Theory]
    [InlineData("Status eq")]
    [InlineData("eq Status 1")]
    [InlineData("Status === 1")]
    public void MalformedOData_ThrowsQueryException(string filter)
    {
        Assert.Throws<QueryException>(() => Sql(filter: filter));
    }

    // -------------------------------------------------------------------------
    // Unsupported OData features
    // -------------------------------------------------------------------------

    /// <summary>
    /// Arithmetic operators are unsupported (see OData.md).
    /// The OData parser accepts them syntactically, but FilterClauseBuilder
    /// cannot resolve the column name from the BinaryOperator(Add) left node —
    /// GetColumnName returns an empty string, ValidateColumn throws ArgumentException,
    /// which is now wrapped in QueryException by QueryBuilderBase.
    /// </summary>
    [Fact]
    public void ArithmeticOperator_Add_IsNotSupported_ThrowsQueryException()
    {
        Assert.Throws<QueryException>(() => Sql(filter: "Amount add 10 eq 110"));
    }

    // -------------------------------------------------------------------------
    // Empty / null query
    // -------------------------------------------------------------------------

    [Fact]
    public void NullQuery_ProducesSelectAll()
    {
        var query = DbQueryFactory.NewQuery<TestEntity>(
            null!, TableName);
        var sql = Compiler.Compile(query).Sql;
        Assert.Contains(TableName, sql);
        Assert.DoesNotContain("WHERE", sql);
    }

    [Fact]
    public void EmptyQuery_ProducesSelectAll()
    {
        var sql = Sql();
        Assert.Contains(TableName, sql);
        Assert.DoesNotContain("WHERE", sql);
    }

    // -------------------------------------------------------------------------
    // Top/Skip boundary values
    // -------------------------------------------------------------------------

    [Fact]
    public void Top_Zero_IsIgnoredByBuilder()
    {
        // top: 0 is treated as "no limit" — FETCH clause is omitted
        var sql = Sql(top: 0);
        Assert.NotNull(sql);
        Assert.Contains(TableName, sql);
    }

    [Fact]
    public void Top_LargeValue_InBindings()
    {
        var result = Result(top: int.MaxValue);
        Assert.True(result.Bindings.Any(b => Convert.ToInt64(b) == int.MaxValue),
            $"Expected int.MaxValue in bindings. Got: [{string.Join(", ", result.Bindings)}]");
    }
}
