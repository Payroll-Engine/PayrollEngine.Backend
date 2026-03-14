using System;

namespace PayrollEngine.Persistence.DbQuery.Tests.Infrastructure;

/// <summary>
/// Simple typed test entity — all value-type and string properties are included as
/// valid OData columns by TypeQueryBuilder via reflection (GetPropertyTypes).
/// Deliberately does NOT implement IDomainAttributeObject; stays on the
/// TypeQueryBuilder path (no OPENJSON).
/// </summary>
public class TestEntity
{
    /// <summary>Primary key (int)</summary>
    public int Id { get; set; }

    /// <summary>Name column (string)</summary>
    public string Name { get; set; }

    /// <summary>Integer status column</summary>
    public int Status { get; set; }

    /// <summary>Decimal money column</summary>
    public decimal Amount { get; set; }

    /// <summary>DateTime column</summary>
    public DateTime Created { get; set; }

    /// <summary>Boolean column</summary>
    public bool Active { get; set; }
}
