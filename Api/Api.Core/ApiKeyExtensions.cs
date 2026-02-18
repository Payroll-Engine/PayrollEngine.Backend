using System;
using Microsoft.Extensions.Configuration;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Extension methods for <see cref="PayrollServerConfiguration"/>
/// </summary>
public static class ApiKeyExtensions
{
    /// <param name="configuration"></param>
    extension(IConfiguration configuration)
    {
        /// <summary>
        /// Get the api key
        /// </summary>
        /// <returns></returns>
        public string GetApiKey()
        {
            var authConfig = configuration.GetAuthConfiguration();
            if (authConfig.Mode != AuthenticationMode.ApiKey)
            {
                return null;
            }

            // priority 1: environment variable
            var apiKey = Environment.GetEnvironmentVariable(SystemSpecification.PayrollApiKey);
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                return apiKey;
            }

            // priority 2: only when mode is ApiKey
            if (string.IsNullOrWhiteSpace(authConfig.ApiKey))
            {
                throw new PayrollException("Authentication: missing api key.");
            }

            return authConfig.ApiKey;
        }

        /// <summary>
        /// Get authentication configuration
        /// </summary>
        /// <returns></returns>
        public AuthenticationConfiguration GetAuthConfiguration() =>
            configuration.GetConfiguration<PayrollServerConfiguration>()?.Authentication ?? new();
    }
}