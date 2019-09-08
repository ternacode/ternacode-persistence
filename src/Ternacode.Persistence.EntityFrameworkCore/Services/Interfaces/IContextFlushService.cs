using Microsoft.EntityFrameworkCore;

namespace Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces
{
    internal interface IContextFlushService<TContext> : IFlushService<TContext>
        where TContext : DbContext
    {
    }
}