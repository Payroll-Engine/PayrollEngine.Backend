
namespace PayrollEngine.Persistence.SqlServer.Tests
{
    public class DapperTests
    {
        /*
        protected IDbConnection Connection { get; }

        public DapperTests()
        {
            Connection = new SqlConnection(SystemSpecification.DatabaseConnectionString);
        }

        [Fact]
        public async void ShouldReturnExistingRecordAsync()
        {
            var id = 1;
            var count = await Connection.ExecuteScalarAsync<int>(
                $"select count(1) from {DbSchema.Tables.Tenant} where {DbSchema.Columns.Object.Id}=@id", new { id });
            var exists = count == 1;
            Assert.True(exists);
        }

        [Fact]
        public async void ShouldReturnNotExistingRecordAsync()
        {
            var id = -1;
            var count = await Connection.ExecuteScalarAsync<int>(
                $"select count(1) from {DbSchema.Tables.Tenant} where {DbSchema.Columns.Object.Id}=@id", new { id });
            var exists = count == 1;
            Assert.False(exists);
        }
        */
    }
}
