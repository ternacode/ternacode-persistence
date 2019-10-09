using AutoFixture;
using Microsoft.AspNetCore.Mvc;

namespace Ternacode.Persistence.Example.API.UnitTest.Extensions
{
    public static class FixtureExtensions
    {
        public static T CreateController<T>(this Fixture fixture) where T : ControllerBase
            => fixture.Build<T>()
                .With(c => c.ControllerContext, new ControllerContext())
                .Create();
    }
}