using Microsoft.EntityFrameworkCore;
using Ternacode.Persistence.Example.Domain.Model;

namespace Ternacode.Persistence.Example.Database
{
    public class BlogContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Post> Posts { get; set; }

        public BlogContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}
