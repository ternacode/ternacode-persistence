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

namespace Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Repository
{
    public class Repository_ContextPool_Test
    {
        public class When_using_context_pool : BaseComponentTest
        {
            protected override PersistenceOptions GetPersistenceOptions()
                => new PersistenceOptions
                {
                    UseContextPool = true
                };

            protected override ComponentTestContext CreateContext()
                => ContextFactory.CreateContextWithMSSQLDb(nameof(When_using_context_pool));

            [TestCase(1, 1)]
            [TestCase(10, 10)]
            [TestCase(100, 16)]
            [TestCase(1000, 16)]
            public void Then_the_context_created_count_does_not_exceed_max_pool_size_for_parallel_adds(int entityCount, int expectedMaxContextCount)
            {
                Enumerable.Range(0, entityCount)
                    .AsParallel()
                    .ForAll(i =>
                    {
                        var repo = _serviceProvider.GetService<IRepository<Foo>>();
                        repo.Add(FooFactory.Create());
                    });

                Assert.Multiple(() =>
                {
                    Assert.That(_contextCount, Is.GreaterThan(0), "Context count too low");
                    Assert.That(_contextCount, Is.LessThanOrEqualTo(expectedMaxContextCount), "Context count too high");
                });
            }

            [TestCase(1)]
            [TestCase(10)]
            [TestCase(1000)]
            public void Then_only_one_context_is_created_for_sequential_adds(int entityCount)
            {
                foreach (var _ in Enumerable.Range(0, entityCount))
                {
                    var repo = _serviceProvider.GetService<IRepository<Foo>>();
                    repo.Add(FooFactory.Create());
                }

                Assert.That(_contextCount, Is.EqualTo(1));
            }

            [Test]
            public async Task When_querying_without_loading_for_the_same_context_in_pool_Then_the_entity_property_is_not_null()
            {
                await _serviceProvider.GetService<IRepository<Foo>>().AddAsync(FooFactory.Create());

                var queryFoo = _serviceProvider.GetService<IRepository<Foo>>().Query(new GetWithoutLoadingQuery<Foo>()).FirstOrDefault();

                // Same context from the pool used for both repository calls, Bar will still be loaded.
                Assert.That(queryFoo?.Bar, Is.Not.Null);
            }

            [Test]
            public void Then_only_one_context_is_created_for_a_unit_of_work_run_with_manual_threads()
            {
                // The context created by the unit of work will be propagated to child threads t1 and t2,
                // why we cannot execute them concurrently.
                // See: https://stackoverflow.com/questions/43814346/why-does-asynclocalt-propagate-to-child-threads-where-as-threadlocalt-does-n

                var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();
                unitOfWork.Run(() =>
                {
                    var t1 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));
                    var t2 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));

                    t1.Start();
                    t1.Join();

                    t2.Start();
                    t2.Join();
                });

                Assert.Multiple(() =>
                {
                    Assert.That(_contextCount, Is.EqualTo(1), "Invalid context count");
                    Assert.That(_serviceProvider.GetService<IRepository<Foo>>().Count(new GetWithoutLoadingQuery<Foo>()), Is.EqualTo(2), "Invalid Foo count");
                });
            }

            [Test]
            public void Then_the_expected_number_of_contexts_are_created_for_a_unit_of_work_run_with_sequential_manual_threads_and_suppressed_flow()
            {
                // The context created by the unit of work will _not_ be propagated to child threads t1 and t2,
                // since we are suppressing flowing the AsyncLocal value downwards.

                // Thread t2 will re-use the same context as t1 from the pool.
                const int expectedContextCount = 2;

                using (ExecutionContext.SuppressFlow())
                {
                    var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();
                    unitOfWork.Run(() =>
                    {
                        var t1 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));
                        var t2 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));

                        t1.Start();
                        t1.Join();

                        t2.Start();
                        t2.Join();
                    });
                }

                Assert.Multiple(() =>
                {
                    Assert.That(_contextCount, Is.EqualTo(expectedContextCount), "Invalid context count");
                    Assert.That(_serviceProvider.GetService<IRepository<Foo>>().Count(new GetWithoutLoadingQuery<Foo>()), Is.EqualTo(2), "Invalid Foo count");
                });
            }

            [Test]
            public void Then_the_expected_number_of_contexts_are_created_for_a_unit_of_work_run_with_parallel_manual_threads_and_suppressed_flow()
            {
                // The context created by the unit of work will _not_ be propagated to child threads t1 and t2,
                // since we are suppressing flowing the AsyncLocal value downwards.

                // Thread t2 and t1 will get one context each from the pool.
                const int expectedContextCount = 3;

                using (ExecutionContext.SuppressFlow())
                {
                    var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();
                    unitOfWork.Run(() =>
                    {
                        var t1 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));
                        var t2 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));

                        t1.Start();
                        t2.Start();

                        t1.Join();
                        t2.Join();
                    });
                }

                Assert.Multiple(() =>
                {
                    Assert.That(_contextCount, Is.EqualTo(expectedContextCount), "Invalid context count");
                    Assert.That(_serviceProvider.GetService<IRepository<Foo>>().Count(new GetWithoutLoadingQuery<Foo>()), Is.EqualTo(2), "Invalid Foo count");
                });
            }
        }

        public class When_not_using_context_pool : BaseComponentTest
        {
            protected override PersistenceOptions GetPersistenceOptions()
                => new PersistenceOptions
                {
                    UseContextPool = false
                };

            protected override ComponentTestContext CreateContext()
                => ContextFactory.CreateContextWithMSSQLDb(nameof(When_not_using_context_pool));

            [TestCase(1)]
            [TestCase(10)]
            [TestCase(1000)]
            public void Then_one_context_per_added_entity_is_created_for_parallel_adds(int entityCount)
            {
                Enumerable.Range(0, entityCount)
                    .AsParallel()
                    .ForAll(i =>
                    {
                        var repo = _serviceProvider.GetService<IRepository<Foo>>();
                        repo.Add(FooFactory.Create());
                    });

                Assert.That(_contextCount, Is.EqualTo(entityCount));
            }

            [TestCase(1)]
            [TestCase(10)]
            [TestCase(1000)]
            public void Then_one_context_per_added_entity_is_created_for_sequential_adds(int entityCount)
            {
                foreach (var _ in Enumerable.Range(0, entityCount))
                {
                    var repo = _serviceProvider.GetService<IRepository<Foo>>();
                    repo.Add(FooFactory.Create());
                }

                Assert.That(_contextCount, Is.EqualTo(entityCount));
            }

            [Test]
            public async Task When_querying_without_loading_Then_the_entity_property_is_null()
            {
                var repo = _serviceProvider.GetService<IRepository<Foo>>();
                await repo.AddAsync(FooFactory.Create());

                var queryFoo = repo.Query(new GetWithoutLoadingQuery<Foo>()).FirstOrDefault();

                Assert.That(queryFoo?.Bar, Is.Null);
            }

            [Test]
            public void Then_only_one_context_is_created_for_a_unit_of_work_run_with_manual_threads()
            {
                // The context created by the unit of work will be propagated to child threads t1 and t2,
                // why we cannot execute them concurrently.
                // See: https://stackoverflow.com/questions/43814346/why-does-asynclocalt-propagate-to-child-threads-where-as-threadlocalt-does-n

                var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();
                unitOfWork.Run(() =>
                {
                    var t1 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));
                    var t2 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));

                    t1.Start();
                    t1.Join();

                    t2.Start();
                    t2.Join();
                });

                Assert.Multiple(() =>
                {
                    Assert.That(_contextCount, Is.EqualTo(1), "Invalid context count");
                    Assert.That(_serviceProvider.GetService<IRepository<Foo>>().Count(new GetWithoutLoadingQuery<Foo>()), Is.EqualTo(2), "Invalid Foo count");
                });
            }

            [Test]
            public void Then_the_expected_number_of_contexts_are_created_for_a_unit_of_work_run_with_parallel_manual_threads_and_suppressed_flow()
            {
                // The context created by the unit of work will _not_ be propagated to child threads t1 and t2,
                // since we are suppressing flowing the AsyncLocal value downwards.

                // Threads t1 and t2 get different contexts.
                const int expectedContextCount = 3;

                using (ExecutionContext.SuppressFlow())
                {
                    var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();
                    unitOfWork.Run(() =>
                    {
                        var t1 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));
                        var t2 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));

                        t1.Start();
                        t2.Start();

                        t1.Join();
                        t2.Join();
                    });
                }

                Assert.Multiple(() =>
                {
                    Assert.That(_contextCount, Is.EqualTo(expectedContextCount), "Invalid context count");
                    Assert.That(_serviceProvider.GetService<IRepository<Foo>>().Count(new GetWithoutLoadingQuery<Foo>()), Is.EqualTo(2), "Invalid Foo count");
                });
            }

            [Test]
            public void Then_the_expected_number_of_contexts_are_created_for_a_unit_of_work_run_with_sequential_manual_threads_and_suppressed_flow()
            {
                // The context created by the unit of work will _not_ be propagated to child threads t1 and t2,
                // since we are suppressing flowing the AsyncLocal value downwards.

                // Threads t1 and t2 get different contexts.
                const int expectedContextCount = 3;

                using (ExecutionContext.SuppressFlow())
                {
                    var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();
                    unitOfWork.Run(() =>
                    {
                        var t1 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));
                        var t2 = new Thread(() => _serviceProvider.GetService<IRepository<Foo>>().Add(FooFactory.Create()));

                        t1.Start();
                        t1.Join();

                        t2.Start();
                        t2.Join();
                    });
                }

                Assert.Multiple(() =>
                {
                    Assert.That(_contextCount, Is.EqualTo(expectedContextCount), "Invalid context count");
                    Assert.That(_serviceProvider.GetService<IRepository<Foo>>().Count(new GetWithoutLoadingQuery<Foo>()), Is.EqualTo(2), "Invalid Foo count");
                });
            }
        }
    }
}