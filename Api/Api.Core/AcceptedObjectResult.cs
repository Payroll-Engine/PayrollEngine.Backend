using System;
using Microsoft.AspNetCore.Mvc;

namespace PayrollEngine.Api.Core;

/// <summary>
/// API controller accepted result for async operations.
/// Returns HTTP 202 Accepted with a Location header for status polling.
/// </summary>
public class AcceptedObjectResult(string statusLocationPath, object value)
    : AcceptedResult(new Uri(statusLocationPath, UriKind.Relative), value);
