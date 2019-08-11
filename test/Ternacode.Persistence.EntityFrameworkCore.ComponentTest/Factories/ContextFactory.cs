using System;
using Microsoft.EntityFrameworkCore;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Contexts;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Enums;

namespace Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Factories
{
    public static class ContextFactory
    {
        public static ComponentTestContext CreateContext(DbType dbType, string dbName)
        {
            switch (dbType)
            {
                case DbType.InMemory: return CreateContextWithInMemoryDb(dbName);
                case DbType.MSSQL: return CreateContextWithMSSQLDb(dbName);
                default: throw new ArgumentException();
            }
        }

        public static ComponentTestContext CreateContextWithInMemoryDb(string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentException(nameof(dbName));

            var options = new DbContextOptionsBuilder<ComponentTestContext>()
                .UseInMemoryDatabase(databaseName: $"db-{dbName}")
                .Options;

            return new ComponentTestContext(options);
        }

        public static ComponentTestContext CreateContextWithMSSQLDb(string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentException(nameof(dbName));

            var connectionString = $"Data Source=.;Initial Catalog=db-{dbName};Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;";
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(connectionString);

            return new ComponentTestContext(optionsBuilder.Options);
        }
    }
}