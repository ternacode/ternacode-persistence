using System;
using System.Threading.Tasks;

namespace Ternacode.Persistence.Abstractions
{
    public interface IUnitOfWork
    {
        void Run(Action action);

        TEntity Run<TEntity>(Func<TEntity> func);

        Task RunAsync(Func<Task> funcAsync);

        Task<TEntity> RunAsync<TEntity>(Func<Task<TEntity>> funcAsync);
    }
}