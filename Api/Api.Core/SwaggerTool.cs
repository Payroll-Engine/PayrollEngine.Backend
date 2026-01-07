using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace PayrollEngine.Api.Core;

public static class SwaggerTool
{
    private static readonly string SwaggerRootFolder = "swagger";
    private static readonly string SwaggerFileName = "swagger.json";

    public static void SetupSwagger(this SwaggerGenOptions options)
    {
        // filters
        options.OperationFilter<SwaggerOperationFilter>();
        options.DocumentFilter<SwaggerDocumentFilter>();

        // operation sort order
        // does not work as expected
        // see https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/401#issuecomment-327395738
        //options.OrderActionsBy(api =>api.HttpMethod);

        // action groups
        options.TagActionsBy(api =>
        {
            if (api.ActionDescriptor is ControllerActionDescriptor controllerDescriptor)
            {
                if (controllerDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(ApiControllerNameAttribute), true).FirstOrDefault() is ApiControllerNameAttribute attribute)
                {
                    return new List<string>([attribute.ControllerName]);
                }
                return new List<string>([controllerDescriptor.ControllerName]);
            }
            return null;
        });
    }

    public static void SetupSwaggerUI(this SwaggerUIOptions options, string apiDocumentationName,
        string apiName, string apiVersion)
    {
        var title = $"{apiName} v{apiVersion}";
        options.SwaggerEndpoint(
            $"/{SwaggerRootFolder}/{apiDocumentationName}/{SwaggerFileName}",
            title);
        options.DocumentTitle = title;
        options.EnableDeepLinking();
        options.DisplayOperationId();
    }

    /// <summary>
    /// Combine multiple XML comment files into one.
    /// </summary>
    /// <param name="combinedXmlFileName"></param>
    /// <param name="xmlCommentFiles"></param>
    /// <returns>The file name of the combined XML comments file</returns>
    public static string CreateXmlCommentsFile(string combinedXmlFileName, IEnumerable<string> xmlCommentFiles)
    {
        if (string.IsNullOrWhiteSpace(combinedXmlFileName))
        {
            throw new ArgumentException(nameof(combinedXmlFileName));
        }
        if (xmlCommentFiles == null)
        {
            throw new ArgumentNullException(nameof(xmlCommentFiles));
        }

        // target path
        var targetPath = AppContext.BaseDirectory;
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            throw new ArgumentException(nameof(targetPath));
        }

        XElement xml = null;
        // build one large xml comments file
        foreach (var xmlCommentFile in xmlCommentFiles)
        {
            var fileName = Path.Combine(targetPath, xmlCommentFile);
            if (xml == null)
            {
                xml = XElement.Load(fileName);
            }
            else
            {
                var combinedXml = XElement.Load(fileName);
                foreach (var ele in combinedXml.Descendants())
                {
                    xml.Add(ele);
                }
            }
        }

        // save xml to file
        if (xml != null)
        {
            var targetFileName = Path.Combine(targetPath, combinedXmlFileName);
            xml.Save(targetFileName);
            return targetFileName;
        }

        return null;
    }

    public static OpenApiInfo CreateInfo(string apiName, string apiVersion, string apiDescription)
    {
        var assemblyInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        var info = new OpenApiInfo
        {
            Title = $"{apiName} v{apiVersion}",
            Version = apiVersion,
            Description = $"{apiDescription} v{assemblyInfo.ProductVersion}"
        };
        return info;
    }
}