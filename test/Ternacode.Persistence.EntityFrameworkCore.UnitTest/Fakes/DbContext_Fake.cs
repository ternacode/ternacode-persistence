using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ternacode.Persistence.EntityFrameworkCore.UnitTest.Fakes
{
    public class DbContext_Fake : DbContext
    {
        private readonly IDictionary<string, int> _calls = new Dictionary<string, int>();

        public DbContext_Fake()
        {
            Database = new DatabaseFacade_Fake(this);
        }

        public override DatabaseFacade Database { get; }

        public override int SaveChanges()
            => default;

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
            => Task.FromResult(default(int));
    }
}