using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using Ternacode.Persistence.EntityFrameworkCore.Services;
using Ternacode.Persistence.EntityFrameworkCore.UnitTest.Fakes;

namespace Ternacode.Persistence.EntityFrameworkCore.UnitTest.Services
{
    [TestFixture]
    public class NoFlushService_Test
    {
        private CustomAutoFixture _fixture;
        private NoFlushService<DbContext_Fake> _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new CustomAutoFixture();

            _sut = _fixture.Create<NoFlushService<DbContext_Fake>>();
        }

        [Test]
        public async Task When_calling_FlushChanges_Then_SaveChanges_is_not_called()
        {
            var context = Substitute.For<DbContext_Fake>();

            _sut.FlushChanges(context);

            context.Received(0).SaveChanges();
            await context.Received(0).SaveChangesAsync();
        }

        [Test]
        public async Task When_calling_FlushChangesAsync_Then_SaveChanges_is_not_called()
        {
            var context = Substitute.For<DbContext_Fake>();

            await _sut.FlushChangesAsync(context);

            context.Received(0).SaveChanges();
            await context.Received(0).SaveChangesAsync();
        }
    }
}