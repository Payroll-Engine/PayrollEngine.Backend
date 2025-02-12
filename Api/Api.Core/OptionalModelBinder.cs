using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Model binder for optional request body.
/// See https://github.com/dotnet/aspnetcore/issues/6878#issuecomment-595515755
/// </summary>
/// <typeparam name="T">The body object</typeparam>
public class OptionalModelBinder<T> : IModelBinder where T : class
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var stream = bindingContext.HttpContext.Request.Body;
        string body;
        using (var reader = new StreamReader(stream))
        {
            body = await reader.ReadToEndAsync();
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        var result = string.IsNullOrWhiteSpace(body) ?
            null :
            JsonSerializer.Deserialize<T>(body, options);

        bindingContext.Result = ModelBindingResult.Success(result);
    }
}