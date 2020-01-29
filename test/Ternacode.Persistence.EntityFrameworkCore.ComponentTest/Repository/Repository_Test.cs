using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Contexts;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Enums;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Factories;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Model;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Queries;
using Ternacode.Persistence.EntityFrameworkCore.Configuration;

// ReSharper disable PossibleMultipleEnumeration

namespace Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Repository
{
    [TestFixtureSource(nameof(TestConfigurations))]
    public class Repository_Test : BaseComponentTest
    {
        public static IEnumerable<object> TestConfigurations = new[]
        {
            new object[] { DbType.InMemory, false },
            new object[] { DbType.InMemory, true },
            new object[] { DbType.MSSQL, false },
            new object[] { DbType.MSSQL, true }
        };

        private readonly DbType _dbType;
        private readonly bool _useContextPool;

        private IRepository<Foo> _fooRepository;
        private IRepository<Bar> _barRepository;

        public Repository_Test(DbType dbType, bool useContextPool)
        {
            _dbType = dbType;
            _useContextPool = useContextPool;
        }

        protected override PersistenceOptions GetPersistenceOptions()
            => new PersistenceOptions
            {
                UseContextPool = _useContextPool
            };

        protected override ComponentTestContext CreateContext()
            => ContextFactory.CreateContext(_dbType, nameof(Repository_Test));

        [SetUp]
        public new void SetUp()
        {
            _fooRepository = _serviceProvider.GetService<IRepository<Foo>>();
            _barRepository = _serviceProvider.GetService<IRepository<Bar>>();
        }

        [Test]
        public void When_adding_an_entity_Then_the_entity_ids_are_updated()
        {
            var foo = FooFactory.Create();

            _fooRepository.Add(foo);

            Assert.Multiple(() =>
            {
                Assert.That(foo.FooId, Is.Not.EqualTo(default(long)), "Invalid FooId");
                Assert.That(foo.Bar.BarId, Is.Not.EqualTo(default(Guid)), "Invalid BarId");
            });
        }

        [Test]
        public async Task When_adding_an_entity_async_Then_the_entity_ids_are_updated()
        {
            var foo = FooFactory.Create();

            await _fooRepository.AddAsync(foo);

            Assert.That(foo.FooId, Is.Not.EqualTo(default(long)));
            Assert.That(foo.Bar.BarId, Is.Not.EqualTo(default(Guid)));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(100)]
        public async Task When_adding_entities_async_in_sequence_Then_the_repository_count_is_correct(int entityCount)
        {
            for (var i = 0; i < entityCount; i++)
            {
                await _fooRepository.AddAsync(FooFactory.Create());
            }

            Assert.Multiple(() =>
            {
                Assert.That(_fooRepository.Count(new GetAllFoosQuery()), Is.EqualTo(entityCount), "Invalid Foo count");
                Assert.That(_barRepository.Count(new GetWithoutLoadingQuery<Bar>()), Is.EqualTo(entityCount), "Invalid Bar count");
            });
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(100)]
        public void When_adding_entities_in_parallel_Then_the_repository_count_is_correct(int entityCount)
        {
            Parallel.ForEach(Enumerable.Range(0, entityCount),
                 _ =>
                 {
                     // Repository instances are not thread safe
                     var repo = _serviceProvider.GetService<IRepository<Foo>>();
                     repo.Add(FooFactory.Create());
                 });

            Assert.Multiple(() =>
            {
                Assert.That(_fooRepository.Count(new GetAllFoosQuery()), Is.EqualTo(entityCount), "Invalid Foo count");
                Assert.That(_barRepository.Count(new GetWithoutLoadingQuery<Bar>()), Is.EqualTo(entityCount), "Invalid Bar count");
            });
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(100)]
        public void When_adding_entities_async_in_parallel_Then_the_repository_count_is_correct(int entityCount)
        {
            var tasks = Enumerable.Range(0, entityCount)
                .AsParallel()
                .Select(async _ =>
                {
                    var repo = _serviceProvider.GetService<IRepository<Foo>>();
                    await repo.AddAsync(FooFactory.Create());
                })
                .ToArray();

            Task.WaitAll(tasks);

            Assert.Multiple(() =>
            {
                Assert.That(_fooRepository.Count(new GetAllFoosQuery()), Is.EqualTo(entityCount), "Invalid Foo count");
                Assert.That(_barRepository.Count(new GetWithoutLoadingQuery<Bar>()), Is.EqualTo(entityCount), "Invalid Bar count");
            });
        }

        [Test]
        public async Task When_querying_for_any_entity_Then_the_expected_result_is_returned()
        {
            const string bar1Name = "bar 1";
            const string bar2Name = "bar 2";

            var foo1 = FooFactory.Create("foo 1", bar1Name);
            var foo2 = FooFactory.Create("foo 2", bar2Name);

            await _fooRepository.AddAsync(foo1);
            _fooRepository.Add(foo2);

            var bars = _barRepository.Query(new GetAllBarsQuery());
            var bar1 = bars.Single(b => b.Name == bar1Name);
            var bar2 = bars.Single(b => b.Name == bar2Name);

            var anyAllFoos = _fooRepository.Any(new GetAllFoosQuery());
            var anyAllBars = _barRepository.Any(new GetAllBarsQuery());
            
            var anyWithFoo1Id = _fooRepository.Any(new GetFooWithIdQuery(foo1.FooId));
            var anyWithFoo2Id = _fooRepository.Any(new GetFooWithIdQuery(foo2.FooId));
            var anyWithInvalidFooId = _fooRepository.Any(new GetFooWithIdQuery(long.MaxValue));

            var anyWithBar1Id = _barRepository.Any(new GetBarWithIdQuery(bar1.BarId));
            var anyWithBar2Id = _barRepository.Any(new GetBarWithIdQuery(bar2.BarId));
            var anyWithInvalidBarId = _barRepository.Any(new GetBarWithIdQuery(Guid.Empty));

            Assert.Multiple(() =>
            {
                Assert.That(anyAllFoos);
                Assert.That(anyAllBars);

                Assert.That(anyWithFoo1Id);
                Assert.That(anyWithFoo2Id);
                Assert.That(anyWithInvalidFooId, Is.False);

                Assert.That(anyWithBar1Id);
                Assert.That(anyWithBar2Id);
                Assert.That(anyWithInvalidBarId, Is.False);
            });
        }

        [Test]
        public async Task When_querying_for_any_entity_async_Then_the_expected_result_is_returned()
        {
            const string bar1Name = "bar 1";
            const string bar2Name = "bar 2";

            var foo1 = FooFactory.Create("foo 1", bar1Name);
            var foo2 = FooFactory.Create("foo 2", bar2Name);

            await _fooRepository.AddAsync(foo1);
            _fooRepository.Add(foo2);

            var bars = await _barRepository.QueryAsync(new GetAllBarsQuery());
            var bar1 = bars.Single(b => b.Name == bar1Name);
            var bar2 = bars.Single(b => b.Name == bar2Name);

            var anyAllFoos = _fooRepository.Any(new GetAllFoosQuery());
            var anyAllBars = _barRepository.Any(new GetAllBarsQuery());

            var anyWithFoo1Id = _fooRepository.Any(new GetFooWithIdQuery(foo1.FooId));
            var anyWithFoo2Id = _fooRepository.Any(new GetFooWithIdQuery(foo2.FooId));
            var anyWithInvalidFooId = _fooRepository.Any(new GetFooWithIdQuery(long.MaxValue));

            var anyWithBar1Id = _barRepository.Any(new GetBarWithIdQuery(bar1.BarId));
            var anyWithBar2Id = _barRepository.Any(new GetBarWithIdQuery(bar2.BarId));
            var anyWithInvalidBarId = _barRepository.Any(new GetBarWithIdQuery(Guid.Empty));

            Assert.Multiple(() =>
            {
                Assert.That(anyAllFoos);
                Assert.That(anyAllBars);

                Assert.That(anyWithFoo1Id);
                Assert.That(anyWithFoo2Id);
                Assert.That(anyWithInvalidFooId, Is.False);

                Assert.That(anyWithBar1Id);
                Assert.That(anyWithBar2Id);
                Assert.That(anyWithInvalidBarId, Is.False);
            });
        }

        [Test]
        public async Task When_adding_entities_with_the_same_reference_entity_Then_it_gets_added_correctly()
        {
            var foo1 = FooFactory.Create(fooName: "foo 1", barName: "bar shared");
            await _fooRepository.AddAsync(foo1);

            var foo2 = new Foo
            {
                Name = "foo 2",
                Bar = foo1.Bar
            };

            _fooRepository.Update(foo2);

            var dbFoo1 = _fooRepository.Query(new GetFooWithIdQuery(foo1.FooId)).Single();
            var dbFoo2 = _fooRepository.Query(new GetFooWithIdQuery(foo2.FooId)).Single();
            var bars = _barRepository.Query(new GetWithoutLoadingQuery<Bar>());

            Assert.Multiple(() =>
            {
                Assert.That(dbFoo1.Bar.BarId, Is.EqualTo(dbFoo2.Bar.BarId), "Inequal BarIds");
                Assert.That(bars.FirstOrDefault()?.BarId, Is.EqualTo(dbFoo1.Bar.BarId), "Invalid BarId");
                Assert.That(bars.Count(), Is.EqualTo(1), "Invalid Bar count");
            });
        }

        [Test]
        public async Task When_getting_entities_by_id_Then_the_correct_items_are_returned()
        {
            var foo1 = FooFactory.Create("foo 1", "bar 1");
            var foo2 = FooFactory.Create("foo 2", "bar 2");

            await _fooRepository.AddAsync(foo1);
            await _fooRepository.AddAsync(foo2);

            var getFoo1 = _fooRepository.Get(foo1.FooId);
            var getBar1 = _barRepository.Get(foo1.Bar.BarId);
            var getFoo2 = _fooRepository.Get(foo2.FooId);
            var getBar2 = _barRepository.Get(foo2.Bar.BarId);

            Assert.Multiple(() =>
            {
                Assert.That(getFoo1?.Name, Is.EqualTo(foo1.Name), "Invalid Foo 1 name");
                Assert.That(getBar1?.Name, Is.EqualTo(foo1.Bar.Name), "Invalid Bar 1 name");
                Assert.That(getFoo2?.Name, Is.EqualTo(foo2.Name), "Invalid Foo 2 name");
                Assert.That(getBar2?.Name, Is.EqualTo(foo2.Bar.Name), "Invalid Bar 2 name");
            });
        }

        [Test]
        public async Task When_getting_entities_by_id_async_Then_the_correct_items_are_returned()
        {
            var foo1 = FooFactory.Create("foo 1", "bar 1");
            var foo2 = FooFactory.Create("foo 2", "bar 2");

            await _fooRepository.AddAsync(foo1);
            await _fooRepository.AddAsync(foo2);

            var getFoo1 = await _fooRepository.GetAsync(foo1.FooId);
            var getBar1 = await _barRepository.GetAsync(foo1.Bar.BarId);
            var getFoo2 = await _fooRepository.GetAsync(foo2.FooId);
            var getBar2 = await _barRepository.GetAsync(foo2.Bar.BarId);

            Assert.Multiple(() =>
            {
                Assert.That(getFoo1?.Name, Is.EqualTo(foo1.Name), "Invalid Foo 1 name");
                Assert.That(getBar1?.Name, Is.EqualTo(foo1.Bar.Name), "Invalid Bar 1 name");
                Assert.That(getFoo2?.Name, Is.EqualTo(foo2.Name), "Invalid Foo 2 name");
                Assert.That(getBar2?.Name, Is.EqualTo(foo2.Bar.Name), "Invalid Bar 2 name");
            });
        }

        [Test]
        public async Task When_querying_with_loading_Then_the_correct_items_are_returned()
        {
            var foo = FooFactory.Create();

            await _fooRepository.AddAsync(foo);

            var queryFoo = _fooRepository.Query(new GetAllFoosQuery()).FirstOrDefault();
            var queryBar = _barRepository.Query(new GetWithoutLoadingQuery<Bar>()).FirstOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(queryFoo?.Name, Is.EqualTo(foo.Name), "Invalid Foo query name");
                Assert.That(queryBar?.Name, Is.EqualTo(foo.Bar.Name), "Invalid Bar query name");
                Assert.That(queryFoo?.FooId, Is.EqualTo(foo.FooId), "Invalid Foo query FooId");
                Assert.That(queryBar?.BarId, Is.EqualTo(foo.Bar.BarId), "Invalid Bar query BarId");
                Assert.That(queryFoo?.Bar?.BarId, Is.EqualTo(foo.Bar.BarId), "Invalid Foo query BarId");
                Assert.That(queryFoo?.Bar?.Name, Is.EqualTo(foo.Bar.Name), "Invalid Foo query Bar name");
            });
        }

        [Test]
        public async Task When_querying_async_with_loading_Then_the_correct_items_are_returned()
        {
            var foo = FooFactory.Create();

            await _fooRepository.AddAsync(foo);

            var queryFoo = (await _fooRepository.QueryAsync(new GetAllFoosQuery())).FirstOrDefault();
            var queryBar = (await _barRepository.QueryAsync(new GetWithoutLoadingQuery<Bar>())).FirstOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(queryFoo?.Name, Is.EqualTo(foo.Name), "Invalid Foo query name");
                Assert.That(queryBar?.Name, Is.EqualTo(foo.Bar.Name), "Invalid Bar query name");
                Assert.That(queryFoo?.FooId, Is.EqualTo(foo.FooId), "Invalid Foo query FooId");
                Assert.That(queryBar?.BarId, Is.EqualTo(foo.Bar.BarId), "Invalid Bar query BarId");
                Assert.That(queryFoo?.Bar?.BarId, Is.EqualTo(foo.Bar.BarId), "Invalid Foo query BarId");
                Assert.That(queryFoo?.Bar?.Name, Is.EqualTo(foo.Bar.Name), "Invalid Foo query Bar name");
            });
        }

        [Test]
        public async Task When_calling_both_Get_and_Query_Then_the_the_same_items_are_returned()
        {
            var foo = FooFactory.Create();

            await _fooRepository.AddAsync(foo);

            var getFoo = _fooRepository.Get(foo.FooId);
            var getBar = _barRepository.Get(foo.Bar.BarId);
            var queryFoo = _fooRepository.Query(new GetAllFoosQuery()).FirstOrDefault();
            var queryBar = _barRepository.Query(new GetWithoutLoadingQuery<Bar>()).FirstOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(queryFoo?.Name, Is.EqualTo(getFoo?.Name), "Invalid Foo query name");
                Assert.That(queryBar?.Name, Is.EqualTo(getBar?.Name), "Invalid Bar query name");
                Assert.That(queryFoo?.FooId, Is.EqualTo(getFoo?.FooId), "Invalid Foo query FooId");
                Assert.That(queryBar?.BarId, Is.EqualTo(getBar?.BarId), "Invalid Bar query BarId");
                Assert.That(queryFoo?.Bar?.Name, Is.EqualTo(getBar?.Name), "Invalid Foo query Bar name");
                Assert.That(queryFoo?.Bar?.BarId, Is.EqualTo(getBar?.BarId), "Invalid Foo query BarId");
            });
        }

        [Test]
        public async Task When_calling_both_GetAsync_and_Query_Then_the_the_same_items_are_returned()
        {
            var foo = FooFactory.Create();

            await _fooRepository.AddAsync(foo);

            var getFoo = await _fooRepository.GetAsync(foo.FooId);
            var getBar = await _barRepository.GetAsync(foo.Bar.BarId);
            var queryFoo = _fooRepository.Query(new GetAllFoosQuery()).FirstOrDefault();
            var queryBar = _barRepository.Query(new GetWithoutLoadingQuery<Bar>()).FirstOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(queryFoo?.Name, Is.EqualTo(getFoo?.Name), "Invalid Foo query name");
                Assert.That(queryBar?.Name, Is.EqualTo(getBar?.Name), "Invalid Bar query name");
                Assert.That(queryFoo?.FooId, Is.EqualTo(getFoo?.FooId), "Invalid Foo query FooId");
                Assert.That(queryBar?.BarId, Is.EqualTo(getBar?.BarId), "Invalid Bar query BarId");
                Assert.That(queryFoo?.Bar?.Name, Is.EqualTo(getBar?.Name), "Invalid Foo query Bar name");
                Assert.That(queryFoo?.Bar?.BarId, Is.EqualTo(getBar?.BarId), "Invalid Foo query BarId");
            });
        }

        [Test]
        public async Task When_calling_both_GetAsync_and_QueryAsync_Then_the_the_same_items_are_returned()
        {
            var foo = FooFactory.Create();

            await _fooRepository.AddAsync(foo);

            var getFoo = await _fooRepository.GetAsync(foo.FooId);
            var getBar = await _barRepository.GetAsync(foo.Bar.BarId);
            var queryFoo = (await _fooRepository.QueryAsync(new GetAllFoosQuery())).FirstOrDefault();
            var queryBar = (await _barRepository.QueryAsync(new GetWithoutLoadingQuery<Bar>())).FirstOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(queryFoo?.Name, Is.EqualTo(getFoo?.Name), "Invalid Foo query name");
                Assert.That(queryBar?.Name, Is.EqualTo(getBar?.Name), "Invalid Bar query name");
                Assert.That(queryFoo?.FooId, Is.EqualTo(getFoo?.FooId), "Invalid Foo query FooId");
                Assert.That(queryBar?.BarId, Is.EqualTo(getBar?.BarId), "Invalid Bar query BarId");
                Assert.That(queryFoo?.Bar?.Name, Is.EqualTo(getBar?.Name), "Invalid Foo query Bar name");
                Assert.That(queryFoo?.Bar?.BarId, Is.EqualTo(getBar?.BarId), "Invalid Foo query BarId");
            });
        }

        [Test]
        public async Task When_updating_an_entity_Then_the_expected_entity_is_updated()
        {
            var foo1 = FooFactory.Create(fooName: "foo 1", barName: "bar 1");
            var foo2 = FooFactory.Create(fooName: "foo 2", barName: "bar 2");
            var foo3 = FooFactory.Create(fooName: "foo 3", barName: "bar 3");

            await _fooRepository.AddAsync(foo1);
            await _fooRepository.AddAsync(foo2);
            await _fooRepository.AddAsync(foo3);

            const string expectedFoo1Name = "foo_1";
            const string expectedFoo2Name = "foo_2";
            const string expectedFoo3Name = "foo_3";

            foo1.Name = expectedFoo1Name;
            foo2.Name = expectedFoo2Name;
            foo3.Name = expectedFoo3Name;

            _fooRepository.Update(foo1);
            _fooRepository.Update(foo2);
            _fooRepository.Update(foo3);

            Assert.Multiple(() =>
            {
                Assert.That(foo1.Name, Is.EqualTo(expectedFoo1Name), "Invalid Foo 1 name");
                Assert.That(foo2.Name, Is.EqualTo(expectedFoo2Name), "Invalid Foo 2 name");
                Assert.That(foo3.Name, Is.EqualTo(expectedFoo3Name), "Invalid Foo 3 name");
            });
        }

        [Test]
        public async Task When_updating_entities_with_the_same_reference_entity_Then_it_gets_updated_correctly()
        {
            var foo1 = FooFactory.Create(fooName: "foo 1", barName: "bar shared");
            await _fooRepository.AddAsync(foo1);

            var foo2 = new Foo
            {
                Name = "foo 2",
                Bar = foo1.Bar
            };

            _fooRepository.Update(foo2);

            const string expectedBarName = "new bar name";

            var dbFoo1 = _fooRepository.Query(new GetFooWithIdQuery(foo1.FooId)).Single();
            dbFoo1.Bar.Name = expectedBarName;
            dbFoo1 = _fooRepository.Update(dbFoo1);

            var dbFoo2 = _fooRepository.Query(new GetFooWithIdQuery(foo2.FooId)).Single();

            Assert.Multiple(() =>
            {
                Assert.That(dbFoo1.Bar.Name, Is.EqualTo(expectedBarName), "Invalid Bar name");
                Assert.That(dbFoo2.Bar.Name, Is.EqualTo(expectedBarName), "Invalid Bar name");
            });
        }

        [Test]
        public async Task When_updating_an_entity_Then_entities_referencing_it_gets_updated_correctly()
        {
            var foo1 = FooFactory.Create(fooName: "foo 1", barName: "bar shared");
            await _fooRepository.AddAsync(foo1);

            var foo2 = new Foo
            {
                Name = "foo 2",
                Bar = foo1.Bar
            };

            _fooRepository.Update(foo2);

            const string expectedBarName = "new bar name";

            var dbBar = _barRepository.Get(foo1.Bar.BarId);
            dbBar.Name = expectedBarName;
            _barRepository.Update(dbBar);

            var dbFoo1 = _fooRepository.Query(new GetFooWithIdQuery(foo1.FooId)).Single();
            var dbFoo2 = _fooRepository.Query(new GetFooWithIdQuery(foo2.FooId)).Single();

            Assert.Multiple(() =>
            {
                Assert.That(dbFoo1.Bar.Name, Is.EqualTo(expectedBarName), "Invalid Bar name");
                Assert.That(dbFoo2.Bar.Name, Is.EqualTo(expectedBarName), "Invalid Bar name");
            });
        }

        [Test]
        public async Task When_deleting_an_entity_Then_the_correct_entity_gets_deleted()
        {
            var foo1 = FooFactory.Create(fooName: "foo 1", barName: "bar 1");
            var foo2 = FooFactory.Create(fooName: "foo 2", barName: "bar 2");
            var foo3 = FooFactory.Create(fooName: "foo 3", barName: "bar 3");

            await _fooRepository.AddAsync(foo1);
            await _fooRepository.AddAsync(foo2);
            await _fooRepository.AddAsync(foo3);

            _fooRepository.Delete(foo2);

            var fooCount = _fooRepository.Count(new GetWithoutLoadingQuery<Foo>());
            var dbFoo1 = _fooRepository.Get(foo1.FooId);
            var dbFoo2 = _fooRepository.Get(foo2.FooId);
            var dbFoo3 = _fooRepository.Get(foo3.FooId);

            Assert.Multiple(() =>
            {
                Assert.That(fooCount, Is.EqualTo(2), "Invalid Foo count");
                Assert.That(dbFoo1.Name, Is.EqualTo(foo1.Name), "Invalid Foo name");
                Assert.That(dbFoo2, Is.Null, "Foo not null");
                Assert.That(dbFoo3.Name, Is.EqualTo(foo3.Name), "Invalid Foo name");
            });
        }

        [Test]
        public async Task When_deleting_an_entity_Then_entities_referencing_it_with_restrict_referential_action_gets_updated_correctly()
        {
            var foo1 = FooFactory.Create(fooName: "foo 1", barName: "bar shared");
            await _fooRepository.AddAsync(foo1);

            var barId = foo1.Bar.BarId;

            var foo2 = new Foo
            {
                Name = "foo 2",
                Bar = foo1.Bar
            };

            _fooRepository.Update(foo2);

            // https://github.com/aspnet/EntityFrameworkCore/issues/6244
            foo1.BarId = null;
            foo1.Bar = null;
            foo2.BarId = null;
            foo2.Bar = null;

            _fooRepository.Update(foo1);
            _fooRepository.Update(foo2);

            var dbBar = _barRepository.Get(barId);
            _barRepository.Delete(dbBar);

            var dbFoo1 = _fooRepository.Query(new GetFooWithIdQuery(foo1.FooId)).Single();
            var dbFoo2 = _fooRepository.Query(new GetFooWithIdQuery(foo2.FooId)).Single();
            var bars = _barRepository.Query(new GetWithoutLoadingQuery<Bar>());

            Assert.Multiple(() =>
            {
                Assert.That(bars, Is.Empty, "Bars not empty");
                Assert.That(dbFoo1.Bar, Is.Null, "Bar not null");
                Assert.That(dbFoo2.Bar, Is.Null, "Bar not null");
            });
        }
    }
}