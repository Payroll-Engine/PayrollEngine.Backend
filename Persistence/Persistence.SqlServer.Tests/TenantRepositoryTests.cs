
namespace PayrollEngine.Persistence.SqlServer.Tests
{
    public class TenantRepositoryTests : DatabaseTests
    {
        /*
        [Fact]
        public async void ShouldReturnAnyTenantsAsync()
        {
            var repository = new TenantRepository(Context);
            var tenants = await repository.GetAllAsync();
            Assert.NotNull(tenants);
            Assert.True(tenants.Any());
        }

        [Fact]
        public async void ShouldReturnAllActiveTenantsAsync()
        {
            var repository = new TenantRepository(Context);
            var tenants = await repository.GetAllAsync(ObjectQueryParameters.ActiveObjects);
            Assert.NotNull(tenants);
            Assert.True(tenants.Any());
        }

        [Fact]
        public async void ShouldReturnAllInactiveTenantsAsync()
        {
            var repository = new TenantRepository(Context);
            var tenants = await repository.GetAllAsync(ObjectQueryParameters.InactiveObjects);
            Assert.NotNull(tenants);
            Assert.True(tenants.Any());
        }

        [Fact]
        public async void ShouldReturnExistingTenantAsync()
        {
            var repository = new TenantRepository(Context);
            var exists = await repository.ExistsAsync(1);
            Assert.True(exists);
        }

        [Fact]
        public async void ShouldReturnNotExistingTenantAsync()
        {
            var repository = new TenantRepository(Context);
            var exists = await repository.ExistsAsync(int.MaxValue);
            Assert.False(exists);
        }

        [Fact]
        public async void ShouldAddActiveTenantAsync()
        {
            Tenant tenant = new Tenant
            {
                Identifier = "Active Tenant",
                Status = ObjectStatus.Active
            };

            var repository = new TenantRepository(Context);
            var addedTenant = await repository.CreateAsync(tenant);
            Assert.NotNull(addedTenant);
            Assert.NotEqual(default, tenant.Id);
            Assert.Equal("Active Tenant", tenant.Identifier);
            Assert.Equal(ObjectStatus.Active, tenant.Status);
        }

        [Fact]
        public async void ShouldAddInactiveTenantAsync()
        {
            Tenant tenant = new Tenant
            {
                Identifier = "Inactive Tenant",
                Status = ObjectStatus.Inactive
            };

            var repository = new TenantRepository(Context);
            var addedTenant = await repository.CreateAsync(tenant);
            Assert.NotNull(addedTenant);
            Assert.NotEqual(default, tenant.Id);
            Assert.Equal("Inactive Tenant", tenant.Identifier);
            Assert.Equal(ObjectStatus.Inactive, tenant.Status);
        }
        */
    }
}
