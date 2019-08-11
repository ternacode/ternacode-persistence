using System.Threading;
using Microsoft.EntityFrameworkCore;
using Ternacode.Persistence.EntityFrameworkCore.Exceptions;
using Ternacode.Persistence.EntityFrameworkCore.Factories.Interfaces;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.Services
{
    internal class ContextService<TContext> : IContextService<TContext>
        where TContext : DbContext
    {
        private readonly IContextFactory<TContext> _contextFactory;

        private static readonly AsyncLocal<TContext> _asyncLocalContext = new AsyncLocal<TContext>();

        public ContextService(IContextFactory<TContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public TContext GetCurrentContext()
            => _asyncLocalContext.Value;

        public bool HasCurrentContext()
            => GetCurrentContext() != null;

        public TContext InitContext()
        {
            if (HasCurrentContext())
                throw new CurrentContextAlreadySetException();

            var context = _contextFactory.CreateContext();
            _asyncLocalContext.Value = context;

            return context;
        }

        public void ClearCurrentContext()
        {
            if (!HasCurrentContext())
                throw new CurrentContextNotSetException();

            _asyncLocalContext.Value.Dispose();
            _asyncLocalContext.Value = null;
        }
    }
}