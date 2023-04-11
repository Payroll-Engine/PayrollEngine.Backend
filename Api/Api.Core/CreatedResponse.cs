using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PayrollEngine.Api.Core;

public sealed class CreatedResponseAttribute : ProducesResponseTypeAttribute
{
    public CreatedResponseAttribute() :
        base(StatusCodes.Status201Created)
    {
    }
}