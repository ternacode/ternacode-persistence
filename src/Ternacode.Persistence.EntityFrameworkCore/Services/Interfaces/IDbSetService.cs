using Microsoft.EntityFrameworkCore;

namespace Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces
{
    internal interface IDbSetService<TContext, TEntity>
        where TContext : DbContext
        where TEntity : class
    {
        DbSet<TEntity> GetDbSet(TContext context);
    }
}