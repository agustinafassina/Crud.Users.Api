using Microsoft.EntityFrameworkCore;
using UsersApi.Repository.SqlServer;

namespace UsersApi.Tests.Helpers;

public static class TestDbContextFactory
{
    public static UserDbContext Create(string databaseName)
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var context = new UserDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
