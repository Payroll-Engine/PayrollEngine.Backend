using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Collector Suva</summary>
public class CollectorSuva<TNational, TCompany, TEmployee> :
    CollectorToolBase<TNational, TCompany, TEmployee>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
{
    /// <summary>Collector Suva constructor</summary>
    public CollectorSuva(CollectorFunction function, TEmployee employee) :
        base(function, employee)
    {
    }

    /// <summary>Get collector Suva Code</summary>
    public string GetCode()
    {
        var employeeUvgCode = Function.CaseValue[CaseFieldName.EmployeeUvgInsuranceCode];
        if (employeeUvgCode.HasValue && !string.IsNullOrWhiteSpace(employeeUvgCode.Value.ToString()))
        {
            return employeeUvgCode.Value.ToString();
        }
        return string.Empty;
    }
}
