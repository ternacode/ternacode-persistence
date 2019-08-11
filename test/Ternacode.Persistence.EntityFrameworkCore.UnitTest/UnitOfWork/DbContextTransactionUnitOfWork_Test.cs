using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;
using NUnit.Framework;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;
using Ternacode.Persistence.EntityFrameworkCore.UnitOfWork;
using Ternacode.Persistence.EntityFrameworkCore.UnitTest.Fakes;

namespace Ternacode.Persistence.EntityFrameworkCore.UnitTest.UnitOfWork
{
    [TestFixture]
    public class DbContextTransactionUnitOfWork_Test
    {
        private CustomAutoFixture _fixture;

        private DbContext_Fake _context;
        private IContextService<DbContext_Fake> _contextService;
        private DatabaseFacade_Fake _databaseFacade;

        private DbContextTransactionUnitOfWork<DbContext_Fake> _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new CustomAutoFixture();

            _context = new DbContext_Fake();

            _contextService = _fixture.Freeze<IContextService<DbContext_Fake>>();
            _contextService.HasCurrentContext().Returns(false);
            _contextService.InitContext().Returns(_context);
            _contextService.GetCurrentContext().Returns(_context);

            _databaseFacade = (DatabaseFacade_Fake)_context.Database;

            _sut = _fixture.Create<DbContextTransactionUnitOfWork<DbContext_Fake>>();
        }

        [Test]
        public void When_calling_run_action_Then_transaction_is_committed_once()
        {
            var transaction = _fixture.Create<IDbContextTransaction>();
            _databaseFacade.Transaction = transaction;

            _sut.Run(() => { });

            transaction.Received(1).Commit();
        }

        [Test]
        public void When_calling_run_func_Then_transaction_is_committed_once()
        {
            var transaction = _fixture.Create<IDbContextTransaction>();
            _databaseFacade.Transaction = transaction;

            _sut.Run(() => string.Empty);

            transaction.Received(1).Commit();
        }

        [Test]
        public async Task When_calling_run_async_action_Then_transaction_is_committed_once()
        {
            var transaction = _fixture.Create<IDbContextTransaction>();
            _databaseFacade.Transaction = transaction;

            await _sut.RunAsync(() => Task.CompletedTask);

            transaction.Received(1).Commit();
        }

        [Test]
        public async Task When_calling_run_async_func_Then_transaction_is_committed_once()
        {
            var transaction = _fixture.Create<IDbContextTransaction>();
            _databaseFacade.Transaction = transaction;

            await _sut.RunAsync(() => Task.FromResult(string.Empty));

            transaction.Received(1).Commit();
        }
    }
}