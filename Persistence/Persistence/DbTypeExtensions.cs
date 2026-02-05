using System;
using System.Data;

namespace PayrollEngine.Persistence;

/// <summary>
/// Extension methods for the <see cref="DbType"/>
/// </summary>
internal static class DbTypeExtensions
{
    /// <summary>
    /// Convert database type to system type
    /// </summary>
    /// <param name="dbType">Database type</param>
    internal static Type ToSystemType(this DbType dbType)
    {
        switch (dbType)
        {
            case DbType.String:
            case DbType.AnsiString:
                return typeof(string);
            case DbType.Int32:
                return typeof(int);
            case DbType.Decimal:
                return typeof(decimal);
            case DbType.DateTime2:
                return typeof(DateTime);
            case DbType.Boolean:
                return typeof(bool);
            case DbType.Int64:
                return typeof(long);
            case DbType.Byte:
                return typeof(byte);
            case DbType.Binary:
                return typeof(byte[]);
            case DbType.Guid:
                return typeof(Guid);
            default:
                throw new PayrollException($"Unsupported database type {dbType}.");
        }
    }
}