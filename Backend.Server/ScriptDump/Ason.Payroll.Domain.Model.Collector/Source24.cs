using System.Collections.Generic;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Collector Qst</summary>
public class CollectorQst<TNational, TCompany, TEmployee> :
    CollectorToolBase<TNational, TCompany, TEmployee>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
{
    /// <summary>Collector Oasi constructor</summary>
    public CollectorQst(CollectorFunction function, TEmployee employee) :
        base(function, employee)
    {
    }

    /// <summary>Get the employee QST code, canton and calculation type</summary>
    /// <returns>A string array of QST code, canton and calculation type</returns>
    public string[] GetCodes()
    {
        var qstCodes = new string[3];

        var qstCode = Function.GetPeriodCaseValue(Function.Period, CaseFieldName.EmployeeQstTaxCode);
        var qstCanton = Function.GetPeriodCaseValue(Function.Period, CaseFieldName.EmployeeQstTaxCanton);
        var qstCalculationType = string.Empty;

        if (qstCanton.HasValue)
        {
            var qstCantonData = Function.GetLookup<Dictionary<string, object>>(LookupName.QstCantons, qstCanton.Value.ToString());
            qstCalculationType = qstCantonData["CalculationCycle"].ToString();
        }

        qstCodes[0] = qstCode.HasValue ? qstCode.Value.ToString() : string.Empty;
        qstCodes[1] = qstCanton.HasValue ? qstCanton.Value.ToString() : string.Empty;
        qstCodes[2] = qstCalculationType;

        return qstCodes;
    }
}
