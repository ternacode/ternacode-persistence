using System;

namespace Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces
{
    internal interface ITransactionService<TTransaction> where TTransaction : IDisposable
    {
        TTransaction BeginTransaction();

        void CommitTransaction(TTransaction transaction);
    }
}