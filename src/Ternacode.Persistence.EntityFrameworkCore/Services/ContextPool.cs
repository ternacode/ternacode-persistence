using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.Services
{
    internal class ContextPool<TContext> : DefaultObjectPool<TContext>, IContextPool<TContext>
        where TContext : DbContext
    {
        public ContextPool(IPooledObjectPolicy<TContext> policy)
            : base(policy)
        {
        }

        public ContextPool(IPooledObjectPolicy<TContext> policy, int maximumRetained)
            : base(policy, maximumRetained)
        {
        }
    }
}