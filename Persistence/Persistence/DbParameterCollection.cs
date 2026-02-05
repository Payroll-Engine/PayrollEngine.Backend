using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Dapper;

namespace PayrollEngine.Persistence;

/// <summary>
/// Database parameter collection
/// </summary>
/// <remarks>Wrapper of dapper dynamic parameters</remarks>
public class DbParameterCollection : DynamicParameters
{
    private Dictionary<string, DbType> ParameterTypes { get; } = new();

    /// <summary>
    /// Get parameter names
    /// </summary>
    /// <returns></returns>
    public List<string> GetNames() =>
        ParameterNames.ToList();

    /// <summary>
    /// Get parameter count
    /// </summary>
    public int Count => ParameterNames.Count();

    /// <summary>
    /// Test for parameters
    /// </summary>
    public bool HasAny => ParameterNames.Any();

    /// <summary>
    /// Get parameter data type
    /// </summary>
    /// <param name="name">Parameter name</param>
    public DbType? GetParameterType(string name) =>
        ParameterTypes.ContainsKey(name) ? ParameterTypes[name] : null;

    /// <summary>
    /// Add status parameter
    /// </summary>
    /// <param name="status">Object status</param>
    public void AddStatus(ObjectStatus status) =>
        Add(DbSchema.ObjectColumn.Status, status, DbType.Int32);

    /// <summary>
    /// Add created date parameter
    /// </summary>
    /// <param name="created">Created date</param>
    public void AddCreated(DateTime created) =>
        Add(DbSchema.ObjectColumn.Created, created, DbType.DateTime2);

    /// <summary>
    /// Add updated date parameter
    /// </summary>
    /// <param name="updated">Updated date</param>
    public void AddUpdated(DateTime updated) =>
        Add(DbSchema.ObjectColumn.Updated, updated, DbType.DateTime2);

    /// <summary>
    /// Add parameter
    /// </summary>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Parameter value</param>
    /// <param name="dbType">Database type</param>
    /// <param name="direction">Data direction</param>
    /// <param name="size">Data size</param>
    public new void Add(string name, object value, DbType? dbType,
        ParameterDirection? direction, int? size)
    {
        if (dbType != null && !string.IsNullOrWhiteSpace(name))
        {
            ParameterTypes.TryAdd(name, dbType.Value);
        }
        base.Add(name, value, dbType, direction, size);
    }

    /// <summary>
    /// Add parameter
    /// </summary>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Parameter value</param>
    /// <param name="dbType">Database type</param>
    /// <param name="direction">Data direction</param>
    /// <param name="size">Data size</param>
    /// <param name="precision">Numeric data precision</param>
    /// <param name="scale">Numeric data scale</param>
    public new void Add(string name, object value = null, DbType? dbType = null,
        ParameterDirection? direction = null, int? size = null,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        byte? precision = null, byte? scale = null)
    {
        if (dbType != null && !string.IsNullOrWhiteSpace(name))
        {
            ParameterTypes.TryAdd(name, dbType.Value);
        }
        base.Add(name, value, dbType, direction, size, precision, scale);
    }
}