using System.Collections.Generic;
// ReSharper disable UnusedMemberInSuper.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Represents a scripting domain object
/// </summary>
public interface IScriptObject : IDomainObject
{
    /// <summary>
    /// The script text
    /// </summary>
    string Script { get; set; }

    /// <summary>
    /// The script version
    /// </summary>
    string ScriptVersion { get; set; }

    /// <summary>
    /// The script bits
    /// </summary>
    byte[] Binary { get; set; }

    /// <summary>
    /// The script hash value
    /// </summary>
    int ScriptHash { get; set; }

    /// <summary>
    /// True, if any expression is available
    /// </summary>
    bool HasAnyExpression { get; }

    /// <summary>
    /// True, if any action is available
    /// </summary>
    bool HasAnyAction { get; }

    /// <summary>
    /// True, if object scripts are supported
    /// </summary>
    bool HasObjectScripts { get; }

    /// <summary>
    /// Get supported function types
    /// </summary>
    List<FunctionType> GetFunctionTypes();

    /// <summary>
    /// Clear script
    /// </summary>
    void Clear();

    /// <summary>
    /// Get embedded scripts
    /// </summary>
    /// <returns>Name list of embedded scripts</returns>
    IEnumerable<string> GetEmbeddedScriptNames();

    /// <summary>
    /// Get object function script
    /// </summary>
    /// <param name="functionType">Function type</param>
    string GetFunctionScript(FunctionType functionType);

    /// <summary>
    /// Get object function actions
    /// </summary>
    /// <param name="functionType">Function type</param>
    List<string> GetFunctionActions(FunctionType functionType);
}