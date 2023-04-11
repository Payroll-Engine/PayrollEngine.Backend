using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PayrollEngine.Api.Core;

public static class ApiSetup
{
    public static void SetupDefaultOptions(MvcOptions options)
    {
        options.Filters.Add(
            new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest));
        options.Filters.Add(
            new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
        options.Filters.Add(
            new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));
        options.Filters.Add(
            new ProducesDefaultResponseTypeAttribute());

        options.ReturnHttpNotAcceptable = true;
    }

    public static void SetupBehaviourOptions(ApiBehaviorOptions options)
    {
        options.InvalidModelStateResponseFactory = actionContext =>
        {
            var actionExecutingContext =
                actionContext as ActionExecutingContext;

            // if there are model state errors & all keys were correctly
            // found/parsed we're dealing with validation errors
            if (actionContext.ModelState.ErrorCount > 0
                && actionExecutingContext?.ActionArguments.Count == actionContext.ActionDescriptor.Parameters.Count)
            {
                return new UnprocessableEntityObjectResult(actionContext.ModelState);
            }

            // if one of the keys wasn't correctly found / couldn't be parsed
            // ReSharper disable once CommentTypo
            // we're dealing with null/unparsable input
            return new BadRequestObjectResult(actionContext.ModelState);
        };
    }

}