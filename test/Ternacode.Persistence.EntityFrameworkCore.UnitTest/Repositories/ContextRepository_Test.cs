﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;
using NUnit.Framework;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.EntityFrameworkCore.Repositories;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;
using Ternacode.Persistence.EntityFrameworkCore.UnitTest.Fakes;
using Ternacode.Persistence.EntityFrameworkCore.UnitTest.Model;

namespace Ternacode.Persistence.EntityFrameworkCore.UnitTest.Repositories
{
    [TestFixture]
    public class ContextRepository_Test
    {
        private CustomAutoFixture _fixture;

        private ContextRepository<DbContext_Fake, Foo> _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new CustomAutoFixture();

            _sut = _fixture.Create<ContextRepository<DbContext_Fake, Foo>>();
        }

        [Test]
        public void When_adding_a_null_entity_Then_an_exception_is_thrown()
        {
            Assert.That(() => _sut.Add(null), Throws.ArgumentNullException);
        }

        [Test]
        public void When_adding_async_a_null_entity_Then_an_exception_is_thrown()
        {
            Assert.That(() => _sut.AddAsync(null), Throws.ArgumentNullException);
        }

        [Test]
        public void When_updating_a_null_entity_Then_an_exception_is_thrown()
        {
            Assert.That(() => _sut.Update(null), Throws.ArgumentNullException);
        }

        [Test]
        public void When_calling_count_for_a_null_query_Then_an_exception_is_thrown()
        {
            Assert.That(() => _sut.Count(null), Throws.ArgumentNullException);
        }

        [Test]
        public void When_calling_any_for_a_null_query_Then_an_exception_is_thrown()
        {
            Assert.That(() => _sut.Any(null), Throws.ArgumentNullException);
        }

        [Test]
        public void When_calling_query_for_a_null_query_Then_an_exception_is_thrown()
        {
            Assert.That(() => _sut.Query(null), Throws.ArgumentNullException);
        }

        [Test]
        public void When_calling_query_async_for_a_null_query_Then_an_exception_is_thrown()
        {
            Assert.That(() => _sut.QueryAsync(null), Throws.ArgumentNullException);
        }

        [TestFixture(false)]
        [TestFixture(true)]
        public class When_adding_an_entity
        {
            private CustomAutoFixture _fixture;

            private readonly bool _hasCurrentContext;
            private int _expectedContextManageCount;

            private ContextRepository<DbContext_Fake, Foo> _sut;

            private IContextService<DbContext_Fake> _contextService;
            private IDbSetService<DbContext_Fake, Foo> _dbSetService;
            private IFlushService<DbContext_Fake> _flushService;

            private Foo _foo;
            private DbContext_Fake _context;
            private DbSet<Foo> _dbSet;

            public When_adding_an_entity(bool hasCurrentContext)
            {
                _hasCurrentContext = hasCurrentContext;
            }

            [SetUp]
            public void SetUp()
            {
                _fixture = new CustomAutoFixture();

                _expectedContextManageCount = _hasCurrentContext ? 0 : 1;

                _contextService = _fixture.Freeze<IContextService<DbContext_Fake>>();
                _dbSetService = _fixture.Freeze<IDbSetService<DbContext_Fake, Foo>>();
                _flushService = _fixture.Freeze<IFlushService<DbContext_Fake>>();

                _context = new DbContext_Fake();

                _contextService.HasCurrentContext().Returns(_hasCurrentContext);
                _contextService.InitContext().Returns(_context);
                _contextService.GetCurrentContext().Returns(_context);

                _dbSet = Substitute.For<DbSet<Foo>, IQueryable<Foo>>();

                _dbSetService.GetDbSet(Arg.Any<DbContext_Fake>())
                    .Returns(_dbSet);

                _sut = _fixture.Create<ContextRepository<DbContext_Fake, Foo>>();

                _foo = _fixture.Create<Foo>();
            }

            [Test]
            public void Then_expected_methods_are_called_the_correct_times()
            {
                _sut.Add(_foo);

                Assert.Multiple(() =>
                {
                    _contextService.Received(_expectedContextManageCount).InitContext();
                    _contextService.Received(_expectedContextManageCount).ClearCurrentContext();
                    _dbSet.Received(1).AddAsync(_foo);
                    _flushService.Received(1).FlushChangesAsync(_context);
                });
            }
        }

        [TestFixture(false)]
        [TestFixture(true)]
        public class When_adding_an_entity_async
        {
            private CustomAutoFixture _fixture;

            private readonly bool _hasCurrentContext;
            private int _expectedContextManageCount;

            private ContextRepository<DbContext_Fake, Foo> _sut;

            private IContextService<DbContext_Fake> _contextService;
            private IDbSetService<DbContext_Fake, Foo> _dbSetService;
            private IFlushService<DbContext_Fake> _flushService;

            private Foo _foo;
            private DbContext_Fake _context;
            private DbSet<Foo> _dbSet;

            public When_adding_an_entity_async(bool hasCurrentContext)
            {
                _hasCurrentContext = hasCurrentContext;
            }

            [SetUp]
            public void SetUp()
            {
                _fixture = new CustomAutoFixture();

                _expectedContextManageCount = _hasCurrentContext ? 0 : 1;

                _contextService = _fixture.Freeze<IContextService<DbContext_Fake>>();
                _dbSetService = _fixture.Freeze<IDbSetService<DbContext_Fake, Foo>>();
                _flushService = _fixture.Freeze<IFlushService<DbContext_Fake>>();

                _context = new DbContext_Fake();

                _contextService.HasCurrentContext().Returns(_hasCurrentContext);
                _contextService.InitContext().Returns(_context);
                _contextService.GetCurrentContext().Returns(_context);

                _dbSet = Substitute.For<DbSet<Foo>, IQueryable<Foo>>();

                _dbSetService.GetDbSet(Arg.Any<DbContext_Fake>())
                    .Returns(_dbSet);

                _sut = _fixture.Create<ContextRepository<DbContext_Fake, Foo>>();

                _foo = _fixture.Create<Foo>();
            }

            [Test]
            public async Task Then_expected_methods_are_called_the_correct_times()
            {
                await _sut.AddAsync(_foo);

                Assert.Multiple(() =>
                {
                    _contextService.Received(_expectedContextManageCount).InitContext();
                    _contextService.Received(_expectedContextManageCount).ClearCurrentContext();
                    _dbSet.Received(1).AddAsync(_foo);
                    _flushService.Received(1).FlushChangesAsync(_context);
                });
            }
        }

        [TestFixture(false)]
        [TestFixture(true)]
        public class When_getting_an_entity
        {
            private CustomAutoFixture _fixture;

            private readonly bool _hasCurrentContext;
            private int _expectedContextManageCount;

            private ContextRepository<DbContext_Fake, Foo> _sut;

            private IContextService<DbContext_Fake> _contextService;
            private IDbSetService<DbContext_Fake, Foo> _dbSetService;

            private Foo _expectedEntity;
            private DbContext_Fake _context;
            private DbSet<Foo> _dbSet;

            public When_getting_an_entity(bool hasCurrentContext)
            {
                _hasCurrentContext = hasCurrentContext;
            }

            [SetUp]
            public void SetUp()
            {
                _fixture = new CustomAutoFixture();

                _expectedContextManageCount = _hasCurrentContext ? 0 : 1;

                _contextService = _fixture.Freeze<IContextService<DbContext_Fake>>();
                _dbSetService = _fixture.Freeze<IDbSetService<DbContext_Fake, Foo>>();

                _context = new DbContext_Fake();

                _contextService.HasCurrentContext().Returns(_hasCurrentContext);
                _contextService.InitContext().Returns(_context);
                _contextService.GetCurrentContext().Returns(_context);

                _expectedEntity = _fixture.Create<Foo>();

                _dbSet = CreateDbSetSubstitute(_expectedEntity);
                _dbSet.FindAsync(_expectedEntity.FooId).Returns(_expectedEntity);

                _dbSetService.GetDbSet(Arg.Any<DbContext_Fake>())
                    .Returns(_dbSet);

                _sut = _fixture.Create<ContextRepository<DbContext_Fake, Foo>>();
            }

            [Test]
            public void Then_expected_methods_are_called_the_correct_times()
            {
                _sut.Get(_expectedEntity.FooId);

                Assert.Multiple(() =>
                {
                    _contextService.Received(_expectedContextManageCount).InitContext();
                    _contextService.Received(_expectedContextManageCount).ClearCurrentContext();
                    _dbSet.Received(1).FindAsync(_expectedEntity.FooId);
                });
            }

            [Test]
            public void Then_the_expected_entity_is_returned()
            {
                var result = _sut.Get(_expectedEntity.FooId);

                Assert.That(result, Is.EqualTo(_expectedEntity), "Invalid entity");
            }
        }

        [TestFixture(false)]
        [TestFixture(true)]
        public class When_getting_an_entity_async
        {
            private CustomAutoFixture _fixture;

            private readonly bool _hasCurrentContext;
            private int _expectedContextManageCount;

            private ContextRepository<DbContext_Fake, Foo> _sut;

            private IContextService<DbContext_Fake> _contextService;
            private IDbSetService<DbContext_Fake, Foo> _dbSetService;

            private Foo _expectedEntity;
            private DbContext_Fake _context;
            private DbSet<Foo> _dbSet;

            public When_getting_an_entity_async(bool hasCurrentContext)
            {
                _hasCurrentContext = hasCurrentContext;
            }

            [SetUp]
            public void SetUp()
            {
                _fixture = new CustomAutoFixture();

                _expectedContextManageCount = _hasCurrentContext ? 0 : 1;

                _contextService = _fixture.Freeze<IContextService<DbContext_Fake>>();
                _dbSetService = _fixture.Freeze<IDbSetService<DbContext_Fake, Foo>>();

                _context = new DbContext_Fake();

                _contextService.HasCurrentContext().Returns(_hasCurrentContext);
                _contextService.InitContext().Returns(_context);
                _contextService.GetCurrentContext().Returns(_context);

                _expectedEntity = _fixture.Create<Foo>();

                _dbSet = CreateDbSetSubstitute(_expectedEntity);
                _dbSet.FindAsync(_expectedEntity.FooId).Returns(_expectedEntity);

                _dbSetService.GetDbSet(Arg.Any<DbContext_Fake>())
                    .Returns(_dbSet);

                _sut = _fixture.Create<ContextRepository<DbContext_Fake, Foo>>();
            }

            [Test]
            public async Task Then_expected_methods_are_called_the_correct_times()
            {
                await _sut.GetAsync(_expectedEntity.FooId);

                Assert.Multiple(() =>
                {
                    _contextService.Received(_expectedContextManageCount).InitContext();
                    _contextService.Received(_expectedContextManageCount).ClearCurrentContext();
                    _dbSet.Received(1).FindAsync(_expectedEntity.FooId);
                });
            }

            [Test]
            public async Task Then_the_expected_entity_is_returned()
            {
                var result = await _sut.GetAsync(_expectedEntity.FooId);

                Assert.That(result, Is.EqualTo(_expectedEntity), "Invalid entity");
            }
        }

        [TestFixture(false)]
        [TestFixture(true)]
        public class When_updating_an_entity
        {
            private CustomAutoFixture _fixture;

            private readonly bool _hasCurrentContext;
            private int _expectedContextManageCount;

            private ContextRepository<DbContext_Fake, Foo> _sut;

            private IContextService<DbContext_Fake> _contextService;
            private IDbSetService<DbContext_Fake, Foo> _dbSetService;
            private IFlushService<DbContext_Fake> _flushService;

            private Foo _foo;
            private DbContext_Fake _context;
            private DbSet<Foo> _dbSet;

            public When_updating_an_entity(bool hasCurrentContext)
            {
                _hasCurrentContext = hasCurrentContext;
            }

            [SetUp]
            public void SetUp()
            {
                _fixture = new CustomAutoFixture();

                _expectedContextManageCount = _hasCurrentContext ? 0 : 1;

                _contextService = _fixture.Freeze<IContextService<DbContext_Fake>>();
                _dbSetService = _fixture.Freeze<IDbSetService<DbContext_Fake, Foo>>();
                _flushService = _fixture.Freeze<IFlushService<DbContext_Fake>>();

                _context = new DbContext_Fake();

                _contextService.HasCurrentContext().Returns(_hasCurrentContext);
                _contextService.InitContext().Returns(_context);
                _contextService.GetCurrentContext().Returns(_context);

                _dbSet = Substitute.For<DbSet<Foo>, IQueryable<Foo>>();

                _dbSetService.GetDbSet(Arg.Any<DbContext_Fake>())
                    .Returns(_dbSet);

                _sut = _fixture.Create<ContextRepository<DbContext_Fake, Foo>>();

                _foo = _fixture.Create<Foo>();
            }

            [Test]
            public void Then_expected_methods_are_called_the_correct_times()
            {
                _sut.Update(_foo);

                Assert.Multiple(() =>
                {
                    _contextService.Received(_expectedContextManageCount).InitContext();
                    _contextService.Received(_expectedContextManageCount).ClearCurrentContext();
                    _dbSet.Received(1).Update(_foo);
                    _flushService.Received(1).FlushChanges(_context);
                });
            }
        }

        [TestFixture(false)]
        [TestFixture(true)]
        public class When_deleting_an_entity
        {
            private CustomAutoFixture _fixture;

            private readonly bool _hasCurrentContext;
            private int _expectedContextManageCount;

            private ContextRepository<DbContext_Fake, Foo> _sut;

            private IContextService<DbContext_Fake> _contextService;
            private IDbSetService<DbContext_Fake, Foo> _dbSetService;
            private IFlushService<DbContext_Fake> _flushService;

            private Foo _foo;
            private DbContext_Fake _context;
            private DbSet<Foo> _dbSet;

            public When_deleting_an_entity(bool hasCurrentContext)
            {
                _hasCurrentContext = hasCurrentContext;
            }

            [SetUp]
            public void SetUp()
            {
                _fixture = new CustomAutoFixture();

                _expectedContextManageCount = _hasCurrentContext ? 0 : 1;

                _contextService = _fixture.Freeze<IContextService<DbContext_Fake>>();
                _dbSetService = _fixture.Freeze<IDbSetService<DbContext_Fake, Foo>>();
                _flushService = _fixture.Freeze<IFlushService<DbContext_Fake>>();

                _context = new DbContext_Fake();

                _contextService.HasCurrentContext().Returns(_hasCurrentContext);
                _contextService.InitContext().Returns(_context);
                _contextService.GetCurrentContext().Returns(_context);

                _dbSet = Substitute.For<DbSet<Foo>, IQueryable<Foo>>();

                _dbSetService.GetDbSet(Arg.Any<DbContext_Fake>())
                    .Returns(_dbSet);

                _sut = _fixture.Create<ContextRepository<DbContext_Fake, Foo>>();

                _foo = _fixture.Create<Foo>();
            }

            [Test]
            public void Then_expected_methods_are_called_the_correct_times()
            {
                _sut.Delete(_foo);

                Assert.Multiple(() =>
                {
                    _contextService.Received(_expectedContextManageCount).InitContext();
                    _contextService.Received(_expectedContextManageCount).ClearCurrentContext();
                    _dbSet.Received(1).Remove(_foo);
                    _flushService.Received(1).FlushChanges(_context);
                });
            }
        }

        [TestFixture(false)]
        [TestFixture(true)]
        public class When_counting_entities
        {
            private CustomAutoFixture _fixture;

            private readonly bool _hasCurrentContext;
            private int _expectedContextManageCount;

            private ContextRepository<DbContext_Fake, Foo> _sut;

            private IContextService<DbContext_Fake> _contextService;
            private IDbSetService<DbContext_Fake, Foo> _dbSetService;

            private DbContext_Fake _context;
            private DbSet<Foo> _dbSet;
            private IEnumerable<Foo> _foos;
            private IQuery<Foo> _query;

            public When_counting_entities(bool hasCurrentContext)
            {
                _hasCurrentContext = hasCurrentContext;
            }

            [SetUp]
            public void SetUp()
            {
                _fixture = new CustomAutoFixture();

                _expectedContextManageCount = _hasCurrentContext ? 0 : 1;

                _contextService = _fixture.Freeze<IContextService<DbContext_Fake>>();
                _dbSetService = _fixture.Freeze<IDbSetService<DbContext_Fake, Foo>>();

                _context = new DbContext_Fake();

                _contextService.HasCurrentContext().Returns(_hasCurrentContext);
                _contextService.InitContext().Returns(_context);
                _contextService.GetCurrentContext().Returns(_context);

                _dbSet = Substitute.For<DbSet<Foo>, IQueryable<Foo>>();

                _dbSetService.GetDbSet(Arg.Any<DbContext_Fake>())
                    .Returns(_dbSet);

                _foos = _fixture.CreateMany<Foo>();

                _query = _fixture.Create<IQuery<Foo>>();
                _query.Query(Arg.Any<IQueryable<Foo>>())
                    .Returns(_foos.AsQueryable());

                _sut = _fixture.Create<ContextRepository<DbContext_Fake, Foo>>();
            }

            [Test]
            public void Then_expected_methods_are_called_the_correct_times()
            {
                _sut.Count(_query);

                Assert.Multiple(() =>
                {
                    _contextService.Received(_expectedContextManageCount).InitContext();
                    _contextService.Received(_expectedContextManageCount).ClearCurrentContext();
                });
            }

            [Test]
            public void Then_the_expected_count_is_returned()
            {
                var result = _sut.Count(_query);

                Assert.That(result, Is.EqualTo(_foos.Count()), "Invalid result count");
            }
        }

        [TestFixture(false)]
        [TestFixture(true)]
        public class When_checking_for_any_entities
        {
            private CustomAutoFixture _fixture;

            private readonly bool _hasCurrentContext;
            private int _expectedContextManageCount;

            private ContextRepository<DbContext_Fake, Foo> _sut;

            private IContextService<DbContext_Fake> _contextService;
            private IDbSetService<DbContext_Fake, Foo> _dbSetService;

            private DbContext_Fake _context;
            private DbSet<Foo> _dbSet;
            private IEnumerable<Foo> _foos;
            private IQuery<Foo> _query;

            public When_checking_for_any_entities(bool hasCurrentContext)
            {
                _hasCurrentContext = hasCurrentContext;
            }

            [SetUp]
            public void SetUp()
            {
                _fixture = new CustomAutoFixture();

                _expectedContextManageCount = _hasCurrentContext ? 0 : 1;

                _contextService = _fixture.Freeze<IContextService<DbContext_Fake>>();
                _dbSetService = _fixture.Freeze<IDbSetService<DbContext_Fake, Foo>>();

                _context = new DbContext_Fake();

                _contextService.HasCurrentContext().Returns(_hasCurrentContext);
                _contextService.InitContext().Returns(_context);
                _contextService.GetCurrentContext().Returns(_context);

                _dbSet = Substitute.For<DbSet<Foo>, IQueryable<Foo>>();

                _dbSetService.GetDbSet(Arg.Any<DbContext_Fake>())
                    .Returns(_dbSet);

                _foos = _fixture.CreateMany<Foo>();

                _query = _fixture.Create<IQuery<Foo>>();
                _query.Query(Arg.Any<IQueryable<Foo>>())
                    .Returns(_foos.AsQueryable());

                _sut = _fixture.Create<ContextRepository<DbContext_Fake, Foo>>();
            }

            [Test]
            public void Then_expected_methods_are_called_the_correct_times()
            {
                _sut.Any(_query);

                Assert.Multiple(() =>
                {
                    _contextService.Received(_expectedContextManageCount).InitContext();
                    _contextService.Received(_expectedContextManageCount).ClearCurrentContext();
                });
            }

            [Test]
            public void Then_the_expected_result_is_returned()
            {
                var result = _sut.Any(_query);

                Assert.That(result, Is.EqualTo(true));
            }
        }

        [TestFixture(false)]
        [TestFixture(true)]
        public class When_querying_entities
        {
            private CustomAutoFixture _fixture;

            private readonly bool _hasCurrentContext;
            private int _expectedContextManageCount;

            private ContextRepository<DbContext_Fake, Foo> _sut;

            private IContextService<DbContext_Fake> _contextService;
            private IDbSetService<DbContext_Fake, Foo> _dbSetService;

            private DbContext_Fake _context;
            private DbSet<Foo> _dbSet;
            private IEnumerable<Foo> _foos;
            private IQuery<Foo> _query;

            public When_querying_entities(bool hasCurrentContext)
            {
                _hasCurrentContext = hasCurrentContext;
            }

            [SetUp]
            public void SetUp()
            {
                _fixture = new CustomAutoFixture();

                _expectedContextManageCount = _hasCurrentContext ? 0 : 1;

                _contextService = _fixture.Freeze<IContextService<DbContext_Fake>>();
                _dbSetService = _fixture.Freeze<IDbSetService<DbContext_Fake, Foo>>();

                _context = new DbContext_Fake();

                _contextService.HasCurrentContext().Returns(_hasCurrentContext);
                _contextService.InitContext().Returns(_context);
                _contextService.GetCurrentContext().Returns(_context);

                _dbSet = Substitute.For<DbSet<Foo>, IQueryable<Foo>>();

                _dbSetService.GetDbSet(Arg.Any<DbContext_Fake>())
                    .Returns(_dbSet);

                _foos = _fixture.CreateMany<Foo>();
                var asyncQueryableFoos = _foos.AsQueryable().BuildMock();

                _query = _fixture.Create<IQuery<Foo>>();
                _query.Query(Arg.Any<IQueryable<Foo>>())
                    .Returns(asyncQueryableFoos);

                _sut = _fixture.Create<ContextRepository<DbContext_Fake, Foo>>();
            }

            [Test]
            public void Then_expected_methods_are_called_the_correct_times()
            {
                _sut.Query(_query);

                Assert.Multiple(() =>
                {
                    _contextService.Received(_expectedContextManageCount).InitContext();
                    _contextService.Received(_expectedContextManageCount).ClearCurrentContext();
                    _query.Received(1).GetLoadedProperties();
                });
            }

            [Test]
            public void Then_the_expected_result_is_returned()
            {
                var result = _sut.Query(_query);

                Assert.That(result, Is.EqualTo(_foos), "Invalid result");
            }
        }

        [TestFixture(false)]
        [TestFixture(true)]
        public class When_querying_entities_async
        {
            private CustomAutoFixture _fixture;

            private readonly bool _hasCurrentContext;
            private int _expectedContextManageCount;

            private ContextRepository<DbContext_Fake, Foo> _sut;

            private IContextService<DbContext_Fake> _contextService;
            private IDbSetService<DbContext_Fake, Foo> _dbSetService;

            private DbContext_Fake _context;
            private DbSet<Foo> _dbSet;
            private IEnumerable<Foo> _foos;
            private IQuery<Foo> _query;

            public When_querying_entities_async(bool hasCurrentContext)
            {
                _hasCurrentContext = hasCurrentContext;
            }

            [SetUp]
            public void SetUp()
            {
                _fixture = new CustomAutoFixture();

                _expectedContextManageCount = _hasCurrentContext ? 0 : 1;

                _contextService = _fixture.Freeze<IContextService<DbContext_Fake>>();
                _dbSetService = _fixture.Freeze<IDbSetService<DbContext_Fake, Foo>>();

                _context = new DbContext_Fake();

                _contextService.HasCurrentContext().Returns(_hasCurrentContext);
                _contextService.InitContext().Returns(_context);
                _contextService.GetCurrentContext().Returns(_context);

                _dbSet = Substitute.For<DbSet<Foo>, IQueryable<Foo>>();

                _dbSetService.GetDbSet(Arg.Any<DbContext_Fake>())
                    .Returns(_dbSet);

                _foos = _fixture.CreateMany<Foo>();
                var asyncQueryableFoos = _foos.AsQueryable().BuildMock();

                _query = _fixture.Create<IQuery<Foo>>();
                _query.Query(Arg.Any<IQueryable<Foo>>())
                    .Returns(asyncQueryableFoos);

                _sut = _fixture.Create<ContextRepository<DbContext_Fake, Foo>>();
            }

            [Test]
            public async Task Then_expected_methods_are_called_the_correct_times()
            {
                await _sut.QueryAsync(_query);

                Assert.Multiple(() =>
                {
                    _contextService.Received(_expectedContextManageCount).InitContext();
                    _contextService.Received(_expectedContextManageCount).ClearCurrentContext();
                    _query.Received(1).GetLoadedProperties();
                });
            }

            [Test]
            public async Task Then_the_expected_result_is_returned()
            {
                var result = await _sut.QueryAsync(_query);

                Assert.That(result, Is.EqualTo(_foos), "Invalid result");
            }
        }

        private static DbSet<T> CreateDbSetSubstitute<T>(params T[] items) where T : class
        {
            var queryable = items.AsQueryable();
            var dbSet = Substitute.For<DbSet<T>, IQueryable<T>>();

            ((IQueryable<T>)dbSet).Provider.Returns(queryable.Provider);
            ((IQueryable<T>)dbSet).Expression.Returns(queryable.Expression);
            ((IQueryable<T>)dbSet).ElementType.Returns(queryable.ElementType);
            ((IQueryable<T>)dbSet).GetEnumerator().Returns(queryable.GetEnumerator());

            return dbSet;
        }
    }
}