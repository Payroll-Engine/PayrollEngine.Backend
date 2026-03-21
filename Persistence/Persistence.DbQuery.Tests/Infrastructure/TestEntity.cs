using System;
using System.Collections.Generic;

namespace PayrollEngine.Persistence.DbQuery.Tests.Infrastructure;

/// <summary>
/// Simple typed test entity — all value-type and string properties are included as
/// valid OData columns by TypeQueryBuilder via reflection (GetPropertyTypes).
/// Deliberately does NOT implement IDomainAttributeObject; stays on the
/// TypeQueryBuilder path (no OPENJSON attribute expansion).
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

    // -------------------------------------------------------------------------
    // JSON collection columns — eligible for OData any() lambda expressions.
    // Stored as JSON arrays in the database, e.g. ["HR","Finance"].
    // TypeQueryBuilder recognises List<string> as IsJsonCollectionType → CollectionColumns.
    // -------------------------------------------------------------------------

    /// <summary>String collection stored as a JSON array — e.g. ["HR","Finance"]</summary>
    public List<string> Divisions { get; set; }

    /// <summary>String tag collection — e.g. ["payroll","active"]</summary>
    public List<string> Tags { get; set; }

    // -------------------------------------------------------------------------
    // JSON key/value column — eligible for OData any() lambda with property access.
    // Stored as a JSON object array, e.g. [{"key":"Dept","value":"HR"}].
    // TypeQueryBuilder recognises Dictionary<string, object> as a collection column.
    // -------------------------------------------------------------------------

    /// <summary>Key/value attribute map — e.g. {"Department":"HR","Level":"Senior"}</summary>
    public Dictionary<string, object> Attributes { get; set; }

    // -------------------------------------------------------------------------
    // Localization column — eligible for OData any() with culture-code key lookup.
    // Stored as a flat JSON object, e.g. {"de-CH":"Lohnart","en-US":"Wage Type"}.
    // TypeQueryBuilder recognises Dictionary<string, string> as a key-value collection column.
    // -------------------------------------------------------------------------

    /// <summary>Localized name map — e.g. {"de-CH":"Lohnart","en-US":"Wage Type"}</summary>
    public Dictionary<string, string> NameLocalizations { get; set; }
}
