using System;
using PayrollEngine.Api.Model;
using Microsoft.AspNetCore.Mvc;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Api controller created result for objects
/// </summary>
/// <remarks>
/// see https://stackoverflow.com/questions/47939945/how-to-use-created-or-createdataction-createdatroute-in-an-asp-net-core-api
/// and https://github.com/aspnet/AspNetCore/issues/7098#issuecomment-486448014
/// </remarks>
public class CreatedObjectResult : CreatedResult
{
    public CreatedObjectResult(string path, ApiObjectBase created) :
        base(new Uri($"{path}/{created.Id}", UriKind.Relative), created)
    {
    }
}