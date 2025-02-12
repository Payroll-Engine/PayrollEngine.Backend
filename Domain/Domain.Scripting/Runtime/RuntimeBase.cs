using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for a function
/// </summary>
public abstract class RuntimeBase : IRuntime
{
    /// <summary>
    /// The runtime settings
    /// </summary>
    protected RuntimeSettings Settings { get; }

    /// <summary>
    /// The function host
    /// </summary>
    private IFunctionHost FunctionHost => Settings.FunctionHost;

    /// <summary>
    /// Function execution timeout <see cref="BackendScriptingSpecification.ScriptFunctionTimeout"/>/>
    /// </summary>
    protected virtual TimeSpan Timeout =>
        TimeSpan.FromMilliseconds(BackendScriptingSpecification.ScriptFunctionTimeout);

    /// <summary>
    /// Initializes a new instance of the <see cref="PayrollRuntimeBase"/> class
    /// </summary>
    /// <param name="settings">The runtime</param>
    protected RuntimeBase(RuntimeSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    #region Culture

    /// <inheritdoc />
    public virtual string UserCulture => Settings.UserCulture;

    #endregion

    #region Tenant

    /// <summary>The tenant</summary>
    private Tenant Tenant => Settings.Tenant;

    /// <summary>The tenant culture</summary>
    protected CultureInfo TenantCulture
    {
        get
        {
            var culture = CultureInfo.DefaultThreadCurrentCulture ?? CultureInfo.InvariantCulture;
            if (!string.IsNullOrWhiteSpace(Tenant?.Culture) &&
                !string.Equals(culture.Name, Tenant.Culture))
            {
                culture = new CultureInfo(Tenant.Culture);
            }
            return culture;
        }
    }

    /// <inheritdoc />
    public int TenantId => Tenant.Id;

    /// <inheritdoc />
    public string TenantIdentifier => Tenant.Identifier;

    /// <inheritdoc />
    public virtual object GetTenantAttribute(string attributeName) =>
        Tenant.Attributes?.GetValue<object>(attributeName);

    #endregion

    #region User

    /// <summary>
    /// The tenant
    /// </summary>
    protected User User => Settings.User;

    /// <inheritdoc />
    public int UserId => User.Id;

    /// <inheritdoc />
    public string UserIdentifier => User.Identifier;

    /// <inheritdoc />
    public virtual object GetUserAttribute(string attributeName) =>
        User.Attributes?.GetValue<object>(attributeName);

    #endregion

    #region Log and Task

    /// <summary>The log owner, the source identifier</summary>
    protected abstract string LogOwner { get; }

    /// <summary>The log owner type</summary>
    protected abstract string LogOwnerType { get; }

    /// <inheritdoc />
    public void AddLog(int level, string message, string error = null, string comment = null)
    {
        FunctionHost.AddLog(TenantId,
            new()
            {
                Level = (LogLevel)level,
                Message = message,
                User = UserIdentifier,
                Error = error,
                Comment = comment,
                Owner = LogOwner,
                OwnerType = LogOwnerType
            });
    }

    /// <inheritdoc />
    public void AddTask(string name, string instruction, DateTime scheduleDate, string category,
        Dictionary<string, object> attributes = null) =>
        FunctionHost.AddTask(TenantId,
            new()
            {
                ScheduledUserId = UserId,
                Name = name,
                Instruction = instruction,
                Scheduled = scheduleDate,
                Category = category,
                Attributes = attributes
            });

    #endregion

    #region Webhook

    /// <summary>The webhook dispatch service</summary>
    private IWebhookDispatchService WebhookDispatchService => Settings.WebhookDispatchService;

    /// <inheritdoc />
    public virtual string InvokeWebhook(string requestOperation, string requestMessage = null)
    {
        // invoke case function webhook without tracking
        var result = WebhookDispatchService.InvokeAsync(Settings.DbContext, TenantId,
            new()
            {
                Action = WebhookAction.CaseFunctionRequest,
                RequestMessage = requestMessage,
                RequestOperation = requestOperation,
                TrackMessage = false
            },
            userId: UserId).Result;
        return result;
    }

    #endregion

    /// <summary>
    /// Create a new script instance
    /// </summary>
    /// <param name="scriptType">The script type</param>
    /// <param name="item">The script object</param>
    /// <returns>New instance of the scripting type</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    protected dynamic CreateScript<T>(Type scriptType, T item) where T : IDomainObject, IScriptObject
    {
        // load assembly
        var assembly = FunctionHost.GetObjectAssembly(typeof(T), item);
        var assemblyScriptType = assembly.GetType(scriptType.FullName ?? throw new InvalidOperationException());
        if (assemblyScriptType == null)
        {
            throw new PayrollException($"Unknown script type {scriptType}.");
        }

        // script function execution
        return Activator.CreateInstance(assemblyScriptType, this);
    }
}