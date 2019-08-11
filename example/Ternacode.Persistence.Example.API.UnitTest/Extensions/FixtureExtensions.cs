using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Ternacode.Persistence.Example.API.UnitTest.Extensions
{
    public static class FixtureExtensions
    {
        public static T CreateController<T>(this Fixture fixture)
            where T : Controller
        {
            var viewDataDictionary = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary());

            return fixture.Build<T>()
                .With(c => c.ViewData, viewDataDictionary)
                .Create();
        }
    }
}