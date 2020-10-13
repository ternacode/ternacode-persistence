using System;
using AutoFixture;
using NUnit.Framework;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.EntityFrameworkCore.Repositories;
using Ternacode.Persistence.EntityFrameworkCore.UnitTest.Fakes;
using Ternacode.Persistence.EntityFrameworkCore.UnitTest.Model;
using Ternacode.Persistence.Extensions;

namespace Ternacode.Persistence.EntityFrameworkCore.UnitTest.Extensions
{
    [TestFixture]
    public class RepositoryExtensions_Test
    {
        private CustomAutoFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new CustomAutoFixture();
        }

        [Test]
        public void When_calling_QueryAsync_with_null_repository_Then_ArgumentNullException_is_thrown()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => RepositoryExtensions.QueryAsync((IRepository<Foo>)null, q => q), Throws.InstanceOf<ArgumentNullException>());
                Assert.That(() => RepositoryExtensions.QueryAsync((IRepository<Foo>)null, q => q, _fixture.CreateMany<string>()), Throws.InstanceOf<ArgumentNullException>());
            });
        }

        [Test]
        public void When_calling_QueryAsync_with_null_queryFunc_Then_ArgumentNullException_is_thrown()
        {
            var repository = _fixture.Create<ContextRepository<DbContext_Fake, Foo>>();

            Assert.Multiple(() =>
            {
                Assert.That(() => repository.QueryAsync(null), Throws.InstanceOf<ArgumentNullException>());
                Assert.That(() => repository.QueryAsync(null, _fixture.CreateMany<string>()), Throws.InstanceOf<ArgumentNullException>());
            });
        }

        [Test]
        public void When_calling_QueryAsync_with_null_loadedPropertyNames_Then_ArgumentNullException_is_thrown()
        {
            var repository = _fixture.Create<ContextRepository<DbContext_Fake, Foo>>();

            Assert.That(() => repository.QueryAsync(q => q, null), Throws.InstanceOf<ArgumentNullException>());
        }
    }
}