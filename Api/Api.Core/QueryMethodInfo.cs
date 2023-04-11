using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace PayrollEngine.Api.Core;

internal sealed class QueryMethodInfo
{
    internal Type DeclaringType { get; }
    internal MethodInfo MethodInfo { get; }

    private string Name => MethodInfo.Name;

    internal QueryMethodInfo(Type declaringType, MethodInfo methodInfo)
    {
        DeclaringType = declaringType ?? throw new ArgumentNullException(nameof(declaringType));
        MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
    }

    internal IDictionary<ParameterInfo, object> GetParameterValues(Dictionary<string, string> parameters)
    {
        var values = new Dictionary<ParameterInfo, object>();

        // properties
        foreach (ParameterInfo parameter in MethodInfo.GetParameters())
        {
            if (IsParameterValue(parameter.ParameterType))
            {
                // reference type
                var value = GetParameterValue(parameter.Name, parameter.ParameterType, parameters);
                values.Add(parameter, value);
            }
            else
            {
                // class (see PayrollEngine.Query)
                values.Add(parameter, GetClassParameterValues(parameter.ParameterType, parameters));
            }
        }

        return values;
    }

    internal IDictionary<ParameterInfo, object> GetParameterValues(string queryName, Dictionary<string, string> reportParameters,
        Dictionary<string, string> requestParameters)
    {
        var values = new Dictionary<ParameterInfo, object>();

        // properties
        foreach (ParameterInfo parameter in MethodInfo.GetParameters())
        {
            if (IsParameterValue(parameter.ParameterType))
            {
                // reference type
                var value = GetParameterValue(queryName, parameter.Name, parameter.ParameterType, reportParameters, requestParameters);
                values.Add(parameter, value);
            }
            else
            {
                // class (see PayrollEngine.Query)
                values.Add(parameter, GetClassParameterValues(parameter.ParameterType, queryName, reportParameters, requestParameters));
            }
        }

        return values;
    }

    private static object GetClassParameterValues(Type type, Dictionary<string, string> parameters)
    {
        var parameterProperties = new Dictionary<PropertyInfo, object>();

        foreach (var property in type.GetInstanceProperties())
        {
            if (IsParameterValue(property.PropertyType))
            {
                // reference type
                var value = GetParameterValue(property.Name, property.PropertyType, parameters);
                if (value != null)
                {
                    parameterProperties.Add(property, value);
                }
            }
        }

        // no parameters specified
        if (!parameterProperties.Any())
        {
            return null;
        }

        // create instance and apply parameters to properties
        var parameterClass = Activator.CreateInstance(type);
        foreach (var parameterProperty in parameterProperties)
        {
            parameterProperty.Key.SetValue(parameterClass, parameterProperty.Value);
        }

        return parameterClass;
    }

    private static object GetClassParameterValues(Type type, string queryName, Dictionary<string, string> reportParameters,
        Dictionary<string, string> requestParameters)
    {
        var parameterProperties = new Dictionary<PropertyInfo, object>();

        foreach (var property in type.GetInstanceProperties())
        {
            if (property.PropertyType.IsClass && (property.PropertyType.IsArray || property.PropertyType != typeof(string)))
            {
                continue;
            }

            // reference type
            var value = GetParameterValue(queryName, property.Name, property.PropertyType, reportParameters, requestParameters);
            if (value != null)
            {
                parameterProperties.Add(property, value);
            }
        }

        // no parameters specified
        if (!parameterProperties.Any())
        {
            return null;
        }

        // create instance and apply parameters to properties
        var parameterClass = Activator.CreateInstance(type);
        foreach (var parameterProperty in parameterProperties)
        {
            parameterProperty.Key.SetValue(parameterClass, parameterProperty.Value);
        }

        return parameterClass;
    }

    private static bool IsParameterValue(Type type) =>
        type.IsArray || type == typeof(string) || !type.IsClass;

    private static object GetParameterValue(string parameterName, Type parameterType, Dictionary<string, string> parameters)
    {
        object value = null;
        if (parameters != null)
        {
            foreach (var parameter in parameters)
            {
                if (string.Equals(parameter.Key, parameterName, StringComparison.InvariantCultureIgnoreCase))
                {
                    value = ConvertParameterValue(parameter.Value, parameterType);
                    break;
                }
            }
        }
        return value ?? parameterType.GetDefaultValue();
    }

    private static object GetParameterValue(string queryName, string parameterName, Type parameterType,
        Dictionary<string, string> reportParameters, Dictionary<string, string> requestParameters)
    {
        var primaryParameterName = $"{queryName}.{parameterName}";
        object value = null;

        // priority 1: request parameter
        if (requestParameters != null)
        {
            foreach (var requestParameter in requestParameters)
            {
                if (string.Equals(requestParameter.Key, primaryParameterName, StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(requestParameter.Key, parameterName, StringComparison.InvariantCultureIgnoreCase))
                {
                    value = ConvertParameterValue(requestParameter.Value, parameterType);
                    break;
                }
            }
        }

        // priority 2: report parameter
        if (value == null && reportParameters != null)
        {
            foreach (var reportParameter in reportParameters)
            {
                if (string.Equals(reportParameter.Key, primaryParameterName, StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(reportParameter.Key, parameterName, StringComparison.InvariantCultureIgnoreCase))
                {
                    value = ConvertParameterValue(reportParameter.Value, parameterType);
                    break;
                }
            }
        }

        // priority 3: default value
        return value ?? parameterType.GetDefaultValue();
    }

    private static object ConvertParameterValue(string jsonValue, Type type)
    {
        if (jsonValue == null)
        {
            return null;
        }

        // resolve nullable types
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = Nullable.GetUnderlyingType(type);
        }
        if (type == null)
        {
            return null;
        }

        // cast string
        if (type == typeof(string))
        {
            return jsonValue;
        }

        // cast bool
        if (type == typeof(bool))
        {
            if (!bool.TryParse(jsonValue, out var boolValue))
            {
                return null;
            }
            return boolValue;
        }

        // cast int
        if (type == typeof(int))
        {
            if (!int.TryParse(jsonValue, out var intValue))
            {
                return null;
            }
            return intValue;
        }

        // cast date time
        if (type == typeof(DateTime))
        {
            if (!DateTime.TryParse(jsonValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dateValue))
            {
                return null;
            }
            return dateValue;
        }

        // cast enum
        if (type.IsEnum)
        {
            if (!Enum.TryParse(type, jsonValue, out var enumValue))
            {
                return null;
            }
            return enumValue;
        }

        // cast object
        return JsonSerializer.Deserialize(jsonValue, type);
    }

    public override string ToString() => Name;
}