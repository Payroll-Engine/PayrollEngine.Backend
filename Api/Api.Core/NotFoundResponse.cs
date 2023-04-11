using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PayrollEngine.Api.Core;

public sealed class NotFoundResponseAttribute : ProducesResponseTypeAttribute
{
    public NotFoundResponseAttribute() :
        base(StatusCodes.Status404NotFound)
    {
    }
}