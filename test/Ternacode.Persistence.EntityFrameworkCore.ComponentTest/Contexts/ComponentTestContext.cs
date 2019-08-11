using Microsoft.EntityFrameworkCore;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Model;

namespace Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Contexts
{
    public class ComponentTestContext : DbContext
    {
        public DbSet<Foo> Foos { get; set; }

        public DbSet<Bar> Bars { get; set; }

        public ComponentTestContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}