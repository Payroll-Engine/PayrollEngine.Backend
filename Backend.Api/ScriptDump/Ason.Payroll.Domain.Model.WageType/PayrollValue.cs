/* PayrollValue */
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ason.Payroll.Client.Scripting;

/// <summary>JSON converter for <see cref="PayrollValue"/></summary>
public class PayrollValueConverter : JsonConverter<PayrollValue>
{
    /// <summary>Read and convert the JSON to T</summary>
    /// <remarks>A converter may throw any Exception, but should throw <cref>JsonException</cref> when the JSON is invalid</remarks>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from</param>
    /// <param name="typeToConvert">The <see cref="Type"/> being converted</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> being used</param>
    /// <returns>The value that was converted</returns>
    public override PayrollValue Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value != null ? new PayrollValue(JsonSerializer.Deserialize<object>(value)) : null;
    }

    /// <summary>Write the value as JSON</summary>
    /// <remarks>A converter may throw any Exception, but should throw <cref>JsonException</cref> when the JSON cannot be created</remarks>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to</param>
    /// <param name="value">The value to convert</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> being used</param>
    public override void Write(Utf8JsonWriter writer, PayrollValue value,
        JsonSerializerOptions options) =>
        writer.WriteStringValue(JsonSerializer.Serialize(value.Value));
}

/// <summary>Payroll value</summary>
[JsonConverter(typeof(PayrollValueConverter))]
public class PayrollValue
{
    /// <summary>New payroll value instance</summary>
    public static readonly PayrollValue Empty = new();

    /// <summary>Initializes a new instance of the <see cref="PayrollValue"/> class</summary>
    protected PayrollValue()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="PayrollValue"/> class</summary>
    /// <param name="value">The value</param>
    public PayrollValue(object value)
    {
        this.CurrentValue = value;
    }

    /// <summary>Initializes a new instance of the <see cref="PayrollValue"/> class</summary>
    /// <param name="source">The source payroll value</param>
    public PayrollValue(PayrollValue source)
    {
        CurrentValue = source.Value;
    }

    /// <summary>The original value</summary>
    public object Value => CurrentValue;
    /// <summary>The value query</summary>
    protected virtual object CurrentValue { get; }

    /// <summary>Test if the payroll value is defined</summary>
    [JsonIgnore]
    public virtual bool HasValue => Value != null;

    /// <summary>Test if the payroll value is numeric</summary>
    [JsonIgnore]
    public virtual bool IsNumeric => Value != null && Value.GetType().IsNumericType();

    #region Casting operators

    /// <summary>Convert payroll value to string/></summary>
    public static implicit operator string(PayrollValue value) =>
        value?.Value as string;

    /// <summary>Convert payroll value to int/></summary>
    public static implicit operator int(PayrollValue value) =>
        (int?)value?.Value ?? default;

    /// <summary>Convert payroll value to nullable int</summary>
    public static implicit operator int?(PayrollValue value) =>
        (int?)value?.Value;

    /// <summary>Convert payroll value to decimal</summary>
    public static implicit operator decimal(PayrollValue value) =>
        (decimal?)value?.Value ?? default;

    /// <summary>Convert payroll value to nullable decimal</summary>
    public static implicit operator decimal?(PayrollValue value) =>
        (decimal?)value?.Value;

    /// <summary>Convert payroll value to DateTime</summary>
    public static implicit operator DateTime(PayrollValue value)
    {
        if (value != null && value.HasValue)
        {
            return (DateTime)value.Value;
        }
        return Date.MinValue;
    }

    /// <summary>Convert payroll value to nullable DateTime</summary>
    public static implicit operator DateTime?(PayrollValue value) =>
        (DateTime?)value?.Value;

    /// <summary>Convert payroll value to bool</summary>
    public static implicit operator bool(PayrollValue value) =>
        (bool?)value?.Value ?? default;

    /// <summary>Convert payroll value to nullable bool</summary>
    public static implicit operator bool?(PayrollValue value) =>
        (bool?)value?.Value;

    #endregion

    #region Unary operators

    /// <summary>Unary plus of a payroll value (decimal, int)</summary>
    public static PayrollValue operator +(PayrollValue value)
    {
        // decimal
        if (value.Value is decimal @decimal)
        {
            return new(+@decimal);
        }
        // int
        if (value.Value is int @int)
        {
            return new(+@int);
        }
        throw new ScriptException($"operator unary + error in payroll value {value}");
    }

    /// <summary>Unary minus of a payroll value (decimal, int)</summary>
    public static PayrollValue operator -(PayrollValue value)
    {
        // decimal
        if (value.Value is decimal @decimal)
        {
            return new(-@decimal);
        }
        // int
        if (value.Value is int @int)
        {
            return new(-@int);
        }
        throw new ScriptException($"operator unary - error in payroll value {value}");
    }

    /// <summary>Logical negation of a payroll value (bool)</summary>
    public static PayrollValue operator !(PayrollValue value)
    {
        // bool
        if (value.Value is bool boolValue)
        {
            return new(!boolValue);
        }

        throw new ScriptException($"operator ! error in payroll value {value}");
    }

    /// <summary>Test if payroll value is true</summary>
    public static bool operator true(PayrollValue value)
    {
        if (value.Value is bool @bool)
        {
            return @bool;
        }
        return false;
    }

    /// <summary>Test if payroll value is false</summary>
    public static bool operator false(PayrollValue value)
    {
        if (value.Value is bool @bool)
        {
            return !@bool;
        }
        return true;
    }

    #endregion

    #region Binary operators

    /// <summary>Addition of two payroll values (decimal, int, string)</summary>
    public static PayrollValue operator +(PayrollValue left, PayrollValue right)
    {
        if (!left.HasValue)
        {
            return right;
        }
        if (!right.HasValue)
        {
            return left;
        }

        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch (left.Value)
        {
            // decimal + decimal
            case decimal leftDecimal when right.Value is decimal rightDecimal:
                return new(leftDecimal + rightDecimal);
            // decimal + int
            case decimal leftDecimal when right.Value is int rightInt:
                return new(leftDecimal + rightInt);
            // int + int
            case int leftInt when right.Value is int rightInt:
                return new(leftInt + rightInt);
            // int + decimal
            case int leftInt when right.Value is decimal rightDecimal:
                return new(leftInt + rightDecimal);
            // string + string
            case string leftString when right.Value is string rightString:
                return new(leftString + rightString);
            default:
                throw new ScriptException($"operator + error in payroll values {left} and {right}");
        }
    }

    /// <summary>Subtraction of two payroll values (decimal, int)</summary>
    public static PayrollValue operator -(PayrollValue left, PayrollValue right)
    {
        // null
        if (!left.HasValue || !right.HasValue)
        {
            return left;
        }

        return left.Value switch
        {
            // decimal - decimal
            decimal leftDecimal when right.Value is decimal rightDecimal => new(leftDecimal - rightDecimal),
            // decimal - int
            decimal leftDecimal when right.Value is int rightInt => new(leftDecimal - rightInt),
            // int - int
            int leftInt when right.Value is int rightInt => new(leftInt - rightInt),
            // int - decimal
            int leftInt when right.Value is decimal rightDecimal => new(leftInt - rightDecimal),
            _ => throw new ScriptException($"operator - error in payroll values {left} and {right}")
        };
    }

    /// <summary>Multiplication of two payroll values (decimal, int)</summary>
    public static PayrollValue operator *(PayrollValue left, PayrollValue right)
    {
        // null
        if (!left.HasValue || !right.HasValue)
        {
            return Empty;
        }

        return left.Value switch
        {
            // decimal * decimal
            decimal leftDecimal when right.Value is decimal rightDecimal => new(leftDecimal * rightDecimal),
            // decimal * int
            decimal leftDecimal when right.Value is int rightInt => new(leftDecimal * rightInt),
            // int * int
            int leftInt when right.Value is int rightInt => new(leftInt * rightInt),
            // int * decimal
            int leftInt when right.Value is decimal rightDecimal => new(leftInt * rightDecimal),
            _ => throw new ScriptException($"operator * error in payroll values {left} and {right}")
        };
    }

    /// <summary>Division of two payroll values (decimal, int)</summary>
    public static PayrollValue operator /(PayrollValue left, PayrollValue right)
    {
        // null
        if (!left.HasValue)
        {
            return left;
        }
        if (!right.HasValue)
        {
            throw new ScriptException("division with undefined payroll value");
        }

        return left.Value switch
        {
            // decimal / decimal
            decimal leftDecimal when right.Value is decimal rightDecimal => new(leftDecimal / rightDecimal),
            // decimal / int
            decimal leftDecimal when right.Value is int rightInt => new(leftDecimal / rightInt),
            // int / int
            int leftInt when right.Value is int rightInt => new(leftInt / rightInt),
            // int / decimal
            int leftInt when right.Value is decimal rightDecimal => new(leftInt / rightDecimal),
            _ => throw new ScriptException($"operator / error in payroll values {left} and {right}")
        };
    }

    /// <summary>Remainder of two payroll values (decimal, int)</summary>
    public static PayrollValue operator %(PayrollValue left, PayrollValue right)
    {
        // null
        if (!left.HasValue)
        {
            return left;
        }
        if (!right.HasValue)
        {
            throw new ScriptException("operator % error; remainder by empty payroll value");
        }

        return left.Value switch
        {
            // decimal % decimal
            decimal leftDecimal when right.Value is decimal rightDecimal => new(leftDecimal % rightDecimal),
            // decimal % int
            decimal leftDecimal when right.Value is int rightInt => new(leftDecimal % rightInt),
            // int % int
            int leftInt when right.Value is int rightInt => new(leftInt % rightInt),
            // int % int
            int leftInt when right.Value is decimal rightDecimal => new(leftInt % rightDecimal),
            _ => throw new ScriptException($"operator % error in payroll values {left} and {right}")
        };
    }

    /// <summary>Test if two payroll values are true (bool)</summary>
    public static PayrollValue operator &(PayrollValue left, PayrollValue right)
    {
        // null
        if (!left.HasValue)
        {
            return left;
        }
        if (!right.HasValue)
        {
            return right;
        }

        // bool & bool
        if (left.Value is bool leftBoolean && right.Value is bool rightBoolean)
        {
            return new(leftBoolean & rightBoolean);
        }
        throw new ScriptException($"operator & error in payroll values {left} and {right}");
    }

    /// <summary>Test if any payroll value is true (bool)</summary>
    public static PayrollValue operator |(PayrollValue left, PayrollValue right)
    {
        // null
        if (!left.HasValue)
        {
            return new(right.Value is bool rightValue && rightValue);
        }
        if (!right.HasValue)
        {
            return new(left.Value is bool leftValue && leftValue);
        }

        // bool | bool
        if (left.Value is bool leftBoolean && right.Value is bool rightBoolean)
        {
            return new(leftBoolean | rightBoolean);
        }
        throw new ScriptException($"operator | error in payroll values {left} and {right}");
    }

    /// <summary>Compare two payroll values for equality</summary>
    public static bool operator ==(PayrollValue left, PayrollValue right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }
        if (left is null || right is null)
        {
            return false;
        }
        return left.Equals(right);
    }

    /// <summary>Compare two payroll values for difference</summary>
    public static bool operator !=(PayrollValue left, PayrollValue right) =>
        !(left == right);

    /// <summary>Compare if a payroll value is less than another payroll value (decimal, int)</summary>
    public static PayrollValue operator <(PayrollValue left, PayrollValue right)
    {
        // null
        if (!left.HasValue)
        {
            return left;
        }
        if (!right.HasValue)
        {
            return right;
        }

        return left.Value switch
        {
            // decimal < decimal
            decimal leftDecimal when right.Value is decimal rightDecimal => new(leftDecimal < rightDecimal),
            // decimal < int
            decimal leftDecimal when right.Value is int rightInt => new(leftDecimal < rightInt),
            // int < int
            int leftInt when right.Value is int rightInt => new(leftInt < rightInt),
            // int < decimal
            int leftInt when right.Value is decimal rightDecimal => new(leftInt < rightDecimal),
            _ => throw new ScriptException($"operator < error in payroll values {left} and {right}")
        };
    }

    /// <summary>Compare if a payroll value is greater than another payroll value (decimal, int)</summary>
    public static PayrollValue operator >(PayrollValue left, PayrollValue right)
    {
        // null
        if (!left.HasValue)
        {
            return left;
        }
        if (!right.HasValue)
        {
            return right;
        }

        return left.Value switch
        {
            // decimal > decimal
            decimal leftDecimal when right.Value is decimal rightDecimal => new(leftDecimal > rightDecimal),
            // decimal > int
            decimal leftDecimal when right.Value is int rightInt => new(leftDecimal > rightInt),
            // int > int
            int leftInt when right.Value is int rightInt => new(leftInt > rightInt),
            // int > decimal
            int leftInt when right.Value is decimal rightDecimal => new(leftInt > rightDecimal),
            _ => throw new ScriptException($"operator > error in payroll values {left} and {right}")
        };
    }

    /// <summary>Compare if a payroll value is less or equals than another payroll value (decimal, int)</summary>
    public static PayrollValue operator <=(PayrollValue left, PayrollValue right)
    {
        // null
        if (!left.HasValue)
        {
            return left;
        }
        if (!right.HasValue)
        {
            return right;
        }

        return left.Value switch
        {
            // decimal <= decimal
            decimal leftDecimal when right.Value is decimal rightDecimal => new(leftDecimal <= rightDecimal),
            // decimal <= int
            decimal leftDecimal when right.Value is int rightInt => new(leftDecimal <= rightInt),
            // int <= int
            int leftInt when right.Value is int rightInt => new(leftInt <= rightInt),
            // int <= int
            int leftInt when right.Value is decimal rightDecimal => new(leftInt <= rightDecimal),
            _ => throw new ScriptException($"operator <= error in payroll values {left} and {right}")
        };
    }

    /// <summary>Compare if a payroll value is greater or equals than another payroll value (decimal, int)</summary>
    public static PayrollValue operator >=(PayrollValue left, PayrollValue right)
    {
        // null
        if (!left.HasValue)
        {
            return left;
        }
        if (!right.HasValue)
        {
            return right;
        }

        return left.Value switch
        {
            // decimal >= decimal
            decimal leftDecimal when right.Value is decimal rightDecimal => new(leftDecimal >= rightDecimal),
            // decimal >= int
            decimal leftDecimal when right.Value is int rightInt => new(leftDecimal >= rightInt),
            // int >= int
            int leftInt when right.Value is int rightInt => new(leftInt >= rightInt),
            // int >= decimal
            int leftInt when right.Value is decimal rightDecimal => new(leftInt >= rightDecimal),
            _ => throw new ScriptException($"operator >= error in payroll values {left} and {right}")
        };
    }

    #endregion

    #region Decimal operations

    /// <summary>Returns the integral digits of the specified decimal, using a step size</summary>
    /// <param name="decimals">The number of significant decimal places (precision) in the return value</param>
    /// <param name="mode">A value that specifies how to round d if it is midway between two other numbers</param>
    public decimal? Round(int decimals = 0, MidpointRounding mode = MidpointRounding.ToEven)
    {
        return Value is decimal decimalValue ?
            decimal.Round(decimalValue, decimals, mode) :
            null;
    }

    /// <summary>Rounds a decimal value up</summary>
    /// <param name="stepSize">The round step size</param>
    /// <returns>The up-rounded value</returns>
    public decimal RoundUp(decimal stepSize = 1) =>
        Value is decimal decimalValue ? decimalValue.RoundUp(stepSize) : default;

    /// <summary>Rounds a decimal value down</summary>
    /// <param name="stepSize">The round step size</param>
    /// <returns>The down-rounded value</returns>
    public decimal RoundDown(decimal stepSize) =>
        Value is decimal decimalValue ? decimalValue.RoundDown(stepSize) : default;

    /// <summary>Returns the integral digits of the specified decimal, using a step size</summary>
    /// <param name="stepSize">The step size used to truncate</param>
    public decimal? Truncate(int stepSize = 1) =>
        Value is decimal decimalValue ? decimalValue.Truncate(stepSize) : default;

    #endregion

    /// <summary>Determines whether the specified <see cref="object" /> is equal to this instance</summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>True if the specified <see cref="object" /> is equal to this instance</returns>
    public override bool Equals(object obj)
    {
        var compare = obj as PayrollValue;
        if (compare == null)
        {
            return false;
        }
        if (ReferenceEquals(this, compare))
        {
            return true;
        }

        if (Value == null)
        {
            return compare.Value == null;
        }
        return Value.Equals(compare.Value);
    }

    /// <summary>Returns a hash code for this instance</summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table</returns>
    public override int GetHashCode() =>
        Value.GetHashCode();

    /// <summary>Returns a <see cref="string" /> that represents this instance</summary>
    /// <returns>A <see cref="string" /> that represents this instance</returns>
    public override string ToString() =>
        Value == null ? base.ToString() : Value.ToString();
}