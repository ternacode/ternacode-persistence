using System;
using System.Threading.Tasks;

namespace Ternacode.Persistence.Abstractions
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Runs an action inside a transaction.
        /// </summary>
        void Run(Action action);

        /// <summary>
        /// Runs a func inside a transaction, returning the TEntity result.
        /// </summary>
        TEntity Run<TEntity>(Func<TEntity> func);

        /// <summary>
        /// Runs an async func inside a transaction.
        /// </summary>
        Task RunAsync(Func<Task> funcAsync);

        /// <summary>
        /// Runs an async func inside a transaction, returning the TEntity result.
        /// </summary>
        Task<TEntity> RunAsync<TEntity>(Func<Task<TEntity>> funcAsync);
    }
}