using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces
{
    internal interface IFlushService<TContext> where TContext : DbContext
    {
        void FlushChanges(TContext context);

        Task FlushChangesAsync(TContext context);
    }
}