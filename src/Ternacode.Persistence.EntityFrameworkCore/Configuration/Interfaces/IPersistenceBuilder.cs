using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ternacode.Persistence.EntityFrameworkCore.Configuration.Interfaces
{
    public interface IPersistenceBuilder<TContext> where TContext : DbContext
    {
        IServiceCollection Services { get; set; }
    }
}