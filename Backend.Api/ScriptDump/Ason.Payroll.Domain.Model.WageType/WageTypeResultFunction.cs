/* WageTypeResultFunction */
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper restore RedundantUsingDirective

namespace Ason.Payroll.Client.Scripting.Function;

/// <summary>Setup wage type results</summary>
/// <seealso cref="WageTypeValueFunction">Wage Type Value Function</seealso>
// ReSharper disable once PartialTypeWithSinglePart
public partial class WageTypeResultFunction : WageTypeFunction
{
    /// <summary>Initializes a new instance with the function runtime</summary>
    /// <param name="runtime">The runtime</param>
    public WageTypeResultFunction(object runtime) :
        base(runtime)
    {
        // wage type
        WageTypeValue = Runtime.WageTypeValue;
    }

    /// <summary>New function instance without runtime (scripting development)</summary>
    /// <remarks>Use <see cref="Function.GetSourceFileName"/> in your constructor for the source file name</remarks>
    /// <param name="sourceFileName">The name of the source file</param>
    protected WageTypeResultFunction(string sourceFileName) :
        base(sourceFileName)
    {
    }

    /// <summary>The wage type value</summary>
    public decimal WageTypeValue { get; }

    /// <exclude />
    public object Result()
    {
        Swissdec.SetResultAttributes();
        // compiler will optimize this out if the code provides a return
#pragma warning disable 162
        return default;
#pragma warning restore 162
    }
}