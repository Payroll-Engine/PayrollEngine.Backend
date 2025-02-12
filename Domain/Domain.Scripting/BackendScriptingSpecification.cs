//#define MAX_SCRIPT_TIMEOUT

namespace PayrollEngine.Domain.Scripting;

internal static class BackendScriptingSpecification
{
        
    /// <summary>Script function timeout in milliseconds (default: 10 seconds, test/debug: 1000 seconds)</summary>
#if MAX_SCRIPT_TIMEOUT
        internal static readonly double ScriptFunctionTimeout = 10000000;
#else
    internal static readonly double ScriptFunctionTimeout = 100000;
#endif

    /// <summary>Payrun script function timeout in milliseconds (default: 10 seconds, test/debug: 1000 seconds)</summary>
#if MAX_SCRIPT_TIMEOUT
        internal static readonly double PayrunScriptFunctionTimeout = 10000000;
#else
    internal static readonly double PayrunScriptFunctionTimeout = 100000;
#endif

}