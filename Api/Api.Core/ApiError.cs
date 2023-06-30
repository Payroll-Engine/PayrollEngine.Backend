
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace PayrollEngine.Api.Core;

public class ApiError
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public string StackTrace { get; set; }
}