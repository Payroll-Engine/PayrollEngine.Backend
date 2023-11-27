using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PayrollEngine.Api.Core;

public sealed class CreatedResponseAttribute() : ProducesResponseTypeAttribute(StatusCodes.Status201Created);