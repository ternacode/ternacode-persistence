using Ternacode.Persistence.EntityFrameworkCore.Enums;

namespace Ternacode.Persistence.EntityFrameworkCore.Configuration
{
    public class PersistenceOptions
    {
        /// <summary>
        /// Specifies which transaction type should be used when running code inside a IUnitOfWork method.
        /// </summary>
        public TransactionType UnitOfWorkTransactionType { get; set; }

        /// <summary>
        /// Specifies if the database context should be managed by a context pool.
        /// </summary>
        public bool UseContextPool { get; set; }

        /// <summary>
        /// Specifies if a flush to the database should only be done manually.
        /// If false, changes will be flushed to the database on each IRepository call that writes data.
        /// NOTE: A successful IUnitOfWork method call will still commit changes to the database independently of this value.
        /// </summary>
        public bool UseManualRepositoryFlush { get; set; }
    }
}