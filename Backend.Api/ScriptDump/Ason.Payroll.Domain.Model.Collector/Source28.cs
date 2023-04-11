using System;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Collector tools</summary>
public abstract class CollectorToolBase<TNational, TCompany, TEmployee>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
{
    /// <summary>Collector Oasi constructor</summary>
    protected CollectorToolBase(CollectorFunction function, TEmployee employee)
    {
        Function = function ?? throw new ArgumentNullException(nameof(function));
        Employee = employee ?? throw new ArgumentNullException(nameof(employee));
    }

    /// <summary>The function</summary>
    protected CollectorFunction Function { get; }

    /// <summary>The employee</summary>
    protected TEmployee Employee { get; }
}
