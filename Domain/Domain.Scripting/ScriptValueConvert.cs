using System;
using System.Linq;
using System.Collections.Concurrent;
using PayrollEngine.Client.Scripting;

namespace PayrollEngine.Domain.Scripting;

public static class ScriptValueConvert
{
    private static ConcurrentDictionary<Type, bool> ValueTypes { get; } = new();

    public static decimal? ToDecimalValue(dynamic scriptValue)
    {
        // undefined wage type value
        if (scriptValue == null)
        {
            return null;
        }

        // decimal value
        if (scriptValue is decimal decimalValue)
        {
            // result from decimal value
            return decimalValue;
        }

        // value object
        if (IsValueType(((object)scriptValue).GetType()))
        {
            return scriptValue.Value as decimal?;
        }

        // convert to decimal value
        try
        {
            return Convert.ToDecimal(scriptValue);
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Error converting type {scriptValue.GetType()} to decimal.", exception);
        }
    }

    private static bool IsValueType(Type type) =>
        ValueTypes.GetOrAdd(type, t => t.GetProperties().Any(x => string.Equals(x.Name, "Value")));
}