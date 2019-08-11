using System.Threading;
using Microsoft.EntityFrameworkCore;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.Services
{
    internal class PooledContextService<TContext> : IContextService<TContext>
        where TContext : DbContext
    {
        private readonly IContextPool<TContext> _contextPool;

        private static readonly AsyncLocal<TContext> _asyncLocalContext = new AsyncLocal<TContext>();

        public PooledContextService(IContextPool<TContext> contextPool)
        {
            _contextPool = contextPool;
        }

        public TContext GetCurrentContext()
            => _asyncLocalContext.Value;

        public bool HasCurrentContext()
            => GetCurrentContext() != null;

        public TContext InitContext()
        {
            _asyncLocalContext.Value = _contextPool.Get();

            return _asyncLocalContext.Value;
        }

        public void ClearCurrentContext()
        {
            _contextPool.Return(_asyncLocalContext.Value);
            _asyncLocalContext.Value = null;
        }
    }
}