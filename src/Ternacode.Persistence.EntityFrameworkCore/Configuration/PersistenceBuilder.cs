using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ternacode.Persistence.EntityFrameworkCore.Configuration.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.Configuration
{
    public class PersistenceBuilder<TContext> : IPersistenceBuilder<TContext>
        where TContext : DbContext
    {
        public IServiceCollection Services { get; set; }

        public PersistenceBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
    }
}