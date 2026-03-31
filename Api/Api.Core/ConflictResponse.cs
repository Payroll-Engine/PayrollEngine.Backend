using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PayrollEngine.Api.Core;

public sealed class ConflictResponseAttribute() : ProducesResponseTypeAttribute(StatusCodes.Status409Conflict);
