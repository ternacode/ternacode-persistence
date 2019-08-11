using Microsoft.EntityFrameworkCore;

namespace Ternacode.Persistence.EntityFrameworkCore.Factories.Interfaces
{
    internal interface IContextFactory<TContext> where TContext : DbContext
    {
        TContext CreateContext();
    }
}