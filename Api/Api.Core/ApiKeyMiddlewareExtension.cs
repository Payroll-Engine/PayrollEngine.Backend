using Microsoft.AspNetCore.Builder;

namespace PayrollEngine.Api.Core;

public static class ApiKeyMiddlewareExtension
{
    public static void UseApiKey(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ApiRequestMiddleware>();
}