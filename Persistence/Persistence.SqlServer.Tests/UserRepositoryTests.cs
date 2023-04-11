namespace PayrollEngine.Persistence.SqlServer.Tests
{
    public class UserRepositoryTests : DatabaseTests
    {
        /*
        [Fact]
        public async void ShouldReturnAnyUsersAsync()
        {
            var repository = new UserRepository(ConnectionString);
            var users = await repository.GetAllAsync();
            Assert.NotNull(users);
            Assert.True(users.Any());
        }

        [Fact]
        public async void ShouldReturnAllActiveUsersAsync()
        {
            var repository = new UserRepository(ConnectionString);
            var users = await repository.GetAllAsync(ObjectStatus.Active);
            Assert.NotNull(users);
            Assert.True(users.Any());
        }

        [Fact]
        public async void ShouldReturnAllInactiveUsersAsync()
        {
            var repository = new UserRepository(ConnectionString);
            var users = await repository.GetAllAsync(ObjectStatus.Inactive);
            Assert.NotNull(users);
            Assert.True(users.Any());
        }

        [Fact]
        public async void ShouldAddActiveUserAsync()
        {
            User user = new User
            {
                Identifier = "pert.smith@foo.com",
                FirstName = "Peter",
                LastName = "Smith"
            };

            var repository = new UserRepository(ConnectionString);
            var addedUser = await repository.AddAsync(user);
            Assert.NotNull(addedUser);
            Assert.NotEqual(default, user.Id);
            Assert.Equal("Active User", user.Name);
            Assert.Equal(ObjectStatus.Active, user.Status);
        }

        [Fact]
        public async void ShouldAddInactiveUserAsync()
        {
            User user = new User
            {
                Name = "Inactive User",
                Status = ObjectStatus.Inactive
            };

            var repository = new UserRepository(ConnectionString);
            var addedUser = await repository.AddAsync(user);
            Assert.NotNull(addedUser);
            Assert.NotEqual(default, user.Id);
            Assert.Equal("Inactive User", user.Name);
            Assert.Equal(ObjectStatus.Inactive, user.Status);
        }
        */
    }
}
