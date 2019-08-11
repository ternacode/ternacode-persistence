using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ternacode.Persistence.EntityFrameworkCore.UnitTest.Fakes
{
    public class DatabaseFacade_Fake : DatabaseFacade
    {
        private readonly CustomAutoFixture _fixture;

        public IDbContextTransaction Transaction { get; set; }

        public DatabaseFacade_Fake(DbContext context)
            : base(context)
        {
            _fixture = new CustomAutoFixture();
            Transaction = _fixture.Create<IDbContextTransaction>();
        }

        public override IDbContextTransaction BeginTransaction()
            => Transaction;

        public override Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
            => Task.FromResult(_fixture.Create<IDbContextTransaction>());
    }
}