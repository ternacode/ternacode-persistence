using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.Services
{
    internal class DbContextTransactionService<TContext> : ITransactionService<IDbContextTransaction>
        where TContext : DbContext
    {
        private readonly IContextService<TContext> _contextService;

        public DbContextTransactionService(IContextService<TContext> contextService)
        {
            _contextService = contextService;
        }

        public IDbContextTransaction BeginTransaction()
            => _contextService.GetCurrentContext().Database.BeginTransaction();

        public void CommitTransaction(IDbContextTransaction transaction)
            => transaction.Commit();
    }
}