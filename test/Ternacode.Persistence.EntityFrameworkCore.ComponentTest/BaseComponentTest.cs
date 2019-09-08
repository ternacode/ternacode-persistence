using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Contexts;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Factories;
using Ternacode.Persistence.EntityFrameworkCore.Configuration;
using Ternacode.Persistence.EntityFrameworkCore.Extensions;

namespace Ternacode.Persistence.EntityFrameworkCore.ComponentTest
{
    [TestFixture]
    public abstract class BaseComponentTest
    {
        protected ServiceProvider _serviceProvider;

        protected int _contextCount;

        [SetUp]
        public void SetUp()
        {
            _contextCount = 0;

            var services = new ServiceCollection();
            services.AddPersistence(CreateContextWithCount, GetPersistenceOptions())
                .AddEntity(c => c.Foos)
                .AddEntity(c => c.Bars);

            _serviceProvider = services.BuildServiceProvider();

            using (var context = CreateContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var context = CreateContext())
            {
                context.Database.EnsureDeleted();
            }
        }

        protected virtual PersistenceOptions GetPersistenceOptions()
            => null;

        protected virtual ComponentTestContext CreateContext()
            => ContextFactory.CreateContextWithInMemoryDb(nameof(BaseComponentTest));

        private ComponentTestContext CreateContextWithCount()
        {
            Interlocked.Add(ref _contextCount, 1);

            return CreateContext();
        }
    }
}
