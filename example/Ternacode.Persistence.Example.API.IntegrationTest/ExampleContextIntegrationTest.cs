using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Ternacode.Persistence.Example.API.IntegrationTest.Factories;
using Ternacode.Persistence.Example.Database;

namespace Ternacode.Persistence.Example.API.IntegrationTest
{
    [TestFixture]
    public abstract class ExampleContextIntegrationTest
    {
        private IConfiguration _configuration;
        private IntegrationTestWebApplicationFactory<Startup> _clientFactory;

        [SetUp]
        public void Up()
        {
            CreateContext().Database.EnsureDeleted();
            CreateContext().Database.EnsureCreated();

            _clientFactory = new IntegrationTestWebApplicationFactory<Startup>();
        }

        [TearDown]
        public void Down()
        {
            CreateContext().Database.EnsureDeleted();
        }

        protected HttpClient CreateClient()
            => _clientFactory.CreateClient(new WebApplicationFactoryClientOptions
               {
                   BaseAddress = new Uri(_configuration.GetSection("ApiBaseUri").Value)
               });

        protected BlogContext CreateContext()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Test.json")
                .Build();

            var connectionString = _configuration.GetSection("ConnectionStrings:SqlServer").Value;

            if (!connectionString.Contains("IntegrationTestDb"))
                throw new ArgumentException("Ensure correct database connection string");

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(connectionString);

            return new BlogContext(optionsBuilder.Options);
        }
    }
}
