namespace PayrollEngine.Api.Core;

/// <summary>
/// Controller visibility
/// </summary>
public interface IControllerVisibility
{
    /// <summary>
    /// Get visible controllers
    /// </summary>
    /// <param name="config">Server configuration</param>
    public string[] GetVisibleControllers(PayrollServerConfiguration config);

    /// <summary>
    /// Get hidden controllers
    /// </summary>
    /// <param name="config">Server configuration</param>
    public string[] GetHiddenControllers(PayrollServerConfiguration config);
}