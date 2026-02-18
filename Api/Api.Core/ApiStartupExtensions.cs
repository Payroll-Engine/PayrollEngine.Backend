using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PayrollEngine.Persistence;
using PayrollEngine.Domain.Scripting;
using DomainModel = PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Core;

public static class ApiStartupExtensions
{
    // see also web.config
    private const int MaxRequestBodySize = 2147483647;

    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddApiServices(this IServiceCollection services,
        IConfiguration configuration, IControllerVisibility controllerVisibility,
        ApiSpecification specification, DomainModel.IDbContext dbContext)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        if (controllerVisibility == null)
        {
            throw new ArgumentNullException(nameof(controllerVisibility));
        }
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }
        if (dbContext == null)
        {
            throw new ArgumentNullException(nameof(dbContext));
        }

        // IIS
        services.Configure<IISServerOptions>(options =>
        {
            if (options.MaxRequestBodySize < MaxRequestBodySize)
            {
                options.MaxRequestBodySize = MaxRequestBodySize;
                Log.Trace($"Increased IIS MaxRequestBodyBufferSize to {MaxRequestBodySize}");
            }
        });

        // Kestrel
        services.Configure<KestrelServerOptions>(options =>
        {
            // don't set default value is: 30 MB
            if (options.Limits.MaxRequestBodySize < MaxRequestBodySize)
            {
                options.Limits.MaxRequestBodySize = MaxRequestBodySize;
                Log.Trace($"Increased Kestrel MaxRequestBodySize to {MaxRequestBodySize}");
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
        ScriptCompiler.DumpCompilerSources = serverConfiguration.DumpCompilerSources;

        // culture
        var cultureName = serverConfiguration.StartupCulture;
        if (!string.IsNullOrWhiteSpace(cultureName) && !CultureInfo.CurrentCulture.Name.Equals(cultureName))
        {
            Thread.CurrentThread.CurrentCulture = new(cultureName);
            Log.Trace($"Changing application culture from {CultureInfo.CurrentCulture.Name} to {cultureName}");
        }

        // API services
        ApiFactory.SetupApiServices(services, configuration, dbContext);

        // controllers
        services.AddControllers(setupAction =>
        {
            setupAction.Conventions.Add(new ControllerVisibilityConvention(
                controllerVisibility.GetVisibleControllers(serverConfiguration),
                controllerVisibility.GetHiddenControllers(serverConfiguration)));
        });

        // authentication services
        var authConfig = configuration.GetAuthConfiguration();
        switch (authConfig.Mode)
        {
            case AuthenticationMode.ApiKey:
                break;

            case AuthenticationMode.OAuth:
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = authConfig.OAuth.Authority;
                        options.Audience = authConfig.OAuth.Audience;
                        options.RequireHttpsMetadata = authConfig.OAuth.RequireHttpsMetadata;
                    });

                // no FallbackPolicy — authorization is enforced directly on MapControllers()
                services.AddAuthorization();
                break;

            case AuthenticationMode.None:
            default:
                break;
        }

        // swagger
        services.AddSwaggerGen(setupAction =>
        {
            switch (authConfig.Mode)
            {
                case AuthenticationMode.ApiKey:
                    var apiKey = configuration.GetApiKey();
                    if (!string.IsNullOrWhiteSpace(apiKey))
                        setupAction.AddSwaggerApiKeySecurity();
                    break;

                case AuthenticationMode.OAuth:
                    setupAction.AddSwaggerOAuthSecurity(authConfig.OAuth);
                    break;

                case AuthenticationMode.None:
                default:
                    break;
            }

            // ensure property names in camel case
            setupAction.DescribeAllParametersInCamelCase();

            // document
            setupAction.SwaggerDoc(
                specification.ApiDocumentationName,
                SwaggerTool.CreateInfo(
                    specification.ApiName,
                    specification.ApiVersion,
                    specification.ApiDescription));

            // shared setup
            setupAction.SetupSwagger();

            // XML comments
            if (serverConfiguration.XmlCommentFileNames == null || !serverConfiguration.XmlCommentFileNames.Any())
            {
                throw new PayrollException($"Missing XML comment files in configuration {nameof(PayrollServerConfiguration)}.{nameof(PayrollServerConfiguration.XmlCommentFileNames)}.");
            }
            var combinedXmlCommentFileName = SwaggerTool.CreateXmlCommentsFile(
                specification.ApiDocumentationFileName, serverConfiguration.XmlCommentFileNames);
            setupAction.IncludeXmlComments(combinedXmlCommentFileName);
        });

        // API Version
        services.AddApiVersioning(setupAction =>
            {
                setupAction.AssumeDefaultVersionWhenUnspecified = true;
                var defaultVersion = BackendSpecification.DefaultApiVersion;
                setupAction.DefaultApiVersion = new(defaultVersion.Major, defaultVersion.Minor);
                setupAction.ReportApiVersions = true;
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

        // configuration
        var serverConfiguration = configuration.GetConfiguration<PayrollServerConfiguration>();

        // dark theme and favicon css
        appBuilder.UseStaticFiles();

        // dev support
        if (environment.IsDevelopment())
        {
            // dev error handling
            appBuilder.UseDeveloperExceptionPage();
        }

        // logs
        appLifetime.UseLog(configuration, appBuilder, environment, serverConfiguration.LogHttpRequests);

        // api key
        appBuilder.UseApiKey();

        // routing
        appBuilder.UseRouting();

        // OAuth
        var authConfig = configuration.GetAuthConfiguration();
        if (authConfig.Mode == AuthenticationMode.OAuth)
        {
            appBuilder.UseAuthentication();
            appBuilder.UseAuthorization();
        }

        // swagger
        appBuilder.UseSwagger(
            apiDocumentationName: specification.ApiDocumentationName,
            apiName: specification.ApiName,
            apiVersion: specification.ApiVersion,
            darkTheme: serverConfiguration.DarkTheme,
            rootRedirect: true,
            oauth: authConfig.Mode == AuthenticationMode.OAuth ? authConfig.OAuth : null);

        // database
        if (serverConfiguration.DbTransactionTimeout > TimeSpan.Zero)
        {
            TransactionFactory.Timeout = serverConfiguration.DbTransactionTimeout;
        }

        // endpoints
        appBuilder.UseEndpoints(endpoints =>
        {
            // enforce authentication on all controller endpoints
            var controllers = endpoints.MapControllers();
            if (authConfig.Mode == AuthenticationMode.OAuth)
            {
                controllers.RequireAuthorization();
            }

            // swagger metadata endpoint — always anonymous
            endpoints.MapSwagger().AllowAnonymous();
        });

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