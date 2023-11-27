using System;
using Microsoft.AspNetCore.Mvc;

namespace PayrollEngine.Api.Core;

public class ApiControllerContext(ControllerContext context) : IApiControllerContext
{
    private ControllerContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));

    public object Activate(Type targetType)
    {
        if (targetType == null)
        {
            throw new ArgumentNullException(nameof(targetType));
        }

        // controller
        // https://andrewlock.net/controller-activation-and-dependency-injection-in-asp-net-core-mvc/
        var controller = Context.HttpContext.RequestServices.GetService(targetType) as ControllerBase;
        if (controller != null)
        {
            // https://stackoverflow.com/a/50777301
            controller.ControllerContext = Context;
        }
        return controller;
    }
}