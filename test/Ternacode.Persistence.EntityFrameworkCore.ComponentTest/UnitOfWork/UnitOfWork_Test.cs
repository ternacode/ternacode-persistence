using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Contexts;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Factories;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Model;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Queries;
using Ternacode.Persistence.EntityFrameworkCore.Configuration;
using Ternacode.Persistence.EntityFrameworkCore.Enums;
using Ternacode.Persistence.EntityFrameworkCore.Exceptions;

// ReSharper disable PossibleMultipleEnumeration

namespace Ternacode.Persistence.EntityFrameworkCore.ComponentTest.UnitOfWork
{
    [TestFixtureSource(nameof(TestConfigurations))]
    public class UnitOfWork_Test : BaseComponentTest
    {
        public static IEnumerable<object> TestConfigurations = new[]
        {
            new object[] { TransactionType.DbContextTransaction, false },
            new object[] { TransactionType.DbContextTransaction, true },
            new object[] { TransactionType.TransactionScope, false },
            new object[] { TransactionType.TransactionScope, true }
        };

        private readonly TransactionType _transactionType;
        private readonly bool _useContextPool;

        public UnitOfWork_Test(TransactionType transactionType, bool useContextPool)
        {
            _transactionType = transactionType;
            _useContextPool = useContextPool;
        }

        protected override PersistenceOptions GetPersistenceOptions()
            => new PersistenceOptions
            {
                UnitOfWorkTransactionType = _transactionType,
                UseContextPool = _useContextPool
            };

        protected override ComponentTestContext CreateContext()
        {
            // NOTE: Db Context transactions are not currenty supported by the EF Core InMemory database.
            // An MSSQL instance is used for unit of work tests.
            return ContextFactory.CreateContextWithMSSQLDb(nameof(UnitOfWork_Test));
        }

        [TestCase(1)]
        [TestCase(100)]
        public void When_adding_inside_a_unit_of_work_run_Then_the_expected_entities_are_added(int count)
        {
            Parallel.ForEach(Enumerable.Range(1, count), i =>
            {
                // Get a transient unit of work instance per run. Instances are not thread safe.
                var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();
                unitOfWork.Run(() =>
                {
                    var repo = _serviceProvider.GetService<IRepository<Foo>>();
                    repo.Add(FooFactory.Create(
                        string.Format(ExpectedFooName, i),
                        string.Format(ExpectedBarName, i)));
                });
            });

            var foos = _serviceProvider.GetService<IRepository<Foo>>().Query(new GetAllFoosQuery());
            var bars = _serviceProvider.GetService<IRepository<Bar>>().Query(new GetWithoutLoadingQuery<Bar>());

            AssertValidEntityCollections(foos, bars, count);
        }

        [TestCase(1)]
        [TestCase(100)]
        public void When_adding_async_inside_a_unit_of_work_run_Then_the_expected_entities_are_added(int count)
        {
            var tasks = Enumerable.Range(1, count)
                .AsParallel()
                .Select(async i =>
                {
                    var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();
                    await unitOfWork.Run(async () =>
                    {
                        var repo = _serviceProvider.GetService<IRepository<Foo>>();
                        await repo.AddAsync(FooFactory.Create(
                            string.Format(ExpectedFooName, i),
                            string.Format(ExpectedBarName, i)));
                    });
                }).ToArray();

            Task.WaitAll(tasks);

            var foos = _serviceProvider.GetService<IRepository<Foo>>().Query(new GetAllFoosQuery());
            var bars = _serviceProvider.GetService<IRepository<Bar>>().Query(new GetWithoutLoadingQuery<Bar>());

            AssertValidEntityCollections(foos, bars, count);
        }

        [TestCase(1)]
        [TestCase(100)]
        public async Task When_adding_async_with_return_value_inside_a_unit_of_work_run_Then_the_expected_entities_are_added(int count)
        {
            var resultTasks = Enumerable.Range(1, count)
                .AsParallel()
                .Select(i =>
                {
                    var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();
                    return unitOfWork.Run(async () =>
                    {
                        var foo = FooFactory.Create(
                            string.Format(ExpectedFooName, i),
                            string.Format(ExpectedBarName, i));

                        var repo = _serviceProvider.GetService<IRepository<Foo>>();
                        await repo.AddAsync(foo);

                        return repo.Query(new GetFooWithIdQuery(foo.FooId)).Single();
                    });
                })
                .ToArray();

            var result = await Task.WhenAll(resultTasks);

            var foos = _serviceProvider.GetService<IRepository<Foo>>().Query(new GetAllFoosQuery()).ToArray();
            var bars = _serviceProvider.GetService<IRepository<Bar>>().Query(new GetWithoutLoadingQuery<Bar>()).ToArray();

            Assert.That(result.Length, Is.EqualTo(foos.Length));

            foreach (var foo in result)
            {
                var matchingFoo = foos.FirstOrDefault(f => f.FooId == foo.FooId && f.Name == foo.Name);

                Assert.That(matchingFoo, Is.Not.Null);
            }

            AssertValidEntityCollections(foos, bars, count);
            AssertValidEntityCollections(result, bars, count);
        }

        [TestCase(1)]
        [TestCase(100)]
        public async Task When_adding_async_with_return_value_inside_a_unit_of_work_run_async_Then_the_expected_entities_are_added(int count)
        {
            var resultTasks = Enumerable.Range(1, count)
                .AsParallel()
                .Select(async i =>
                {
                    var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();
                    var createdFoo = await unitOfWork.RunAsync(async () =>
                    {
                        var foo = FooFactory.Create(
                            string.Format(ExpectedFooName, i),
                            string.Format(ExpectedBarName, i));

                        var repo = _serviceProvider.GetService<IRepository<Foo>>();
                        await repo.AddAsync(foo);

                        return repo.Query(new GetFooWithIdQuery(foo.FooId)).Single();
                    });

                    return createdFoo;
                })
                .ToArray();

            var result = await Task.WhenAll(resultTasks);

            var foos = _serviceProvider.GetService<IRepository<Foo>>().Query(new GetAllFoosQuery()).ToArray();
            var bars = _serviceProvider.GetService<IRepository<Bar>>().Query(new GetWithoutLoadingQuery<Bar>()).ToArray();

            Assert.That(result.Length, Is.EqualTo(foos.Length));

            foreach (var foo in result)
            {
                var matchingFoo = foos.FirstOrDefault(f => f.FooId == foo.FooId && f.Name == foo.Name);

                Assert.That(matchingFoo, Is.Not.Null);
            }

            AssertValidEntityCollections(foos, bars, count);
            AssertValidEntityCollections(result, bars, count);
        }

        [TestCase(1)]
        [TestCase(100)]
        public void When_adding_async_inside_a_unit_of_work_run_async_Then_the_expected_entities_are_added(int count)
        {
            var tasks = Enumerable.Range(1, count)
                .AsParallel()
                .Select(async i =>
                {
                    var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();
                    await unitOfWork.RunAsync(async () =>
                    {
                        var foo = FooFactory.Create(
                            string.Format(ExpectedFooName, i),
                            string.Format(ExpectedBarName, i));

                        var repo = _serviceProvider.GetService<IRepository<Foo>>();
                        await repo.AddAsync(foo);
                    });
                })
                .ToArray();

            Task.WaitAll(tasks);

            var foos = _serviceProvider.GetService<IRepository<Foo>>().Query(new GetAllFoosQuery()).ToArray();
            var bars = _serviceProvider.GetService<IRepository<Bar>>().Query(new GetWithoutLoadingQuery<Bar>()).ToArray();

            AssertValidEntityCollections(foos, bars, count);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(100)]
        public void When_an_exception_is_thrown_inside_a_run_Then_no_entities_are_added(int addedBefore)
        {
            Parallel.ForEach(Enumerable.Range(1, addedBefore),
                _ =>
                {
                    var repo = _serviceProvider.GetService<IRepository<Foo>>();
                    repo.Add(FooFactory.Create());
                });

            const string unitOfWorkFooName = "foo in unit of work";

            try
            {
                var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();
                unitOfWork.Run(() =>
                {
                    var foo = FooFactory.Create(fooName: unitOfWorkFooName);
                    var repo = _serviceProvider.GetService<IRepository<Foo>>();
                    repo.Add(foo);

                    var dbFoo = repo.Get(foo.FooId);
                    if (dbFoo?.Name == unitOfWorkFooName)
                    {
                        throw new Exception();
                    }
                });
            }
            catch
            { /* Ignore */ }

            var foos = _serviceProvider.GetService<IRepository<Foo>>().Query(new GetWithoutLoadingQuery<Foo>());

            Assert.Multiple(() =>
            {
                Assert.That(foos.Count(), Is.EqualTo(addedBefore), "Invalid Foo count");
                Assert.That(foos.All(f => f.Name != unitOfWorkFooName), "Invalid Foo in collection");
            });
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(100)]
        public async Task When_an_exception_is_thrown_inside_a_run_async_Then_no_entities_are_added(int addedBefore)
        {
            Parallel.ForEach(Enumerable.Range(1, addedBefore),
                _ =>
                {
                    var repo = _serviceProvider.GetService<IRepository<Foo>>();
                    repo.Add(FooFactory.Create());
                });

            const string unitOfWorkFooName = "foo in unit of work";

            try
            {
                var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();
                await unitOfWork.RunAsync(async () =>
                {
                    var foo = FooFactory.Create(fooName: unitOfWorkFooName);
                    var repo = _serviceProvider.GetService<IRepository<Foo>>();
                    await repo.AddAsync(foo);

                    var dbFoo = repo.Get(foo.FooId);
                    if (dbFoo?.Name == unitOfWorkFooName)
                    {
                        throw new Exception();
                    }
                });
            }
            catch
            { /* Ignore */ }

            var foos = _serviceProvider.GetService<IRepository<Foo>>().Query(new GetWithoutLoadingQuery<Foo>());

            Assert.Multiple(() =>
            {
                Assert.That(foos.Count(), Is.EqualTo(addedBefore), "Invalid Foo count");
                Assert.That(foos.All(f => f.Name != unitOfWorkFooName), "Invalid Foo in collection");
            });
        }

        [Test]
        public void When_adding_inside_manual_threads_and_an_exception_is_thrown_Then_no_entities_are_added()
        {
            var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();

            try
            {
                // The context created by the unit of work will flow to the child threads,
                // and will be part of the same transaction.
                unitOfWork.Run(() =>
                {
                    var t1 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));
                    var t2 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));

                    t1.Start();
                    t1.Join();

                    t2.Start();
                    t2.Join();

                    throw new Exception();
                });
            }
            catch
            { /* Ignore */ }

            Assert.That(_serviceProvider.GetService<IRepository<Foo>>().Count(new GetWithoutLoadingQuery<Foo>()), Is.EqualTo(0), "Invalid Foo count");
        }

        [Test]
        public void When_adding_inside_manual_threads_with_suppressed_flow_and_an_exception_is_thrown_Then_the_entities_are_added()
        {
            var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();

            try
            {
                using (ExecutionContext.SuppressFlow())
                {
                    // The context created by the unit of work will _not_ flow to the child threads.
                    // Threads t1 and t2 will get separate contexts, and will not be part of the transaction.
                    unitOfWork.Run(() =>
                    {
                        var t1 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));
                        var t2 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));

                        t1.Start();
                        t2.Start();

                        t1.Join();
                        t2.Join();

                        throw new Exception();
                    });
                }
            }
            catch
            { /* Ignore */ }

            Assert.That(_serviceProvider.GetService<IRepository<Foo>>().Count(new GetWithoutLoadingQuery<Foo>()), Is.EqualTo(2), "Invalid Foo count");
        }

        [Test]
        public void When_nesting_a_unit_of_work_run_action_Then_an_exception_is_thrown()
        {
            var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();

            Assert.That(() => unitOfWork.Run(() =>
            {
                _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create());

                unitOfWork.Run(() =>
                {
                    _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create());
                });
            }), Throws.InstanceOf<CurrentContextAlreadySetException>());
        }

        [Test]
        public void When_nesting_a_unit_of_work_run_func_Then_an_exception_is_thrown()
        {
            var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();

            Assert.That(() => unitOfWork.Run(() =>
            {
                _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create());

                unitOfWork.Run(() =>
                {
                    _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create());

                    return 0;
                });
            }), Throws.InstanceOf<CurrentContextAlreadySetException>());
        }

        [Test]
        public void When_nesting_a_unit_of_work_run_async_action_Then_an_exception_is_thrown()
        {
            var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();

            var e = Assert.Throws<AggregateException>(() => unitOfWork.Run(() =>
            {
                _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create());

                unitOfWork.RunAsync(() =>
                {
                    _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create());

                    return Task.CompletedTask;
                }).Wait();
            }));

            Assert.That(e.InnerExceptions.SingleOrDefault(), Is.InstanceOf<CurrentContextAlreadySetException>());
        }

        [Test]
        public void When_nesting_a_unit_of_work_run_async_func_Then_an_exception_is_thrown()
        {
            var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();

            var e = Assert.Throws<AggregateException>(() => unitOfWork.Run(() =>
            {
                _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create());

                var _ = unitOfWork.RunAsync(() =>
                {
                    _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create());

                    return Task.FromResult(0);
                }).Result;
            }));

            Assert.That(e.InnerExceptions.SingleOrDefault(), Is.InstanceOf<CurrentContextAlreadySetException>());
        }

        private void AssertValidEntityCollections(
            IEnumerable<Foo> foos,
            IEnumerable<Bar> bars,
            int count)
        {
            Assert.That(foos.Count(), Is.EqualTo(count), "Invalid Foos count");
            Assert.That(bars.Count(), Is.EqualTo(count), "Invalid Bars count");

            foreach (var i in Enumerable.Range(1, count))
            {
                var foo = foos.FirstOrDefault(f => f.Name == string.Format(ExpectedFooName, i));
                var bar = bars.FirstOrDefault(f => f.Name == string.Format(ExpectedBarName, i));

                Assert.That(foo, Is.Not.Null, "Invalid Foo");
                Assert.That(foo.Bar?.Name, Is.EqualTo(string.Format(ExpectedBarName, i)), "Invalid Bar name");
                Assert.That(bar, Is.Not.Null, "Invalid Bar");
            }

            CollectionAssert.AreEquivalent(
                foos.Select(f => f.Bar.BarId),
                bars.Select(b => b.BarId),
                "BarIds not equivalent");

            CollectionAssert.AreEquivalent(
                foos.Select(f => f.Bar.Name),
                bars.Select(b => b.Name),
                "Bar names not equivalent");
        }

        private string ExpectedFooName => "foo {0}";

        private string ExpectedBarName => "bar {0}";
    }
}