using System;
using System.Data;
using System.Text.Json;
using Dapper;

namespace PayrollEngine.Persistence;

public class JsonObjectTypeHandler<T> : SqlMapper.TypeHandler<T> where T : class
{
    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.Value = value == null ?
            DBNull.Value :
            JsonSerializer.Serialize(value);
        parameter.DbType = DbType.String;
    }

    public override T Parse(object value)
    {
        var json = value as string;
        return string.IsNullOrWhiteSpace(json) ?
            null :
            JsonSerializer.Deserialize<T>(json);
    }
}