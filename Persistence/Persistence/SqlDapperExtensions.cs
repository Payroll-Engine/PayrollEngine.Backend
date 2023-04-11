
namespace PayrollEngine.Persistence;

internal static class SqlDapperExtensions
{
    /// <summary>
    /// Convert a value from bool to int (SQL bit)
    /// Should be handled by a dapper type handler, but the TypeHandler of bool doesn't work
    /// https://github.com/StackExchange/Dapper/issues/433
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static int ToDbValue(this bool value) =>
        value ? 1 : 0;
}