using Microsoft.AspNetCore.Mvc;

namespace PayrollEngine.Api.Core;

public static class ActionResultExtensions
{
    public static bool IsValidResult<TValue>(this ActionResult<TValue> actionResult)
    {
        if (actionResult.Result is ObjectResult objectResult)
        {
            return objectResult.StatusCode.IsValidStatusCode();
        }
        return true;
    }

    public static bool IsValidResult<TValue>(this ActionResult<TValue[]> actionResult)
    {
        if (actionResult.Result is ObjectResult objectResult)
        {
            return objectResult.StatusCode.IsValidStatusCode();
        }
        return true;
    }
}