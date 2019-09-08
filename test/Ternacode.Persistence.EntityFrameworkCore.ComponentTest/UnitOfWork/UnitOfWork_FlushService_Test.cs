using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Contexts;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Factories;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Model;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Queries;
using Ternacode.Persistence.EntityFrameworkCore.Configuration;

namespace Ternacode.Persistence.EntityFrameworkCore.ComponentTest.UnitOfWork
{
    public class UnitOfWork_FlushService_Test
    {
        [TestFixture(false)]
        [TestFixture(true)]
        public class When_setting_manual_flush : BaseComponentTest
        {
            private readonly bool _useManualFlush;

            public When_setting_manual_flush(bool useManualFlush)
            {
                _useManualFlush = useManualFlush;
            }

            protected override PersistenceOptions GetPersistenceOptions()
                => new PersistenceOptions
                {
                    UseManualRepositoryFlush = _useManualFlush
                };

            protected override ComponentTestContext CreateContext()
                => ContextFactory.CreateContextWithMSSQLDb(nameof(When_setting_manual_flush));

            [Test]
            public void Then_the_expected_entities_are_added()
            {
                var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();

                unitOfWork.Run(() =>
                {
                    var foo = FooFactory.Create();
                    var repo = _serviceProvider.GetService<IRepository<Foo>>();

                    repo.Add(foo);
                });

                var foos = _serviceProvider.GetService<IRepository<Foo>>().Query(new GetWithoutLoadingQuery<Foo>());
                var bars = _serviceProvider.GetService<IRepository<Bar>>().Query(new GetWithoutLoadingQuery<Bar>());

                Assert.Multiple(() =>
                {
                    Assert.That(foos.Count(), Is.EqualTo(1), "Invalid foo count");
                    Assert.That(bars.Count(), Is.EqualTo(1), "Invalid bar count");
                });
            }
        }
    }
}