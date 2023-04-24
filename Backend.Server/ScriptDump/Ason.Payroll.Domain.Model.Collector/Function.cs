/* Function */
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ason.Payroll.Client.Scripting.Function;

/// <summary>Base class for any scripting function</summary>
// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Function : IDisposable
{
    /// <summary>The function runtime</summary><exclude />
    protected dynamic Runtime { get; }

    /// <summary>New function instance</summary>
    /// <param name="runtime">The function runtime</param>
    protected Function(object runtime)
    {
        Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));

        // tenant
        TenantId = Runtime.TenantId;
        TenantIdentifier = Runtime.TenantIdentifier;
        // user
        UserId = Runtime.UserId;
        UserIdentifier = Runtime.UserIdentifier;
        UserLanguage = (Language)Runtime.UserLanguage;
    }

    #region Tenant

    /// <summary>The tenant id</summary>
    public int TenantId { get; }

    /// <summary>The tenant identifier</summary>
    public string TenantIdentifier { get; }

    /// <summary>Get tenant attribute value</summary>
    public object GetTenantAttribute(string attributeName) =>
        Runtime.GetTenantAttribute(attributeName);

    /// <summary>Get tenant attribute typed value</summary>
    public T GetTenantAttribute<T>(string attributeName, T defaultValue = default)
    {
        var value = GetTenantAttribute(attributeName);
        return value == null ? defaultValue : (T)Convert.ChangeType(value, typeof(T));
    }

    #endregion

    #region User

    /// <summary>The user id</summary>
    public int UserId { get; }

    /// <summary>The user language</summary>
    public Language UserLanguage { get; }

    /// <summary>The user identifier</summary>
    public string UserIdentifier { get; }

    /// <summary>Get user attribute value</summary>
    public object GetUserAttribute(string attributeName) =>
        Runtime.GetUserAttribute(attributeName);

    /// <summary>Get user attribute typed value</summary>
    public T GetUserAttribute<T>(string attributeName, T defaultValue = default)
    {
        var value = GetUserAttribute(attributeName);
        return value == null ? defaultValue : (T)Convert.ChangeType(value, typeof(T));
    }

    #endregion

    #region Log and Task

    /// <summary>Add a verbose log</summary>
    /// <param name="message">The log message</param>
    /// <param name="error">The log error</param>
    /// <param name="comment">The log comment</param>
    public void LogVerbose(string message, string error = null, string comment = null) =>
        Log(LogLevel.Verbose, message, error, comment);

    /// <summary>Add a debug log</summary>
    /// <param name="message">The log message</param>
    /// <param name="error">The log error</param>
    /// <param name="comment">The log comment</param>
    public void LogDebug(string message, string error = null, string comment = null) =>
        Log(LogLevel.Debug, message, error, comment);

    /// <summary>Add a information log</summary>
    /// <param name="message">The log message</param>
    /// <param name="error">The log error</param>
    /// <param name="comment">The log comment</param>
    public void LogInformation(string message, string error = null, string comment = null) =>
        Log(LogLevel.Information, message, error, comment);

    /// <summary>Add a warning log</summary>
    /// <param name="message">The log message</param>
    /// <param name="error">The log error</param>
    /// <param name="comment">The log comment</param>
    public void LogWarning(string message, string error = null, string comment = null) =>
        Log(LogLevel.Warning, message, error, comment);

    /// <summary>Add a error log</summary>
    /// <param name="message">The log message</param>
    /// <param name="error">The log error</param>
    /// <param name="comment">The log comment</param>
    public void LogError(string message, string error = null, string comment = null) =>
        Log(LogLevel.Error, message, error, comment);

    /// <summary>Add a error log using an exception</summary>
    /// <param name="exception">The exception</param>
    /// <param name="message">The log message, default is the exception message</param>
    /// <param name="comment">The log comment</param>
    public void LogError(Exception exception, string message = null, string comment = null)
    {
        message ??= exception.GetBaseException().Message;
        LogError(message, exception.ToString(), comment);
    }

    /// <summary>Add a fatal log</summary>
    /// <param name="message">The log message</param>
    /// <param name="error">The log error</param>
    /// <param name="comment">The log comment</param>
    public void LogFatal(string message, string error = null, string comment = null) =>
        Log(LogLevel.Fatal, message, error, comment);

    /// <summary>Add a fatal log using an exception</summary>
    /// <param name="exception">The exception</param>
    /// <param name="message">The log message, default is the exception message</param>
    /// <param name="comment">The log comment</param>
    public void LogFatal(Exception exception, string message = null, string comment = null)
    {
        message ??= exception.GetBaseException().Message;
        LogFatal(message, exception.ToString(), comment);
    }

    /// <summary>Add a log</summary>
    /// <param name="level">The log level</param>
    /// <param name="message">The log message</param>
    /// <param name="error">The log error</param>
    /// <param name="comment">The log comment</param>
    public void Log(LogLevel level, string message, string error = null, string comment = null) =>
        Runtime.AddLog((int)level, message, error, comment);

    /// <summary>Add task</summary>
    /// <param name="name">The task name</param>
    /// <param name="instruction">The task instruction</param>
    /// <param name="scheduleDate">The task schedule date</param>
    /// <param name="category">The task category</param>
    /// <param name="attributes">The task attributes</param>
    public void AddTask(string name, string instruction, DateTime scheduleDate, string category = null,
        Dictionary<string, object> attributes = null) =>
        Runtime.AddTask(name, instruction, scheduleDate, category);

    #endregion

    #region Scripting Development

    /// <summary>The name of the source file (scripting development)</summary>
    /// <value>The name of the source file</value>
    public string SourceFileName { get; }

    /// <summary>New function instance without runtime (scripting development)</summary>
    /// <remarks>Use <see cref="GetSourceFileName"/> in your constructor for the source file name</remarks>
    /// <param name="sourceFileName">The name of the source file</param>
    protected Function(string sourceFileName)
    {
        if (string.IsNullOrWhiteSpace(sourceFileName))
        {
            throw new ArgumentException(nameof(sourceFileName));
        }
        SourceFileName = sourceFileName;
    }

    /// <summary>Initialize the source file path (scripting development)</summary>
    /// <param name="sourceFilePath">The source file path (do not provide a value)</param>
    /// <returns>Source code file name</returns>
    protected static string GetSourceFileName(
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
    {
        return sourceFilePath;
    }

    #endregion

    #region Extension Methods

    /// <summary>Get type extension methods from the current assembly</summary>
    /// <param name="returnType">The extension method return type</param>
    /// <returns>List of extension methods</returns>
    protected List<MethodInfo> GetExtensionMethods(Type returnType = null)
    {
        var targetType = GetType();
        var methods = new List<MethodInfo>();
        Assembly assembly = targetType.Assembly;
        // assembly types
        foreach (var type in assembly.GetTypes())
        {
            // static
            if (!type.IsSealed || type.IsGenericType || type.IsNested)
            {
                continue;
            }

            // function extension attributes
            foreach (var attribute in type.GetCustomAttributes<FunctionExtensionsAttribute>())
            {
                if (attribute.TargetType == targetType)
                {
                    // type extension methods
                    foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                    {
                        // return type
                        if (returnType != null && method.ReturnParameter?.ParameterType != returnType)
                        {
                            continue;
                        }

                        // extension method parameters
                        var parameters = method.GetParameters();
                        if (parameters.Length > 0)
                        {
                            if (targetType.IsAssignableFrom(parameters[0].ParameterType))
                            {
                                methods.Add(method);
                            }
                        }
                    }
                }
            }
        }

        return methods;
    }

    /// <summary>Invoke extension method</summary>
    /// <param name="method">The method to invoke</param>
    /// <returns>The method result</returns>
    protected T InvokeExtensionMethod<T>(MethodInfo method) =>
        (T)method.Invoke(null, new object[] { this });

    #endregion

    /// <summary>Dispose the function</summary>
    public virtual void Dispose() =>
        GC.SuppressFinalize(this);
}

/// <summary>Attribute for function extension methods</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class FunctionExtensionsAttribute : Attribute
{
    /// <summary>The extension target type</summary>
    public Type TargetType { get; }

    /// <summary>Initializes a new instance of the <see cref="FunctionExtensionsAttribute"/> class</summary>
    /// <param name="targetType">The extension target type</param>
    public FunctionExtensionsAttribute(Type targetType)
    {
        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
    }
}