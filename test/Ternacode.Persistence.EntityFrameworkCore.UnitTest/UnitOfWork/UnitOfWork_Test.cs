using System;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.EntityFrameworkCore.Enums;
using Ternacode.Persistence.EntityFrameworkCore.Exceptions;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;
using Ternacode.Persistence.EntityFrameworkCore.UnitOfWork;
using Ternacode.Persistence.EntityFrameworkCore.UnitTest.Fakes;

namespace Ternacode.Persistence.EntityFrameworkCore.UnitTest.UnitOfWork
{
    [TestFixture(TransactionType.DbContextTransaction)]
    [TestFixture(TransactionType.TransactionScope)]
    public class UnitOfWork_Test
    {
        private readonly TransactionType _transactionType;

        private CustomAutoFixture _fixture;

        private IUnitOfWork _sut;

        private IContextService<DbContext_Fake> _contextService;

        private IContextFlushService<DbContext_Fake> _contextFlushService;

        private DbContext_Fake _context;

        public UnitOfWork_Test(TransactionType transactionType)
        {
            _transactionType = transactionType;
        }

        [SetUp]
        public void SetUp()
        {
            _fixture = new CustomAutoFixture();

            _context = new DbContext_Fake();

            _contextService = _fixture.Freeze<IContextService<DbContext_Fake>>();
            _contextService.HasCurrentContext().Returns(false);
            _contextService.InitContext().Returns(_context);
            _contextService.GetCurrentContext().Returns(_context);

            _contextFlushService = _fixture.Freeze<IContextFlushService<DbContext_Fake>>();

            _sut = CreateSut(_transactionType);
        }

        [Test]
        public void When_calling_run_action_with_existing_context_Then_an_exception_is_thrown()
        {
            _contextService.HasCurrentContext().Returns(true);

            Assert.That(() => _sut.Run(() => { }), Throws.InstanceOf<CurrentContextAlreadySetException>());
        }

        [Test]
        public void When_calling_run_func_with_existing_context_Then_an_exception_is_thrown()
        {
            _contextService.HasCurrentContext().Returns(true);

            Assert.That(() => _sut.Run(() => string.Empty), Throws.InstanceOf<CurrentContextAlreadySetException>());
        }

        [Test]
        public void When_calling_run_async_action_with_existing_context_Then_an_exception_is_thrown()
        {
            _contextService.HasCurrentContext().Returns(true);

            Assert.That(async () => await _sut.RunAsync(() => Task.CompletedTask), Throws.InstanceOf<CurrentContextAlreadySetException>());
        }

        [Test]
        public void When_calling_run_async_func_with_existing_context_Then_an_exception_is_thrown()
        {
            _contextService.HasCurrentContext().Returns(true);

            Assert.That(async () => await _sut.RunAsync(() => Task.FromResult(string.Empty)), Throws.InstanceOf<CurrentContextAlreadySetException>());
        }

        [Test]
        public void When_calling_run_action_Then_expected_service_methods_are_called_once_each()
        {
            _sut.Run(() => { });

            _contextService.Received(1).InitContext();
            _contextService.Received(1).ClearCurrentContext();
            _contextFlushService.Received(1).FlushChanges(Arg.Any<DbContext_Fake>());
            _contextFlushService.Received().FlushChanges(_context);
        }

        [Test]
        public void When_calling_run_func_Then_expected_service_methods_are_called_once_each()
        {
            _sut.Run(() => string.Empty);

            _contextService.Received(1).InitContext();
            _contextService.Received(1).ClearCurrentContext();
            _contextFlushService.Received(1).FlushChanges(Arg.Any<DbContext_Fake>());
            _contextFlushService.Received().FlushChanges(_context);
        }

        [Test]
        public async Task When_calling_run_async_action_Then_expected_service_methods_are_called_once_each()
        {
            await _sut.RunAsync(() => Task.CompletedTask);

            _contextService.Received(1).InitContext();
            _contextService.Received(1).ClearCurrentContext();
            await _contextFlushService.Received(1).FlushChangesAsync(Arg.Any<DbContext_Fake>());
            await _contextFlushService.Received().FlushChangesAsync(_context);
        }

        [Test]
        public async Task When_calling_run_async_func_Then_expected_service_methods_are_called_once_each()
        {
            await _sut.RunAsync(() => Task.FromResult(string.Empty));

            _contextService.Received(1).InitContext();
            _contextService.Received(1).ClearCurrentContext();
            await _contextFlushService.Received(1).FlushChangesAsync(Arg.Any<DbContext_Fake>());
            await _contextFlushService.Received().FlushChangesAsync(_context);
        }

        [Test]
        public void When_calling_run_func_Then_the_expected_result_is_returned()
        {
            var expectedResult = _fixture.Create<string>();
            var result = _sut.Run(() => expectedResult);

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void When_calling_run_func_returning_Task_Then_the_task_is_waited_on_before_returning()
        {
            var result = _sut.Run(() => Task.Delay(500));

            Assert.That(result.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }

        [Test]
        public async Task When_calling_run_async_func_Then_the_expected_result_is_returned()
        {
            var expectedResult = _fixture.Create<string>();
            var result = await _sut.RunAsync(() => Task.FromResult(expectedResult));

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private IUnitOfWork CreateSut(TransactionType transactionType)
        {
            switch (transactionType)
            {
                case TransactionType.DbContextTransaction: return _fixture.Create<DbContextTransactionUnitOfWork<DbContext_Fake>>();
                case TransactionType.TransactionScope: return _fixture.Create<TransactionScopeUnitOfWork<DbContext_Fake>>();
                default: throw new ArgumentException();
            }
        }
    }
}