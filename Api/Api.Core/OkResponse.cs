using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PayrollEngine.Api.Core;

public sealed class OkResponseAttribute() : ProducesResponseTypeAttribute(StatusCodes.Status200OK);