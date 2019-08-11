using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
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
        public void When_calling_query_for_a_null_query_Then_an_exception_is_thrown()
        {
            Assert.That(() => _sut.Query(null), Throws.ArgumentNullException);
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
                    _dbSet.Received(1).Add(_foo);
                    Assert.That(_context.GetCallCount(nameof(DbContext.SaveChanges)), Is.EqualTo(1), "Invalid SaveChanges call count");
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
                _sut.AddAsync(_foo).Wait();

                Assert.Multiple(() =>
                {
                    _contextService.Received(_expectedContextManageCount).InitContext();
                    _contextService.Received(_expectedContextManageCount).ClearCurrentContext();
                    _dbSet.Received(1).AddAsync(_foo);
                    Assert.That(_context.GetCallCount(nameof(DbContext.SaveChangesAsync)), Is.EqualTo(1), "Invalid SaveChangesAsync call count");
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

            private Foo _foo;
            private DbContext_Fake _context;
            private DbSet<Foo> _dbSet;

            private Foo _result;

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

                _foo = _fixture.Create<Foo>();

                _dbSet = CreateDbSetSubstitute(_foo);
                _dbSet.Find(_foo.FooId).Returns(_foo);

                _dbSetService.GetDbSet(Arg.Any<DbContext_Fake>())
                    .Returns(_dbSet);

                _sut = _fixture.Create<ContextRepository<DbContext_Fake, Foo>>();
            }

            [Test]
            public void Then_expected_methods_are_called_the_correct_times()
            {
                _result = _sut.Get(_foo.FooId);

                Assert.Multiple(() =>
                {
                    _contextService.Received(_expectedContextManageCount).InitContext();
                    _contextService.Received(_expectedContextManageCount).ClearCurrentContext();
                    _dbSet.Received(1).Find(_foo.FooId);
                    Assert.That(_result, Is.EqualTo(_foo), "Invalid entity returned");
                });
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
                    Assert.That(_context.GetCallCount(nameof(DbContext.SaveChanges)), Is.EqualTo(1), "Invalid SaveChanges call count");
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
                    Assert.That(_context.GetCallCount(nameof(DbContext.SaveChanges)), Is.EqualTo(1), "Invalid SaveChanges call count");
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

            private int _result;

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
                _result = _sut.Count(_query);

                Assert.Multiple(() =>
                {
                    _contextService.Received(_expectedContextManageCount).InitContext();
                    _contextService.Received(_expectedContextManageCount).ClearCurrentContext();
                    Assert.That(_result, Is.EqualTo(_foos.Count()), "Invalid result count");
                });
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

            private IEnumerable<Foo> _result;

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

                _query = _fixture.Create<IQuery<Foo>>();
                _query.Query(Arg.Any<IQueryable<Foo>>())
                    .Returns(_foos.AsQueryable());

                _sut = _fixture.Create<ContextRepository<DbContext_Fake, Foo>>();
            }

            [Test]
            public void Then_expected_methods_are_called_the_correct_times()
            {
                _result = _sut.Query(_query);

                Assert.Multiple(() =>
                {
                    _contextService.Received(_expectedContextManageCount).InitContext();
                    _contextService.Received(_expectedContextManageCount).ClearCurrentContext();
                    _query.Received(1).GetLoadedProperties();
                    Assert.That(_result, Is.EqualTo(_foos), "Invalid result");
                });
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