using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ternacode.Persistence.Abstractions;

namespace Ternacode.Persistence.Extensions
{
    public static class RepositoryExtensions
    {
        public static async Task<IEnumerable<TEntity>> QueryAsync<TEntity>(
            this IRepository<TEntity> repository,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> queryFunc)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (queryFunc == null)
            {
                throw new ArgumentNullException(nameof(queryFunc));
            }

            return await repository.QueryAsync(new FuncQuery<TEntity>(queryFunc));
        }

        public static async Task<IEnumerable<TEntity>> QueryAsync<TEntity>(
            this IRepository<TEntity> repository,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> queryFunc,
            IEnumerable<string> loadedPropertyNames)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (queryFunc == null)
            {
                throw new ArgumentNullException(nameof(queryFunc));
            }

            if (loadedPropertyNames == null)
            {
                throw new ArgumentNullException(nameof(loadedPropertyNames));
            }

            return await repository.QueryAsync(new FuncQuery<TEntity>(queryFunc, loadedPropertyNames));
        }

        private class FuncQuery<TEntity> : BaseQuery<TEntity>
        {
            private readonly Func<IQueryable<TEntity>, IQueryable<TEntity>> _queryFunc;
            private readonly IEnumerable<string> _loadedPropertyNames;

            public FuncQuery(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryFunc)
                => _queryFunc = queryFunc;

            public FuncQuery(
                Func<IQueryable<TEntity>, IQueryable<TEntity>> queryFunc,
                IEnumerable<string> loadedPropertyNames)
            {
                _queryFunc = queryFunc;
                _loadedPropertyNames = loadedPropertyNames;
            }

            public override IQueryable<TEntity> Query(IQueryable<TEntity> queryable)
                => _queryFunc(queryable);

            public override IEnumerable<string> GetLoadedProperties()
                => _loadedPropertyNames ?? base.GetLoadedProperties();
        }
    }
}
