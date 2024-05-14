using Microsoft.EntityFrameworkCore.Storage;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        public IRepositoryWrapper Repositories { get; set; }
        private IDbContextTransaction? _currentTransaction;
        private ApplicationDbContext _context;

        public UnitOfWork(IRepositoryWrapper repositories, 
            ApplicationDbContext context)
        {
            Repositories = repositories;
            _context = context;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
             _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is not null)
                await _currentTransaction.CommitAsync(cancellationToken);
        }

        public async Task RollBackAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is not null)
                await _currentTransaction.RollbackAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
