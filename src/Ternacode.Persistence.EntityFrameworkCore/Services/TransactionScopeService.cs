using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Ternacode.Persistence.EntityFrameworkCore.Configuration;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.Services
{
    internal class TransactionScopeService<TContext> : ITransactionService<TransactionScope>
        where TContext : DbContext
    {
        private readonly IContextService<TContext> _contextService;
        private readonly TransactionScopeOptions _options;

        public TransactionScopeService(
            IContextService<TContext> contextService,
            TransactionScopeOptions options)
        {
            _contextService = contextService;
            _options = options;
        }

        public TransactionScope BeginTransaction()
            => new TransactionScope(
                _options.TransactionScopeOption,
                new TransactionOptions
                {
                    IsolationLevel = _options.IsolationLevel
                },
                _options.AsyncFlowOption);

        public void CommitTransaction(TransactionScope transaction)
            => transaction.Complete();
    }
}