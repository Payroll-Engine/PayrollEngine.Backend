using System;
using Microsoft.Extensions.Configuration;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Extension methods for <see cref="PayrollServerConfiguration"/>
/// </summary>
public static class ApiKeyExtensions
{
    public static string GetApiKey(this IConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        // priority 1: from environment variable
        var apiKey = Environment.GetEnvironmentVariable(SystemSpecification.PayrollApiKey);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return apiKey;
        }

        // priority 2: from application configuration
        var serverConfiguration = configuration.GetConfiguration<PayrollServerConfiguration>();
        return serverConfiguration?.ApiKey;
    }
}