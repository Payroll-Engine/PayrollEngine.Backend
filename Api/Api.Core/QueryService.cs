using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PayrollEngine.Api.Model;
using PayrollEngine.Domain.Scripting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Api.Core;

/// <inheritdoc />
public class QueryService : IQueryService
{
    private Dictionary<string, QueryMethodInfo> queryMethods;

    public IServiceProvider ServiceProvider { get; }

    public QueryService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public bool ExistsQuery(string methodName) =>
        Queries.ContainsKey(methodName);

    /// <inheritdoc />
    public virtual DataTable ExecuteQuery(int tenantId, string methodName, Language? language,
        Dictionary<string, string> parameters, IApiControllerContext controllerContext)
    {
        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException(nameof(methodName));
        }
        if (controllerContext == null)
        {
            throw new ArgumentNullException(nameof(controllerContext));
        }

        if (!Queries.TryGetValue(methodName, out var queryMethod))
        {
            throw new QueryException($"Unknown query on method {methodName}");
        }

        // controller
        using var scope = ServiceProvider.CreateScope();
        var controller = controllerContext.Activate(queryMethod.DeclaringType) as ControllerBase;
        if (controller == null)
        {
            throw new QueryException($"Missing web controller {queryMethod.DeclaringType} for query {methodName}");
        }

        // method parameters
        var methodParameters = queryMethod.GetParameterValues(parameters);
        SetupMethodParameters(tenantId, methodParameters);

        // method execution
        var methodResult = (Task)queryMethod.MethodInfo.Invoke(controller, methodParameters.Values.ToArray());
        if (methodResult == null)
        {
            return null;
        }
        methodResult.Wait();

        // query result
        var result = GetQueryResult(methodResult);
        if (result == null)
        {
            return null;
        }

        // localizations
        ApplyLocalizations(result, language);

        // result table
        var resultTable = GetResultTable(methodName, result, methodParameters);
        return resultTable;
    }

    private static void SetupMethodParameters(int tenantId, IDictionary<ParameterInfo, object> methodParameters)
    {
        var tenantParameter = methodParameters.Keys.FirstOrDefault(x => string.Equals(x.Name, "tenantId"));
        if (tenantParameter != null)
        {
            methodParameters[tenantParameter] = tenantId;
        }
    }

    public virtual async Task<DataTable> ExecuteQueryAsync(int tenantId, string methodName, Language? language,
        Dictionary<string, string> parameters, IApiControllerContext controllerContext)
    {
        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException(nameof(methodName));
        }
        if (controllerContext == null)
        {
            throw new ArgumentNullException(nameof(controllerContext));
        }

        if (!Queries.TryGetValue(methodName, out var queryMethod))
        {
            throw new QueryException($"Unknown query on method {methodName}");
        }

        // controller
        var controller = controllerContext.Activate(queryMethod.DeclaringType) as ControllerBase;
        if (controller == null)
        {
            throw new QueryException($"Missing web controller {queryMethod.DeclaringType}");
        }

        // method parameters
        var methodParameters = queryMethod.GetParameterValues(parameters);
        SetupMethodParameters(tenantId, methodParameters);

        // method execution
        var methodResult = (Task)queryMethod.MethodInfo.Invoke(controller, methodParameters.Values.ToArray());
        if (methodResult == null)
        {
            return null;
        }
        await methodResult;

        // query result
        var result = GetQueryResult(methodResult);
        if (result == null)
        {
            return null;
        }

        // localizations
        ApplyLocalizations(result, language);

        // result table
        var resultTable = GetResultTable(methodName, result, methodParameters);
        return resultTable;
    }

    /// <inheritdoc />
    public virtual async Task<DataTable> ExecuteQueryAsync(int tenantId, string queryName, string methodName, Language? language,
        Dictionary<string, string> requestParameters, Dictionary<string, string> reportParameters,
        IApiControllerContext controllerContext)
    {
        if (string.IsNullOrWhiteSpace(queryName))
        {
            throw new ArgumentException(nameof(queryName));
        }
        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException(nameof(methodName));
        }
        if (controllerContext == null)
        {
            throw new ArgumentNullException(nameof(controllerContext));
        }

        if (!Queries.TryGetValue(methodName, out var queryMethod))
        {
            throw new QueryException($"Unknown query on method {methodName}");
        }

        // controller
        var controller = controllerContext.Activate(queryMethod.DeclaringType) as ControllerBase;
        if (controller == null)
        {
            throw new QueryException($"Missing web controller {queryMethod.DeclaringType}");
        }

        // method parameters
        var methodParameters = queryMethod.GetParameterValues(queryName, reportParameters, requestParameters);
        SetupMethodParameters(tenantId, methodParameters);

        try
        {
            // method execution
            var methodResult = (Task)queryMethod.MethodInfo.Invoke(controller, methodParameters.Values.ToArray());
            if (methodResult == null)
            {
                return null;
            }
            await methodResult;

            // query result
            var result = GetQueryResult(methodResult);
            if (result == null)
            {
                return null;
            }

            // localizations
            ApplyLocalizations(result, language);

            // result table
            var resultTable = GetResultTable(queryName, result, methodParameters);
            return resultTable;
        }
        catch (Exception exception)
        {
            var buffer = new StringBuilder();
            buffer.Append($"Error in {queryMethod.MethodInfo.Name}: {exception.GetBaseMessage()}");
            foreach (var methodParameter in methodParameters)
            {
                buffer.Append($", Parameter {methodParameter.Key.Name}: {methodParameter.Value}");
            }
            throw new QueryException(buffer.ToString(), exception);
        }
    }

    private static DataTable GetResultTable(string methodName, IList<object> result, IDictionary<ParameterInfo, object> methodParameters)
    {
        var tableName = ApiOperationTool.GetOperationBaseName(methodName);
        var dataTable = Data.DataTableConversionExtensions.ToSystemDataTable(result, tableName, primaryKey: nameof(ApiObjectBase.Id));

        // parameter columns
        var parameterColumns = GetParameterColumns(methodParameters);
        foreach (var parameterColumn in parameterColumns)
        {
            // table column
            var columnName = parameterColumn.Key.FirstCharacterToUpper();
            if (!dataTable.Columns.Contains(columnName))
            {
                dataTable.Columns.Add(columnName, parameterColumn.Value.Item1);
            }
        }

        // result items
        Data.DataTableItemExtensions.AppendItems(dataTable, result);

        // result parameters
        SetResultParameters(parameterColumns, dataTable);

        return dataTable;
    }

    /// <summary>
    /// Get the parameter columns
    /// </summary>
    /// <param name="methodParameters">The method parameters</param>
    /// <returns>Parameter columns dictionary: key=column name, value=tuple of parameter type and parameter value</returns>
    private static Dictionary<string, Tuple<Type, object>> GetParameterColumns(IDictionary<ParameterInfo, object> methodParameters)
    {
        var parameterColumns = new Dictionary<string, Tuple<Type, object>>();
        foreach (var methodParameter in methodParameters)
        {
            var parameterName = methodParameter.Key.Name;
            if (parameterName == null || methodParameter.Value == null)
            {
                // no parameter value
                continue;
            }

            // column type
            var nullableType = Nullable.GetUnderlyingType(methodParameter.Key.ParameterType);
            var columnType = nullableType ?? methodParameter.Key.ParameterType;
            // json column
            if (columnType.IsSerializedType())
            {
                columnType = typeof(string);
            }

            // column
            var columnName = parameterName.FirstCharacterToUpper();
            parameterColumns.Add(columnName, new(columnType, methodParameter.Value));
        }
        return parameterColumns;
    }

    private static void SetResultParameters(Dictionary<string, Tuple<Type, object>> parameterColumns, DataTable dataTable)
    {
        if (!parameterColumns.Any())
        {
            return;
        }

        foreach (var parameterColumn in parameterColumns)
        {
            foreach (var dataRow in dataTable.AsEnumerable())
            {
                var value = parameterColumn.Value.Item2;
                // json value
                if (value != null && parameterColumn.Value.Item1.IsSerializedType())
                {
                    value = JsonSerializer.Serialize(value);
                }
                if (value != null)
                {
                    dataRow[parameterColumn.Key] = value;
                }
            }
        }
    }

    private static IList<object> GetQueryResult(Task methodResult)
    {
        var resultProperty = methodResult.GetType().GetProperty("Result");
        if (resultProperty == null)
        {
            return null;
        }

        var methodResultValue = resultProperty.GetValue(methodResult);
        if (methodResultValue is IConvertToActionResult actionResult)
        {
            var value = actionResult.GetPropertyValue("Value");
            if (value == null)
            {
                return null;
            }
            if (value is IEnumerable<object> array)
            {
                return new List<object>(array);
            }
            return new List<object> { value };
        }

        if (methodResultValue is ObjectResult objectResult)
        {
            if (objectResult.StatusCode == 500)
            {
                throw new PayrollException($"Internal server error {objectResult.Value}");
            }
            if (objectResult.StatusCode is null or < 200 or >= 300)
            {
                return null;
            }
            return objectResult.Value as IList<object>;
        }
        return null;
    }

    private void ApplyLocalizations(IList<object> items, Language? language)
    {
        if (items == null || !language.HasValue)
        {
            return;
        }

        // language
        var languageCode = language.Value.LanguageCode();
        Dictionary<PropertyInfo, PropertyInfo> localizationProperties = null;
        foreach (var item in items)
        {
            // setup localization properties
            if (localizationProperties == null)
            {
                localizationProperties = new();
                var itemType = item.GetType();
                foreach (var propertyInfo in item.GetType().GetInstanceProperties())
                {
                    // requires the localization attribute
                    if (propertyInfo.GetCustomAttributes(typeof(LocalizationAttribute), false)
                            .FirstOrDefault() is LocalizationAttribute localizationAttribute)
                    {
                        // collect localization property
                        var localizationSource = itemType.GetProperty(localizationAttribute.Source);
                        localizationProperties.Add(propertyInfo, localizationSource);
                    }
                }
            }

            // localize values
            foreach (var localizationProperty in localizationProperties)
            {
                if (localizationProperty.Key.GetValue(item) is Dictionary<string, string> localizationValues &&
                    localizationValues.TryGetValue(languageCode, out var value))
                {
                        // apply localization property
                    localizationProperty.Value.SetValue(item, value);
                }
            }
        }
    }

    /// <summary>
    /// Loads queries on demand
    /// </summary>
    private Dictionary<string, QueryMethodInfo> Queries =>
        queryMethods ??= ApiQueryFactory.GetQueryMethods();

}