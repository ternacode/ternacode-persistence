using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ternacode.Persistence.Abstractions;

namespace Ternacode.Persistence.Example.Domain.UnitTest.Fakes
{
    // TODO: Use NSubstitute
    public class UnitOfWork_Fake : IUnitOfWork
    {
        private readonly IDictionary<string, int> _calls = new Dictionary<string, int>();

        public void Run(Action action)
        {
            CountCall();

            action?.Invoke();
        }

        public T Run<T>(Func<T> func)
        {
            CountCall();

            return func != null ? func() : default;
        }

        public async Task RunAsync(Func<Task> funcAsync)
        {
            CountCall();

            if (funcAsync != null)
                await funcAsync();
        }

        public async Task<T> RunAsync<T>(Func<Task<T>> funcAsync)
        {
            CountCall();

            if (funcAsync != null)
                return await funcAsync();

            return default;
        }

        public int GetCallCount(string methodName)
            => _calls[methodName];

        private void CountCall([CallerMemberName] string caller = null)
        {
            if (!_calls.TryGetValue(caller, out _))
                _calls.Add(caller, 0);

            _calls[caller]++;
        }
    }
}