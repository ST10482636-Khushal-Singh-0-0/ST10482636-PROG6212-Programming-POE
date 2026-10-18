using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

namespace RaceDay.Tests
{
    public static class TestDatabaseFixture
    {
        public static RaceDayDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new RaceDayDbContext(options);
        }
    }
}