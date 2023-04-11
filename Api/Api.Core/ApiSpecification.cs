using System;

namespace PayrollEngine.Api.Core;

/// <summary>
/// The Payroll API specification
/// </summary>
public class ApiSpecification
{
    public ApiSpecification(string apiDocumentationName, string apiDescription,
        string apiName, string apiDocumentationFileName, string apiVersion)
    {
        if (string.IsNullOrWhiteSpace(apiDocumentationName))
        {
            throw new ArgumentException(nameof(apiDocumentationName));
        }
        if (string.IsNullOrWhiteSpace(apiDescription))
        {
            throw new ArgumentException(nameof(apiDescription));
        }
        if (string.IsNullOrWhiteSpace(apiName))
        {
            throw new ArgumentException(nameof(apiName));
        }
        if (string.IsNullOrWhiteSpace(apiVersion))
        {
            throw new ArgumentException(nameof(apiVersion));
        }

        ApiDocumentationName = apiDocumentationName;
        ApiDescription = apiDescription;
        ApiName = apiName;
        ApiDocumentationFileName = apiDocumentationFileName;
        ApiVersion = apiVersion;
    }

    /// <summary>
    /// API documentation name
    /// </summary>
    public string ApiDocumentationName { get; }

    /// <summary>
    /// API description
    /// </summary>
    public string ApiDescription { get; }

    /// <summary>
    /// API name
    /// </summary>
    public string ApiName { get; }

    /// <summary>
    /// API documentation file name
    /// </summary>
    public string ApiDocumentationFileName { get; }

    /// <summary>
    /// API version
    /// </summary>
    public string ApiVersion { get; }
}