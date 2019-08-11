using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ternacode.Persistence.EntityFrameworkCore.UnitTest.Fakes
{
    // TODO: Use NSubstitute
    public class DbContext_Fake : DbContext
    {
        private readonly IDictionary<string, int> _calls = new Dictionary<string, int>();

        public DbContext_Fake()
        {
            Database = new DatabaseFacade_Fake(this);
        }

        public override DatabaseFacade Database { get; }

        public override int SaveChanges()
            => CountCall<int>();

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
            => Task.FromResult(CountCall<int>());

        public int GetCallCount(string methodName)
            => _calls[methodName];

        private T CountCall<T>([CallerMemberName] string caller = null, T value = default)
        {
            if (!_calls.TryGetValue(caller, out _))
                _calls.Add(caller, 0);

            _calls[caller]++;

            return value;
        }
    }
}