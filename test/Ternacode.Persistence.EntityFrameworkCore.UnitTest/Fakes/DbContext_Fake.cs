using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ternacode.Persistence.EntityFrameworkCore.UnitTest.Fakes
{
    public class DbContext_Fake : DbContext
    {
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