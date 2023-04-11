/* WageTypeResult.Register */

namespace Ason.Payroll.Client.Scripting.Function
{
    public partial class WageTypeResultFunction
    {
        private Ason.Regulation.Swissdec5.WageTypeResult swissdec;
        public Ason.Regulation.Swissdec5.WageTypeResult Swissdec => swissdec ??= new(this);
    }
}
