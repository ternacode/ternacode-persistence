using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.UnitOfWork
{
    internal class TransactionScopeUnitOfWork<TContext> : BaseDbContextUnitOfWork<TContext, TransactionScope>
        where TContext : DbContext
    {
        public TransactionScopeUnitOfWork(IContextService<TContext> contextService)
            : base(contextService)
        {
        }

        protected override TransactionScope BeginTransaction()
            => CreateTransactionScope();

        protected override void CommitTransaction(TransactionScope transaction)
            => transaction.Complete();

        // TODO: Make configurable
        private TransactionScope CreateTransactionScope(
            TransactionScopeOption transactionScopeOption = TransactionScopeOption.Required,
            TransactionScopeAsyncFlowOption asyncFlowOption = TransactionScopeAsyncFlowOption.Enabled,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
            => new TransactionScope(
                transactionScopeOption,
                new TransactionOptions
                {
                    IsolationLevel = isolationLevel
                },
                asyncFlowOption);
    }
}