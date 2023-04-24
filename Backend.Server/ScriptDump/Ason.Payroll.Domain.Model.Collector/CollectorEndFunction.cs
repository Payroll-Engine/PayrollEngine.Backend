/* CollectorEndFunction */
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper restore RedundantUsingDirective

namespace Ason.Payroll.Client.Scripting.Function;

/// <summary>End the collector</summary>
// ReSharper disable once PartialTypeWithSinglePart
public partial class CollectorEndFunction : CollectorFunction
{
    /// <summary>Initializes a new instance with the function runtime</summary>
    /// <param name="runtime">The runtime</param>
    public CollectorEndFunction(object runtime) :
        base(runtime)
    {
    }

    /// <summary>New function instance without runtime (scripting development)</summary>
    /// <remarks>Use <see cref="Function.GetSourceFileName"/> in your constructor for the source file name</remarks>
    /// <param name="sourceFileName">The name of the source file</param>
    protected CollectorEndFunction(string sourceFileName) :
        base(sourceFileName)
    {
    }

    /// <summary>Get collector values</summary>
    public decimal[] GetValues() => Runtime.GetValues();

    /// <summary>Set collector values</summary>
    public void SetValues(decimal[] values) => Runtime.SetValues(values);

    /// <exclude />
    public object End()
    {
        SetResultTags(Swissdec.Collectors.Dsa.GetCodes());
        // compiler will optimize this out if the code provides a return
#pragma warning disable 162
        return default;
#pragma warning restore 162
    }
}