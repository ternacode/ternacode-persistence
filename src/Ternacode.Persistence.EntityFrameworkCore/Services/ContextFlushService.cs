using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.Services
{
    public class ContextFlushService<TContext> : IFlushService<TContext>
        where TContext : DbContext
    {
        public void FlushChanges(TContext context)
            => context.SaveChanges();

        public async Task FlushChangesAsync(TContext context)
            => await context.SaveChangesAsync();
    }
}