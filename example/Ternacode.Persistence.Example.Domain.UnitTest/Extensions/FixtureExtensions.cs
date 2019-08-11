using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoFixture;
using NSubstitute;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.Example.Database;
using Ternacode.Persistence.Example.Domain.UnitTest.Fakes;

namespace Ternacode.Persistence.Example.Domain.UnitTest.Extensions
{
    public static class FixtureExtensions
    {
        public static void StubPersistence(this CustomAutoFixture fixture)
        {
            fixture.Inject((IUnitOfWork)Substitute.ForPartsOf<UnitOfWork_Fake>());

            fixture.Entities = new List<object>();
            fixture.StubRepositories();
        }

        private static void StubRepositories(this CustomAutoFixture fixture)
        {
            var entityTypes = typeof(BlogContext).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.Name == "DbSet`1" && p.PropertyType.GenericTypeArguments.Length == 1)
                .Select(p => p.PropertyType.GenericTypeArguments[0]);

            foreach (var entityType in entityTypes)
            {
                var method = typeof(FixtureExtensions).GetMethod(nameof(StubRepository), BindingFlags.Static | BindingFlags.NonPublic);
                var genericMethod = method.MakeGenericMethod(entityType);
                genericMethod.Invoke(null, new object[] { fixture, fixture.Entities });
            }
        }

        private static void StubRepository<T>(
            this IFixture fixture,
            ICollection<object> entityCollection) where T : class
        {
            var repo = fixture.Freeze<IRepository<T>>();

            repo.Get(Arg.Any<object>())
                .Returns(ci => entityCollection.SingleOrDefault(
                    e => e.GetType() == typeof(T)
                         && (dynamic)GetEntityId((T)e) == (dynamic)ci.Arg<object>()));

            repo.When(r => r.Add(Arg.Any<T>()))
                .Do(ci => entityCollection.Add(ci.Arg<T>()));

            repo.When(r => r.AddAsync(Arg.Any<T>()))
                .Do(ci => entityCollection.Add(ci.Arg<T>()));

            repo.Query(Arg.Any<IQuery<T>>())
                .Returns(c => c.Arg<IQuery<T>>()
                    .Query(entityCollection.OfType<T>().AsQueryable()));

            repo.Count(Arg.Any<IQuery<T>>())
                .Returns(c => c.Arg<IQuery<T>>()
                    .Query(entityCollection.OfType<T>().AsQueryable())
                    .Count());
        }

        private static object GetEntityId<T>(T entity)
        {
            var type = entity.GetType();
            var propertyName = $"{type.Name}Id";

            var idProperties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (idProperties.Count == 0)
                throw new ArgumentException($"No default id property '{propertyName}' found on entity type '{type.Name}'");

            if (idProperties.Count != 1)
                throw new ArgumentException($"Multiple matching id properties '{propertyName}' found on entity type '{type.Name}'");

            return idProperties[0].GetValue(entity);
        }
    }
}