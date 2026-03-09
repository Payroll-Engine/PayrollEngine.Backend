using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Factory for creating standardized <see cref="ActionResult"/> responses
/// with consistent error formatting across all API controllers.
/// </summary>
public static class ActionResultFactory
{
    /// <summary>Create an HTTP 200 OK result with the specified value</summary>
    [NonAction]
    public static OkObjectResult Ok([ActionResultObjectValue] object value) =>
        new(value);

    /// <summary>Create an HTTP 400 Bad Request result with the specified error</summary>
    [NonAction]
    public static BadRequestObjectResult BadRequest([ActionResultObjectValue] object error) =>
        new(error);

    /// <summary>Create an HTTP 404 Not Found result with the specified value</summary>
    [NonAction]
    public static NotFoundObjectResult NotFound([ActionResultObjectValue] object value) =>
        new(value);

    /// <summary>
    /// Create an HTTP 500 Internal Server Error result with serialized error details.
    /// Logs the exception and returns an <see cref="ApiError"/> JSON payload.
    /// </summary>
    /// <param name="exception">The exception to log and serialize</param>
    [NonAction]
    public static ActionResult InternalServerError(Exception exception)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        var message = exception.GetBaseMessage();
        Log.Error(exception, message);

        var errorData = JsonSerializer.Serialize(new ApiError
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            Message = message,
            StackTrace = exception.StackTrace
        }, new JsonSerializerOptions { WriteIndented = true });

        return new ObjectResult(errorData)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }
}