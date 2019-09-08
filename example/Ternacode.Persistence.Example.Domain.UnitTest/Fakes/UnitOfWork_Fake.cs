using System;
using System.Threading.Tasks;
using Ternacode.Persistence.Abstractions;

namespace Ternacode.Persistence.Example.Domain.UnitTest.Fakes
{
    public class UnitOfWork_Fake : IUnitOfWork
    {
        public void Run(Action action)
        {
            action?.Invoke();
        }

        public T Run<T>(Func<T> func)
            => func != null ? func() : default;

        public async Task RunAsync(Func<Task> funcAsync)
        {
            if (funcAsync != null)
                await funcAsync();
        }

        public async Task<T> RunAsync<T>(Func<Task<T>> funcAsync)
        {
            if (funcAsync != null)
                return await funcAsync();

            return default;
        }
    }
}