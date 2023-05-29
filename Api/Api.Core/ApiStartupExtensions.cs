using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PayrollEngine.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PayrollEngine.Api.Core;

public static class ApiStartupExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services,
        IConfiguration configuration, ApiSpecification specification)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        // IIS
        services.Configure<IISServerOptions>(options =>
        {
            if (options.MaxRequestBodySize < int.MaxValue)
            {
                options.MaxRequestBodySize = int.MaxValue;
                Log.Trace($"Increased IIS MaxRequestBodyBufferSize to {int.MaxValue}");
            }
        });

        // Kestrel
        services.Configure<KestrelServerOptions>(options =>
        {
            // if don't set default value is: 30 MB
            if (options.Limits.MaxRequestBodySize < int.MaxValue)
            {
                options.Limits.MaxRequestBodySize = int.MaxValue;
                Log.Trace($"Increased Kestrel MaxRequestBodySize to {int.MaxValue}");
            }
        });

        // server configuration
        var serverConfiguration = configuration.GetConfiguration<PayrollServerConfiguration>();
        if (serverConfiguration == null)
        {
            throw new ArgumentException("Missing payroll server configuration", nameof(configuration));
        }

        // script compiler
        if (serverConfiguration.InitializeScriptCompiler)
        {
            InitializeScriptCompiler();
        }

        // culture
        var culture = serverConfiguration.StartupCulture;
        if (!string.IsNullOrWhiteSpace(culture) && !CultureInfo.CurrentCulture.Name.Equals(culture))
        {
            CultureInfo.CurrentCulture = new(culture);
        }

        // health check
        if (serverConfiguration.UseHealthCheck)
        {
            services.AddApiHealthCheck(configuration);
        }

        // API services
        ApiFactory.SetupApiServices(services, configuration);

        // controllers
        services.AddControllers();

        // swagger
        services.AddSwaggerGen(setupAction =>
        {
            // ensure property names in camel case
            setupAction.DescribeAllParametersInCamelCase();

            // document
            setupAction.SwaggerDoc(
                specification.ApiDocumentationName,
                SwaggerTool.CreateInfo(
                    specification.ApiName,
                    specification.ApiVersion,
                    specification.ApiDescription,
                    configuration["HealthChecksUI:Uri"]));

            // shared setup
            setupAction.SetupSwagger();

            // XML comments
            if (serverConfiguration.XmlCommentFileNames == null || !serverConfiguration.XmlCommentFileNames.Any())
            {
                throw new PayrollException($"Missing XML comment files in configuration {nameof(PayrollServerConfiguration)}.{nameof(PayrollServerConfiguration.XmlCommentFileNames)}");
            }
            var combinedXmlCommentFileName = SwaggerTool.CreateXmlCommentsFile(
                specification.ApiDocumentationFileName, serverConfiguration.XmlCommentFileNames);
            setupAction.IncludeXmlComments(combinedXmlCommentFileName);
        });

        return services;
    }

    public static void UsePayrollApiServices(this IApplicationBuilder appBuilder, IWebHostEnvironment environment,
        IHostApplicationLifetime appLifetime, IConfiguration configuration, ApiSpecification specification)
    {
        if (appBuilder == null)
        {
            throw new ArgumentNullException(nameof(appBuilder));
        }
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        if (environment == null)
        {
            throw new ArgumentNullException(nameof(environment));
        }
        if (appLifetime == null)
        {
            throw new ArgumentNullException(nameof(appLifetime));
        }
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        // dev support
        if (environment.IsDevelopment())
        {
            appBuilder.UseDeveloperExceptionPage();
        }

        // logs
        var serverConfiguration = configuration.GetConfiguration<PayrollServerConfiguration>();
        appLifetime.UseLog(appBuilder, environment, serverConfiguration.LogHttpRequests);

        // https
        appBuilder.UseHttps(useHttpRedirection: serverConfiguration.UseHealthCheck);

        // swagger
        appBuilder.UseSwagger(specification.ApiDocumentationName, specification.ApiName,
            specification.ApiVersion);

        // database
        if (serverConfiguration.TransactionTimeout > TimeSpan.Zero)
        {
            TransactionFactory.Timeout = serverConfiguration.TransactionTimeout;
        }

        // routing
        appBuilder.UseRouting();
        appBuilder.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        // health check
        if (serverConfiguration.UseHealthCheck)
        {
            appBuilder.UseHealthCheck();
        }

        // dapper
        DapperTypes.AddTypeHandlers();
    }

    private static void InitializeScriptCompiler()
    {
        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);
        var syntaxTrees = new List<SyntaxTree>
        {
            SyntaxFactory.ParseSyntaxTree(SourceText.From("using System; public class HelloWorld {}"), options)
        };
        using var peStream = new MemoryStream();
        CSharpCompilation.Create("BootCompile", syntaxTrees, null,
            new(OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default)).Emit(peStream);
    }
}