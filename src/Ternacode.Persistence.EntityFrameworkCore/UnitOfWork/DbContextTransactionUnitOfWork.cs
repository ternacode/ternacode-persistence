using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.UnitOfWork
{
    internal class DbContextTransactionUnitOfWork<TContext> : BaseDbContextUnitOfWork<TContext, IDbContextTransaction>
        where TContext : DbContext
    {
        public DbContextTransactionUnitOfWork(IContextService<TContext> contextService)
            : base(contextService)
        {
        }

        protected override void CommitTransaction(IDbContextTransaction transaction)
            => transaction.Commit();

        protected override IDbContextTransaction BeginTransaction()
            => _contextService.GetCurrentContext().Database.BeginTransaction();
    }
}