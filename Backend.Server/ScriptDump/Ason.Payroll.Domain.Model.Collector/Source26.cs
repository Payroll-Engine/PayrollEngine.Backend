using System;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Collector Oasi</summary>
public class CollectorOasi<TNational, TCompany, TEmployee> :
    CollectorToolBase<TNational, TCompany, TEmployee>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
{
    /// <summary>Collector Oasi constructor</summary>
    public CollectorOasi(CollectorFunction function, TEmployee employee) :
        base(function, employee)
    {
    }

    /// <summary>Get collector OASI/AHV Code</summary>
    public string GetCode() =>
        Enum.GetName(typeof(PensionContributionStatus), Employee.GetPensionContributionStatus());
}
