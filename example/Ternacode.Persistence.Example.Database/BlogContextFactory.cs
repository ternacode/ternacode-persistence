using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ternacode.Persistence.Example.Database
{
    public class BlogContextFactory : IDesignTimeDbContextFactory<BlogContext>
    {
        private const string DEFAULT_CONNECTION_STRING = "Data Source=.;Initial Catalog=ExampleDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;";

        private readonly string _connectionString;

        public BlogContextFactory()
        {
            // Used by design time migrations
        }

        public BlogContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public BlogContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(_connectionString ?? DEFAULT_CONNECTION_STRING);

            return new BlogContext(optionsBuilder.Options);
        }

        public BlogContext CreateDbContext()
            => CreateDbContext(new string[] { });
    }
}