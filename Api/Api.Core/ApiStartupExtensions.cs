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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using PayrollEngine.Persistence;
using PayrollEngine.Domain.Application;
using PayrollEngine.Domain.Scripting;
using DomainModel = PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Extension methods for configuring and registering all Payroll Engine API services,
/// middleware, authentication, rate limiting, CORS, and Swagger in the ASP.NET Core pipeline.
/// </summary>
public static class ApiStartupExtensions
{
    // see also web.config
    private const int MaxRequestBodySize = 2147483647;

    /// <summary>
    /// Rate limiting policy name for the payrun job start endpoint.
    /// Used by <see cref="Microsoft.AspNetCore.RateLimiting.EnableRateLimitingAttribute"/>.
    /// </summary>
    private const string RateLimitPolicyPayrunJobStart = "PayrunJobStart";

    /// <summary>
    /// Register all API services into the DI container, including authentication,
    /// Swagger, rate limiting, CORS, and controller visibility.
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <param name="configuration">Application configuration (appsettings.json)</param>
    /// <param name="controllerVisibility">Controller visibility rules for Swagger filtering</param>
    /// <param name="specification">API metadata (name, version, documentation)</param>
    /// <param name="dbContext">Database context for persistence operations</param>
    /// <returns>The configured service collection for chaining</returns>
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
        ScriptCompiler.ScriptSafetyAnalysis = serverConfiguration.ScriptSafetyAnalysis;

        // culture
        var cultureName = serverConfiguration.StartupCulture;
        if (!string.IsNullOrWhiteSpace(cultureName) && !CultureInfo.CurrentCulture.Name.Equals(cultureName))
        {
            Thread.CurrentThread.CurrentCulture = new(cultureName);
            Log.Trace($"Changing application culture from {CultureInfo.CurrentCulture.Name} to {cultureName}");
        }

        // webhook HTTP client (replaces static HttpClient for DNS rotation and connection pooling)
        services.AddHttpClient(WebhookDispatchService.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });

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
                // guard: Authority and Audience are mandatory for secure token validation
                if (string.IsNullOrWhiteSpace(authConfig.OAuth.Authority))
                {
                    throw new PayrollException("OAuth authentication requires a configured Authority.");
                }
                if (string.IsNullOrWhiteSpace(authConfig.OAuth.Audience))
                {
                    throw new PayrollException("OAuth authentication requires a configured Audience. " +
                        "Without an audience, tokens issued for other APIs would be accepted.");
                }

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

        // swagger (only when explicitly enabled)
        if (serverConfiguration.EnableSwagger)
        {
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
        }

        // API Version
        services.AddApiVersioning(setupAction =>
            {
                setupAction.AssumeDefaultVersionWhenUnspecified = true;
                var defaultVersion = BackendSpecification.DefaultApiVersion;
                setupAction.DefaultApiVersion = new(defaultVersion.Major, defaultVersion.Minor);
                setupAction.ReportApiVersions = true;
            });

        // CORS (only when at least one origin is configured)
        var cors = serverConfiguration.Cors;
        if (cors.IsActive)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CorsConfiguration.PolicyName, policy =>
                {
                    // origins
                    if (cors.AllowedOrigins is ["*"])
                    {
                        policy.AllowAnyOrigin();
                    }
                    else
                    {
                        policy.WithOrigins(cors.AllowedOrigins);
                    }

                    // methods
                    if (cors.AllowedMethods is ["*"])
                    {
                        policy.AllowAnyMethod();
                    }
                    else
                    {
                        policy.WithMethods(cors.AllowedMethods);
                    }

                    // headers
                    if (cors.AllowedHeaders is ["*"])
                    {
                        policy.AllowAnyHeader();
                    }
                    else
                    {
                        policy.WithHeaders(cors.AllowedHeaders);
                    }

                    // credentials (incompatible with wildcard origin)
                    if (cors.AllowCredentials && cors.AllowedOrigins is not ["*"])
                    {
                        policy.AllowCredentials();
                    }

                    // preflight cache
                    if (cors.PreflightMaxAgeSeconds > 0)
                    {
                        policy.SetPreflightMaxAge(TimeSpan.FromSeconds(cors.PreflightMaxAgeSeconds));
                    }
                });
            });
            Log.Information($"CORS: policy active for origins [{string.Join(", ", cors.AllowedOrigins)}]");
        }

        // rate limiting (only when at least one policy is active)
        var rateLimiting = serverConfiguration.RateLimiting;
        if (rateLimiting.IsActive)
        {
            services.AddRateLimiter(options =>
            {
                // HTTP 429 response
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // global fixed-window limiter
                if (rateLimiting.Global.IsActive)
                {
                    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = rateLimiting.Global.PermitLimit,
                                Window = TimeSpan.FromSeconds(rateLimiting.Global.WindowSeconds)
                            }));
                    Log.Information($"Rate limiting: global policy active ({rateLimiting.Global.PermitLimit} req/{rateLimiting.Global.WindowSeconds}s)");
                }

                // endpoint-specific: payrun job start
                if (rateLimiting.PayrunJobStart.IsActive)
                {
                    options.AddFixedWindowLimiter(RateLimitPolicyPayrunJobStart, limiterOptions =>
                    {
                        limiterOptions.PermitLimit = rateLimiting.PayrunJobStart.PermitLimit;
                        limiterOptions.Window = TimeSpan.FromSeconds(rateLimiting.PayrunJobStart.WindowSeconds);
                    });
                    Log.Information($"Rate limiting: {RateLimitPolicyPayrunJobStart} policy active ({rateLimiting.PayrunJobStart.PermitLimit} req/{rateLimiting.PayrunJobStart.WindowSeconds}s)");
                }
            });
        }

        return services;
    }

    /// <summary>
    /// Configure the HTTP request pipeline with all Payroll Engine middleware
    /// (routing, CORS, authentication, rate limiting, Swagger, endpoints).
    /// </summary>
    /// <param name="appBuilder">Application builder for middleware registration</param>
    /// <param name="environment">Hosting environment (Development/Production)</param>
    /// <param name="appLifetime">Application lifetime for startup/shutdown hooks</param>
    /// <param name="configuration">Application configuration (appsettings.json)</param>
    /// <param name="specification">API metadata for Swagger generation</param>
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

        // error handling
        if (environment.IsDevelopment())
        {
            appBuilder.UseDeveloperExceptionPage();
        }
        else
        {
            appBuilder.UseExceptionHandler(error =>
            {
                error.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "An internal server error occurred."
                    });
                });
            });
        }

        // logs
        appLifetime.UseLog(configuration, appBuilder, environment, serverConfiguration, serverConfiguration.LogHttpRequests);

        // api key
        appBuilder.UseApiKey();

        // routing
        appBuilder.UseRouting();

        // CORS (after routing, before auth)
        if (serverConfiguration.Cors.IsActive)
        {
            appBuilder.UseCors(CorsConfiguration.PolicyName);
        }

        // OAuth
        var authConfig = configuration.GetAuthConfiguration();
        if (authConfig.Mode == AuthenticationMode.OAuth)
        {
            appBuilder.UseAuthentication();
            appBuilder.UseAuthorization();
        }

        // rate limiting (after routing, before endpoints)
        if (serverConfiguration.RateLimiting.IsActive)
        {
            appBuilder.UseRateLimiter();
        }

        // swagger (only when explicitly enabled)
        if (serverConfiguration.EnableSwagger)
        {
            if (!environment.IsDevelopment())
            {
                Log.Warning("Swagger is enabled in a non-development environment. " +
                    "This exposes the full API surface and interactive 'Try it out' functionality. " +
                    "Consider setting EnableSwagger to false in production.");
            }

            appBuilder.UseSwagger(
                apiDocumentationName: specification.ApiDocumentationName,
                apiName: specification.ApiName,
                apiVersion: specification.ApiVersion,
                darkTheme: serverConfiguration.DarkTheme,
                rootRedirect: true,
                oauth: authConfig.Mode == AuthenticationMode.OAuth ? authConfig.OAuth : null);
        }

        // database transaction timeout (one-time initialization)
        if (serverConfiguration.DbTransactionTimeout > TimeSpan.Zero)
        {
            TransactionFactory.Initialize(serverConfiguration.DbTransactionTimeout);
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

            // swagger metadata endpoint — always anonymous (only when enabled)
            if (serverConfiguration.EnableSwagger)
            {
                endpoints.MapSwagger().AllowAnonymous();
            }
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