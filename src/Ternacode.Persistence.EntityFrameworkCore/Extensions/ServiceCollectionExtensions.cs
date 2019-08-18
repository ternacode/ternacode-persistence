using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.EntityFrameworkCore.Configuration;
using Ternacode.Persistence.EntityFrameworkCore.Configuration.Interfaces;
using Ternacode.Persistence.EntityFrameworkCore.Enums;
using Ternacode.Persistence.EntityFrameworkCore.Factories;
using Ternacode.Persistence.EntityFrameworkCore.Factories.Interfaces;
using Ternacode.Persistence.EntityFrameworkCore.Services;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;
using Ternacode.Persistence.EntityFrameworkCore.UnitOfWork;

namespace Ternacode.Persistence.EntityFrameworkCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IPersistenceBuilder<TContext> AddPersistence<TContext>(
            this IServiceCollection services,
            Func<TContext> contextFactory)
            where TContext : DbContext
            => services.AddPersistence(contextFactory: contextFactory, options: null);

        public static IPersistenceBuilder<TContext> AddPersistence<TContext>(
            this IServiceCollection services,
            Func<TContext> contextFactory,
            PersistenceOptions options)
            where TContext : DbContext
        {
            if (services.IsContextServiceAdded<TContext>())
                throw new InvalidOperationException("Persistence services already added");

            if (contextFactory == null)
                throw new ArgumentNullException(nameof(contextFactory));

            services.AddContextServices<TContext>(options, contextFactory)
                    .AddUnitOfWork<TContext>(options);

            return new PersistenceBuilder<TContext>(services);
        }

        private static IServiceCollection AddContextServices<TContext>(
            this IServiceCollection services,
            PersistenceOptions options,
            Func<TContext> contextFactory)
            where TContext : DbContext
        {
            services.AddSingleton<IContextFactory<TContext>>(s => new ContextFactory<TContext>(contextFactory));

            if (options == null)
            {
                return services.AddTransient<IContextService<TContext>, ContextService<TContext>>()
                               .AddTransient<IFlushService<TContext>, ContextFlushService<TContext>>();
            }

            if (options.UseContextPool)
            {
                services.AddSingleton<IPooledObjectPolicy<TContext>, ContextPolicy<TContext>>()
                        .AddSingleton<IContextPool<TContext>, ContextPool<TContext>>()
                        .AddTransient<IContextService<TContext>, PooledContextService<TContext>>();
            }
            else
            {
                services.AddTransient<IContextService<TContext>, ContextService<TContext>>();
            }

            if (options.UseManualRepositoryFlush)
            {
                services.AddTransient<IFlushService<TContext>, NoFlushService<TContext>>();
            }
            else
            {
                services.AddTransient<IFlushService<TContext>, ContextFlushService<TContext>>();
            }

            return services;
        }

        private static IServiceCollection AddUnitOfWork<TContext>(
            this IServiceCollection services,
            PersistenceOptions options)
            where TContext : DbContext
        {
            if (options == null)
                return services.AddTransient<IUnitOfWork, TransactionScopeUnitOfWork<TContext>>();

            switch (options.UnitOfWorkTransactionType)
            {
                case TransactionType.TransactionScope:
                    return services.AddTransient<IUnitOfWork, TransactionScopeUnitOfWork<TContext>>();
                case TransactionType.DbContextTransaction:
                    return services.AddTransient<IUnitOfWork, DbContextTransactionUnitOfWork<TContext>>();
                default:
                    throw new ArgumentOutOfRangeException(nameof(options.UnitOfWorkTransactionType));
            }
        }

        internal static bool IsContextServiceAdded<TContext>(this IServiceCollection services)
            where TContext : DbContext
            => services.Any(s => s.ServiceType == typeof(IContextService<TContext>));
    }
}