/* PayrollCompany */
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Swissdec Payroll Company</summary>
public class PayrollCompany<TNational> : Company<TNational>
    where TNational : PayrollNational
{
    /// <summary>Function constructor</summary>
    public PayrollCompany(PayrollFunction function, TNational national) :
        base(function, national)
    {
    }

    #region Company Timeless Shared Lookups

    /// <summary>Get Workplace</summary>
    public WorkplaceData GetWorkplace(string workplaceId) =>
        Function.GetLookup<WorkplaceData>(workplaceId);

    #endregion

}
