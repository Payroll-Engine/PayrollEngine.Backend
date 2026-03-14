using System;
using System.Linq;
using Xunit;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence.DbQuery;
using PayrollEngine.Persistence.DbQuery.Tests.Infrastructure;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence.DbQuery.Tests;

/// <summary>
/// Tests for the attribute-column path (IDomainAttributeObject / OPENJSON).
/// Uses Employee as the test type because it correctly implements IDomainAttributeObject
/// and has known typed fields (Identifier, FirstName, LastName, Culture, Calendar)
/// plus attribute columns via the configured attribute prefixes.
///
/// Key assertions:
/// - Regular Employee field filters → standard [Column] WHERE
/// - Attribute field filters → OPENJSON sub-select in FROM
/// - Attribute column name injection → QueryException
/// </summary>
public class AttributeQueryTests : QueryTestBase
{
    private static readonly StubDbContext DbCtx = new();

    // The attribute prefixes configured in PE (read at runtime to stay in sync)
    private static string TextPrefix => Prefixes.AttributePrefixes[0];
    private static string DatePrefix => Prefixes.AttributePrefixes[1];
    private static string NumericPrefix => Prefixes.AttributePrefixes[2];

    // -------------------------------------------------------------------------
    // Regular typed fields on Employee — must NOT produce OPENJSON
    // -------------------------------------------------------------------------

    [Fact]
    public void TypedField_Identifier_ProducesStandardWhere()
    {
        var sql = Sql<Employee>(DbCtx, filter: "Identifier eq 'E001'");
        Assert.Contains("WHERE", sql);
        Assert.Contains("[Identifier]", sql);
        Assert.DoesNotContain("OPENJSON", sql);
    }

    [Fact]
    public void TypedField_FirstName_Contains()
    {
        var sql = Sql<Employee>(DbCtx, filter: "contains(FirstName, 'Hans')");
        Assert.Contains("like", sql.ToLowerInvariant());
        Assert.DoesNotContain("OPENJSON", sql);
    }

    [Fact]
    public void TypedField_Id_Comparison()
    {
        // Values are parametrized — assert column/operator in SQL, value in Bindings
        var result = Result<Employee>(DbCtx, filter: "Id gt 100");
        Assert.Contains("[Id]", result.Sql);
        Assert.Contains(">", result.Sql);
        Assert.True(result.Bindings.Any(b => Convert.ToInt64(b) == 100L), "Id=100 must be in bindings");
        Assert.DoesNotContain("OPENJSON", result.Sql);
    }

    // -------------------------------------------------------------------------
    // Attribute fields — must produce OPENJSON sub-select
    // -------------------------------------------------------------------------

    [Fact]
    public void TextAttribute_Filter_ProducesOpenJson()
    {
        // e.g. "textAttributes.Department eq 'IT'"
        var attributeColumn = $"{TextPrefix}Department";
        var filter = $"{attributeColumn} eq 'IT'";

        var sql = Sql<Employee>(DbCtx, filter: filter);

        Assert.Contains("OPENJSON", sql);
        Assert.Contains("Department", sql);
    }

    [Fact]
    public void TextAttribute_Select_ProducesOpenJson()
    {
        // Selecting an attribute column forces OPENJSON into the FROM
        var attributeColumn = $"{TextPrefix}CostCenter";
        var sql = Sql<Employee>(DbCtx, select: $"Id, {attributeColumn}");

        Assert.Contains("OPENJSON", sql);
        Assert.Contains("CostCenter", sql);
    }

    [Fact]
    public void DateAttribute_Select_IncludesDateTimeCast()
    {
        // Date attributes use dbContext.DateTimeType for CAST
        var attributeColumn = $"{DatePrefix}HireDate";
        var sql = Sql<Employee>(DbCtx, select: $"Id, {attributeColumn}");

        Assert.Contains("OPENJSON", sql);
        // StubDbContext returns DATETIME2(7)
        Assert.Contains("DATETIME2(7)", sql);
    }

    [Fact]
    public void NumericAttribute_Select_IncludesDecimalCast()
    {
        // Numeric attributes use dbContext.DecimalType for CAST
        var attributeColumn = $"{NumericPrefix}Salary";
        var sql = Sql<Employee>(DbCtx, select: $"Id, {attributeColumn}");

        Assert.Contains("OPENJSON", sql);
        // StubDbContext returns DECIMAL(28, 6)
        Assert.Contains("DECIMAL(28, 6)", sql);
    }

    // -------------------------------------------------------------------------
    // SQL-Injection guard — documented as current vulnerability (OData Review §1)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Injection attempts via attribute names are blocked by the OData parser.
    /// Characters like ' ; -- are not valid OData identifier characters — the parser
    /// rejects them before BuildAttributeQuery is ever reached.
    /// BuildAttributeQuery still has no explicit whitelist validation, but the OData
    /// parser provides effective accidental protection for filter/select paths.
    /// Residual risk: a raw call to BuildAttributeQuery bypassing the OData parser.
    /// </summary>
    [Fact]
    public void AttributeName_SqlInjection_BlockedByODataParser()
    {
        var maliciousColumn = $"{TextPrefix}'; DROP TABLE Employee;--";
        var filter = $"{maliciousColumn} eq 'x'";

        // OData parser rejects the malicious identifier → QueryException thrown
        // (from OData parse error, not from BuildAttributeQuery validation)
        Assert.Throws<QueryException>(() => Sql<Employee>(DbCtx, filter: filter));
    }

    // -------------------------------------------------------------------------
    // No attribute columns → no OPENJSON (fast path)
    // -------------------------------------------------------------------------

    [Fact]
    public void NoAttributeColumns_SkipsOpenJsonPath()
    {
        // A query with only typed Employee fields must not produce OPENJSON
        var sql = Sql<Employee>(DbCtx, filter: "Id eq 1");
        Assert.DoesNotContain("OPENJSON", sql);
    }

    // -------------------------------------------------------------------------
    // Count mode with attribute query
    // -------------------------------------------------------------------------

    [Fact]
    public void AttributeQuery_CountMode_WrapsInCountSubQuery()
    {
        var attributeColumn = $"{TextPrefix}Department";
        var sql = Sql<Employee>(DbCtx,
            filter: $"{attributeColumn} eq 'HR'",
            queryMode: QueryMode.ItemCount);

        Assert.Contains("count(*)", sql.ToLowerInvariant());
    }
}
