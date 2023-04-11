using Microsoft.AspNetCore.Builder;

namespace PayrollEngine.Api.Core;

public static class HttpsExtensions
{
    public static void UseHttps(this IApplicationBuilder appBuilder, bool useHttpRedirection)
    {
        if (useHttpRedirection)
        {
            appBuilder.UseHttpsRedirection();
        }
    }

    public static bool IsValidStatusCode(this int? statusCode) =>
        statusCode.HasValue && (statusCode.Value >= 200 && statusCode.Value <= 299);
}