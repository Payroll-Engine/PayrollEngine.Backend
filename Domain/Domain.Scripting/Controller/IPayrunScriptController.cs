using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting.Runtime;

namespace PayrollEngine.Domain.Scripting.Controller;

/// <summary>
/// Payrun script controller
/// </summary>
public interface IPayrunScriptController : IScriptController<Payrun>
{
    /// <summary>
    /// Execute payrun start script
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    bool? Start(PayrunRuntimeSettings settings);

    /// <summary>
    /// Test if the employee is available for a payrun
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    /// <returns>The case value at a given time, null if no value is available</returns>
    bool? IsEmployeeAvailable(PayrunRuntimeSettings settings);

    /// <summary>
    /// Employee start function
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    /// <returns>True if the employee can be processed</returns>
    bool? EmployeeStart(PayrunRuntimeSettings settings);

    /// <summary>
    /// Employee end function
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    /// <returns>True if the employee can be processed</returns>
    void EmployeeEnd(PayrunRuntimeSettings settings);

    /// <summary>
    /// Test if a wage type for an employee is available for a payrun
    /// </summary>
    /// <param name="wageType">The wage type</param>
    /// <param name="wageTypeAttributes">The wage type attributes</param>
    /// <param name="settings">The runtime settings</param>
    /// <returns>The case value at a given time, null if no value is available</returns>
    bool? IsWageTypeAvailable(WageType wageType, Dictionary<string, object> wageTypeAttributes,
        PayrunRuntimeSettings settings);

    /// <summary>
    /// Execute payrun end script
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    void End(PayrunRuntimeSettings settings);
}