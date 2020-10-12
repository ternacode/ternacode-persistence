using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Factories;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Model;
using Ternacode.Persistence.Extensions;

namespace Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Extensions
{
    public class RepositoryExtensions_Test : BaseComponentTest
    {
        [Test]
        public async Task When_querying_async_with_queryFunc_Then_the_expected_entity_is_returned()
        {
            const string expectedName = "foo1";

            var foo1 = FooFactory.Create(fooName: expectedName);
            var foo2 = FooFactory.Create(fooName: "foo2");

            var fooRepository = _serviceProvider.GetService<IRepository<Foo>>();
            
            await fooRepository.AddAsync(foo1);
            await fooRepository.AddAsync(foo2);

            var result = await fooRepository.QueryAsync(q => q.Where(f => f.Name == expectedName));

            Assert.Multiple(() =>
            {
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.FirstOrDefault()?.Name, Is.EqualTo(expectedName));
                Assert.That(result.FirstOrDefault()?.Bar, Is.Null);
            });
        }

        [Test]
        public async Task When_querying_async_with_queryFunc_and_loaded_proerties_Then_the_expected_entity_is_returned()
        {
            const string expectedFooName = "foo1";
            const string expectedBarName = "bar1";

            var foo1 = FooFactory.Create(fooName: expectedFooName, barName: expectedBarName);
            var foo2 = FooFactory.Create(fooName: "foo2");

            var fooRepository = _serviceProvider.GetService<IRepository<Foo>>();

            await fooRepository.AddAsync(foo1);
            await fooRepository.AddAsync(foo2);

            var result = await fooRepository.QueryAsync(
                queryFunc: q => q.Where(f => f.Name == expectedFooName),
                loadedPropertyNames: new []{ nameof(foo1.Bar) });

            Assert.Multiple(() =>
            {
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.FirstOrDefault()?.Name, Is.EqualTo(expectedFooName));
                Assert.That(result.FirstOrDefault()?.Bar?.Name, Is.EqualTo(expectedBarName));
            });
        }
    }
}