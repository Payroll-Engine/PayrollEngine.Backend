
namespace PayrollEngine.Persistence.SqlServer.Tests
{

    public class DerivedObjectTests : DatabaseTests
    {
        // TODO rewrite test to payroll
        /*
        private static IWageTypeRepository NewWageTypeRepository(IDbContext context) =>
            new WageTypeRepository(
                new WageTypeScriptController(),
                new ScriptRepository(
                    new ScriptAuditRepository(context),
                    context),
                new WageTypeLocalizationRepository(context),
                new WageTypeAuditRepository(context),
                context);

        [Fact]
        public async void TestDerivedGrouping()
        {
            var repository = NewWageTypeRepository(Context);

            var wageTypes = (await repository.GetDerivedWageTypesAsync(22)).ToList();
            Assert.NotNull(wageTypes);
            Assert.True(wageTypes.Any());

            var wageTypeGroups = wageTypes.ToLookup(wt => wt.Number, wt => wt);
            Assert.NotNull(wageTypeGroups);
            Assert.True(wageTypeGroups.Any());

            foreach (var wageTypeGroup in wageTypeGroups)
            {
                foreach (var wageType in wageTypeGroup)
                {
                    var value = wageType;
                    Assert.NotNull(value);
                }
            }
        }
        */

    }
}
