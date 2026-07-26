using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OpenBrewery.Core.Entities;
using OpenBrewery.Infrastructure.Persistence.Context;
using OpenBrewery.Infrastructure.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBrewery.Tests.Integration.Repositories
{
    public class BreweryRepositoryTests
    {

        [Fact]
        public async Task GetAllAsync_ShouldReturnBreweriesFromDatabase()
        {
            //arrange
            SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<BreweryDbContext>()
                                .UseSqlite(connection)
                                .Options;

            await using var context = new BreweryDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repository = new BreweryRepository(context);

            var breweries = new List<Brewery>
            { 
                new Brewery
                {
                    Name = "Brewery One",
                    City = "Pune",
                    BreweryType = "micro"
                },
                new Brewery
                {
                    Name = "Brewery Two",
                    City = "Mumbai",
                    BreweryType = "brewpub"
                }

            };
            await repository.SeedAsync(breweries);
            //act
            var result = await context.Breweries.ToListAsync();
            
            //assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.Name == "Brewery One");
            Assert.Contains(result, x => x.Name == "Brewery Two");
        }
    }
}
