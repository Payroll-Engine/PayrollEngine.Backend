using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Manager queries on Api web methods
/// </summary>
public interface IQueryService
{
    /// <summary>
    /// Test if a query web method exists
    /// </summary>
    /// <param name="methodName">Name of the web method</param>
    /// <returns>True if the query web method exists</returns>
    bool ExistsQuery(string methodName);

    /// <summary>
    /// Execute a web method sync
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="methodName">Name of the web method</param>
    /// <param name="culture">The query culture</param>
    /// <param name="parameters">The query parameters</param>
    /// <param name="controllerContext">The controller context</param>
    /// <returns>The query result data table</returns>
    DataTable ExecuteQuery(int tenantId, string methodName, string culture, Dictionary<string, string> parameters,
        IApiControllerContext controllerContext);

    /// <summary>
    /// Execute a web method async
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="methodName">Name of the web method</param>
    /// <param name="culture">The query culture</param>
    /// <param name="parameters">The query parameters</param>
    /// <param name="controllerContext">The controller context</param>
    /// <returns>The query result data table</returns>
    Task<DataTable> ExecuteQueryAsync(int tenantId, string methodName, string culture, Dictionary<string, string> parameters,
        IApiControllerContext controllerContext);

    /// <summary>
    /// Execute a query on a web method
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="queryName">The query name</param>
    /// <param name="methodName">Name of the web method</param>
    /// <param name="culture">The query culture</param>
    /// <param name="requestParameters">The request parameters</param>
    /// <param name="reportParameters">The report parameters</param>
    /// <param name="controllerContext">The controller context</param>
    /// <returns>The query result data table</returns>
    Task<DataTable> ExecuteQueryAsync(int tenantId, string queryName, string methodName, string culture,
        Dictionary<string, string> requestParameters, Dictionary<string, string> reportParameters,
        IApiControllerContext controllerContext);
}