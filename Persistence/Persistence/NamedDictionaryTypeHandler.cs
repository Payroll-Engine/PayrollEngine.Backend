using System.Collections.Generic;
using System.Data;
using Dapper;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class NamedDictionaryTypeHandler<TValue> : SqlMapper.TypeHandler<Dictionary<string, TValue>>
{
    public override void SetValue(IDbDataParameter parameter, Dictionary<string, TValue> value)
    {
        // suppress 'null' text for empty dictionaries
        parameter.Value = JsonSerializer.SerializeNamedDictionary(value);
    }

    public override Dictionary<string, TValue> Parse(object value)
    {
        var json = value as string;
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }
        // replace MS JsonSerializer.Deserialize<Dictionary<string,object>>()
        // it deserializes the json { "myKey": 86 } into the dictionary item
        // key: myKey
        // value: ValueKind = Number : "86"
        // and not
        // key: myKey
        // value: 86
        // see also https://github.com/dotnet/runtime/issues/30524#issuecomment-539704342
        return JsonSerializer.DeserializeNamedDictionary<TValue>(json);
    }
}