using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using Ternacode.Persistence.EntityFrameworkCore.Services;
using Ternacode.Persistence.EntityFrameworkCore.UnitTest.Fakes;

namespace Ternacode.Persistence.EntityFrameworkCore.UnitTest.Services
{
    [TestFixture]
    public class ContextFlushService_Test
    {
        private CustomAutoFixture _fixture;
        private ContextFlushService<DbContext_Fake> _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new CustomAutoFixture();

            _sut = _fixture.Create<ContextFlushService<DbContext_Fake>>();
        }

        [Test]
        public void When_calling_FlushChanges_Then_SaveChanges_is_called_once()
        {
            var context = Substitute.For<DbContext_Fake>();

            _sut.FlushChanges(context);

            context.Received(1).SaveChanges();
        }

        [Test]
        public async Task When_calling_FlushChangesAsync_Then_SaveChangesAsync_is_called_once()
        {
            var context = Substitute.For<DbContext_Fake>();

            await _sut.FlushChangesAsync(context);

            await context.Received(1).SaveChangesAsync();
        }
    }
}