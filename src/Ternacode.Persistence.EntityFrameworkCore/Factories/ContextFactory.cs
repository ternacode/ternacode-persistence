using System;
using Microsoft.EntityFrameworkCore;
using Ternacode.Persistence.EntityFrameworkCore.Factories.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.Factories
{
    internal class ContextFactory<TContext> : IContextFactory<TContext>
        where TContext: DbContext
    {
        private readonly Func<TContext> _func;

        public ContextFactory(Func<TContext> func)
        {
            _func = func;
        }

        public TContext CreateContext()
        {
            return _func();
        }
    }
}