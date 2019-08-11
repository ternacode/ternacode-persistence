using System.Transactions;

namespace Ternacode.Persistence.EntityFrameworkCore.Configuration
{
    public class TransactionScopeOptions
    {
        public TransactionScopeOption TransactionScopeOption { get; set; }

        public TransactionScopeAsyncFlowOption AsyncFlowOption { get; set; }

        public IsolationLevel IsolationLevel { get; set; }
    }
}