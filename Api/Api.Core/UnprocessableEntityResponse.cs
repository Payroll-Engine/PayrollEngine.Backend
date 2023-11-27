using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PayrollEngine.Api.Core;

public sealed class UnprocessableEntityResponseAttribute() : ProducesResponseTypeAttribute(typeof(ModelStateDictionary), StatusCodes.Status422UnprocessableEntity);