/* WageTypeValue.Register */

namespace Ason.Payroll.Client.Scripting.Function
{
    public partial class WageTypeValueFunction
    {
        private Ason.Regulation.Swissdec5.WageTypeValue swissdec;
        public Ason.Regulation.Swissdec5.WageTypeValue Swissdec => swissdec ??= new(this);
    }
}
