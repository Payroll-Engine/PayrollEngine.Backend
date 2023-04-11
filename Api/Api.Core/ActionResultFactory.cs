using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace PayrollEngine.Api.Core;

public static class ActionResultFactory
{
    [NonAction]
    public static OkObjectResult Ok([ActionResultObjectValue] object value) =>
        new(value);

    [NonAction]
    public static BadRequestObjectResult BadRequest([ActionResultObjectValue] object error) =>
        new(error);

    [NonAction]
    public static NotFoundObjectResult NotFound([ActionResultObjectValue] object value) =>
        new(value);

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