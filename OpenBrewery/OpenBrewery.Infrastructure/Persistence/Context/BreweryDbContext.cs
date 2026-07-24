using Microsoft.EntityFrameworkCore;
using OpenBrewery.Infrastructure.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenBrewery.Core.Entities;

namespace OpenBrewery.Infrastructure.Persistence.Context
{
    public class BreweryDbContext : DbContext
    {
        public BreweryDbContext(DbContextOptions<BreweryDbContext> options) : base(options) 
        { 
        }

        public DbSet<Core.Entities.Brewery> Breweries => Set<Core.Entities.Brewery>();
    }
}
