using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for the payrun end function
/// </summary>
public class PayrunEndRuntime : PayrunRuntime, IPayrunEndRuntime
{
    internal PayrunEndRuntime(PayrunRuntimeSettings settings) :
        base(settings)
    {
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(PayrunEndFunction);

    #region Runtime Values

    /// <inheritdoc />
    public Dictionary<string, string> GetPayrunRuntimeValues() =>
        RuntimeValueProvider.PayrunValues;

    /// <inheritdoc />
    public List<string> GetRuntimeValuesEmployees() =>
        RuntimeValueProvider.EmployeeValues.Keys.ToList();

    /// <inheritdoc />
    public Dictionary<string, string> GetEmployeeRuntimeValues(string employeeIdentifier)
    {
        if (string.IsNullOrWhiteSpace(employeeIdentifier))
        {
            throw new ArgumentException(nameof(employeeIdentifier));
        }
        return RuntimeValueProvider.EmployeeValues.TryGetValue(employeeIdentifier, out var value)
            ? value
            : new();
    }

    #endregion

    /// <summary>
    /// Execute the payrun end script
    /// </summary>
    /// <param name="payrun">The payrun</param>
    /// <returns>True if the employee is available</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal void ExecuteEndScript(Payrun payrun)
    {
        try
        {
            var task = Task.Factory.StartNew(() =>
            {
                // create script
                using var script = CreateScript(typeof(PayrunEndFunction), payrun);
                // call the script function
                script.End();
            });
            task.Wait(Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"End script error in payrun {payrun.Name}: {exception.GetBaseMessage()}", exception);
        }
    }
}