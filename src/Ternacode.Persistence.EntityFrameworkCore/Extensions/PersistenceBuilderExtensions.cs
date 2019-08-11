using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.EntityFrameworkCore.Configuration.Interfaces;
using Ternacode.Persistence.EntityFrameworkCore.Repositories;
using Ternacode.Persistence.EntityFrameworkCore.Services;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.Extensions
{
    public static class PersistenceBuilderExtensions
    {
        public static IPersistenceBuilder<TContext> AddEntity<TContext, TEntity>(
            this IPersistenceBuilder<TContext> builder,
            Func<TContext, DbSet<TEntity>> dbSetFunc)
            where TContext : DbContext
            where TEntity : class
        {
            if (!builder.Services.IsContextServiceAdded<TContext>())
                throw new InvalidOperationException($"Ensure '{nameof(ServiceCollectionExtensions.AddPersistence)}' is called before adding entity services");

            builder.Services.AddTransient<IDbSetService<TContext, TEntity>>(s => new DbSetService<TContext, TEntity>(dbSetFunc));
            builder.Services.AddTransient<IRepository<TEntity>, ContextRepository<TContext, TEntity>>();

            return builder;
        }
    }
}