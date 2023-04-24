/* CollectorEnd.Register */

namespace Ason.Payroll.Client.Scripting.Function
{
    public partial class CollectorEndFunction
    {
        private Ason.Regulation.Swissdec5.CollectorEnd swissdec;
        public Ason.Regulation.Swissdec5.CollectorEnd Swissdec => swissdec ??= new(this);
    }
}
