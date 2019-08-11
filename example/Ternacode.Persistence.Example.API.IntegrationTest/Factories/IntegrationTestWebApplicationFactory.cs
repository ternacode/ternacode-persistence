using Microsoft.AspNetCore.Mvc.Testing;

namespace Ternacode.Persistence.Example.API.IntegrationTest.Factories
{
    public class IntegrationTestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
    }
}