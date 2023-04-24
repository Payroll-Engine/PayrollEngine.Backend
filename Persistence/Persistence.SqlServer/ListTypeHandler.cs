using System.Collections.Generic;
using System.Data;
using Dapper;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence.SqlServer;

public class ListTypeHandler<T> : SqlMapper.TypeHandler<List<T>>
{
    public override void SetValue(IDbDataParameter parameter, List<T> value)
    {
        parameter.Value = JsonSerializer.SerializeList(value);
    }

    public override List<T> Parse(object value)
    {
        var json = value as string;
        return string.IsNullOrWhiteSpace(json) ?
            null :
            JsonSerializer.DeserializeList<T>(json);
    }
}