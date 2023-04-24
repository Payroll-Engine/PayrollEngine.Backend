/* CollectorApply.Register */

namespace Ason.Payroll.Client.Scripting.Function
{
    public partial class CollectorApplyFunction
    {
        private Ason.Regulation.Swissdec5.CollectorApply swissdec;
        public Ason.Regulation.Swissdec5.CollectorApply Swissdec => swissdec ??= new(this);
    }
}
