using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.Repositories
{
    internal class ContextRepository<TContext, TEntity> : IRepository<TEntity>
        where TContext : DbContext
        where TEntity : class
    {
        private readonly IContextService<TContext> _contextService;
        private readonly IDbSetService<TContext, TEntity> _dbSetService;
        private readonly IFlushService<TContext> _flushService;

        public ContextRepository(
            IContextService<TContext> contextService,
            IDbSetService<TContext, TEntity> dbSetService,
            IFlushService<TContext> flushService)
        {
            _contextService = contextService;
            _dbSetService = dbSetService;
            _flushService = flushService;
        }

        public void Add(TEntity entity)
            => AddAsync(entity).GetAwaiter().GetResult();

        public async Task AddAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var (context, hasOwnContext) = GetCurrentContext();
            try
            {
                await _dbSetService.GetDbSet(context)
                    .AddAsync(entity);

                await _flushService.FlushChangesAsync(context);
            }
            finally
            {
                ClearOwnContext(hasOwnContext);
            }
        }

        public TEntity Get(object id)
            => GetAsync(id).GetAwaiter().GetResult();

        public async Task<TEntity> GetAsync(object id)
        {
            var (context, hasOwnContext) = GetCurrentContext();
            try
            {
                return await _dbSetService.GetDbSet(context)
                    .FindAsync(id);
            }
            finally
            {
                ClearOwnContext(hasOwnContext);
            }
        }

        public TEntity Update(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var (context, hasOwnContext) = GetCurrentContext();
            try
            {
                var updatedEntity = _dbSetService.GetDbSet(context)
                    .Update(entity);

                _flushService.FlushChanges(context);

                return updatedEntity?.Entity;
            }
            finally
            {
                ClearOwnContext(hasOwnContext);
            }
        }

        public void Delete(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var (context, hasOwnContext) = GetCurrentContext();
            try
            {
                _dbSetService.GetDbSet(context)
                    .Remove(entity);

                _flushService.FlushChanges(context);
            }
            finally
            {
                ClearOwnContext(hasOwnContext);
            }
        }

        public IEnumerable<TEntity> Query(IQuery<TEntity> query)
            => QueryAsync(query).GetAwaiter().GetResult();

        public async Task<IEnumerable<TEntity>> QueryAsync(IQuery<TEntity> query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var (context, hasOwnContext) = GetCurrentContext();
            try
            {
                var queryable = _dbSetService.GetDbSet(context)
                    .AsQueryable();

                var queryResult = query.Query(queryable);
                queryResult = query.GetLoadedProperties()
                    .Aggregate(queryResult, (current, property) => current.Include(property));

                return await queryResult.ToListAsync();
            }
            finally
            {
                ClearOwnContext(hasOwnContext);
            }
        }

        public int Count(IQuery<TEntity> query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var (context, hasOwnContext) = GetCurrentContext();
            try
            {
                var queryable = _dbSetService.GetDbSet(context)
                    .AsQueryable();

                return query.Query(queryable)
                    .Count();
            }
            finally
            {
                ClearOwnContext(hasOwnContext);
            }
        }

        public bool Any(IQuery<TEntity> query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var (context, hasOwnContext) = GetCurrentContext();
            try
            {
                var queryable = _dbSetService.GetDbSet(context)
                    .AsQueryable();

                return query.Query(queryable)
                    .Any();
            }
            finally
            {
                ClearOwnContext(hasOwnContext);
            }
        }

        private (TContext, bool) GetCurrentContext()
        {
            var hasOwnContext = false;
            if (!_contextService.HasCurrentContext())
            {
                _contextService.InitContext();
                hasOwnContext = true;
            }

            return (_contextService.GetCurrentContext(), hasOwnContext);
        }

        private void ClearOwnContext(bool hasOwnContext)
        {
            if (hasOwnContext)
                _contextService.ClearCurrentContext();
        }
    }
}