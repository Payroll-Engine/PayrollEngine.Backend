using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PayrollEngine.Api.Core;

public sealed class OkResponseAttribute : ProducesResponseTypeAttribute
{
    public OkResponseAttribute() :
        base(StatusCodes.Status200OK)
    {
    }
}