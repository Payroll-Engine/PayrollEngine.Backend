using System;
using PayrollEngine.Client.Scripting;

namespace PayrollEngine.Domain.Scripting;

public static class ScriptValueConvert
{
    public static decimal? ToDecimalValue(dynamic scriptValue)
    {
        // undefined wage type value
        if (scriptValue == null)
        {
            return null;
        }

        decimal? value;
        var typeName = scriptValue.GetType().FullName;
        if (typeName.Equals(typeof(PayrollValue).FullName) ||
            typeName.Equals(typeof(PeriodValue).FullName) ||
            typeName.Equals(typeof(CasePayrollValue).FullName))
        {
            // result from payroll value
            value = scriptValue.Value as decimal?;
        }
        else if (scriptValue is decimal decimalValue)
        {
            // result from decimal value
            value = decimalValue;
        }
        else
        {
            // convert to decimal value
            try
            {
                value = Convert.ToDecimal(scriptValue);
            }
            catch (Exception exception)
            {
                throw new ScriptException($"Error converting type {scriptValue.GetType()} to decimal.", exception);
            }
        }

        // result
        return value;
    }
}