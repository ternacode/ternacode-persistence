using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;
using Ternacode.Persistence.EntityFrameworkCore.Factories.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.Services
{
    internal class ContextPolicy<TContext> : PooledObjectPolicy<TContext>
        where TContext : DbContext
    {
        private readonly IContextFactory<TContext> _contextFactory;

        public ContextPolicy(IContextFactory<TContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public override TContext Create()
            => _contextFactory.CreateContext();

        public override bool Return(TContext obj)
            => true;
    }
}