
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence.SqlServer.Tests
{
    public abstract class DatabaseTests
    {
        protected IDbContext Context { get; }

        protected DatabaseTests()
        {
            Context = new DbContext(SystemSpecification.DatabaseConnectionString);
        }
    }
}
