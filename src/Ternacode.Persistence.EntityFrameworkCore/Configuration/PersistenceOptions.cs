using Ternacode.Persistence.EntityFrameworkCore.Enums;

namespace Ternacode.Persistence.EntityFrameworkCore.Configuration
{
    public class PersistenceOptions
    {
        public TransactionType UnitOfWorkTransactionType { get; set; }

        public bool UseContextPool { get; set; }
    }
}