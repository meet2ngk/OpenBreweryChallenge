using Microsoft.EntityFrameworkCore.Storage;
using OpenBrewery.Core.Interfaces;
using OpenBrewery.Infrastructure.Persistence.Context;

namespace OpenBrewery.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BreweryDbContext _context;

        private IDbContextTransaction? _transaction;

        public UnitOfWork(BreweryDbContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction =
                await _context.Database.BeginTransactionAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException(
                    "No active transaction exists.");
            }

            await _transaction.CommitAsync();

            await _transaction.DisposeAsync();

            _transaction = null;
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                return;
            }

            await _transaction.RollbackAsync();

            await _transaction.DisposeAsync();

            _transaction = null;
        }
    }
}