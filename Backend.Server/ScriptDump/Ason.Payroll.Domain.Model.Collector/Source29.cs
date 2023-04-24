using System.Linq;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Collector Sai</summary>
public class CollectorSai<TNational, TCompany, TEmployee> :
    CollectorToolBase<TNational, TCompany, TEmployee>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
{
    /// <summary>Collector Oasi constructor</summary>
    public CollectorSai(CollectorFunction function, TEmployee employee) :
        base(function, employee)
    {
    }

    /// <summary>Get collector UVGZ Code</summary>
    public string[] GetCodes()
    {
        // string array size is number of employee slots
        var codes = new string[Specification.MaxEmployeeInsuranceCodes];
        for (var i = 1; i <= Employee.Uvgz.InsuranceCount; i++)
        {
            codes[i] = Employee.Uvgz.Insurances[i].Code;
        }
        return codes.Distinct().Where(x => x != null).ToArray();
    }
}
