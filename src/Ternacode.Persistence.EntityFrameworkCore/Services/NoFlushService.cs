using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.Services
{
    internal class NoFlushService<TContext> : IFlushService<TContext>
        where TContext : DbContext
    {
        public void FlushChanges(TContext context)
        {
        }

        public Task FlushChangesAsync(TContext context)
            => Task.CompletedTask;
    }
}