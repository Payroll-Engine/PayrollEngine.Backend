using System;
using System.Data;

namespace PayrollEngine.Persistence;

public interface IDbContext
{
    /// <summary>The date time type name</summary>
    string DateTimeType { get; }

    /// <summary>The decimal type name</summary>
    string DecimalType { get; }

    /// <summary>The database connection</summary>
    IDbConnection Connection { get; }

    /// <summary>Transform a database exception</summary>
    Exception TransformException(Exception exception);
}