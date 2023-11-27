using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PayrollEngine.Api.Core;

public sealed class NotFoundResponseAttribute() : ProducesResponseTypeAttribute(StatusCodes.Status404NotFound);