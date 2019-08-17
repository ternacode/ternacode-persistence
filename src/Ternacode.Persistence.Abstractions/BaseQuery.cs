using System.Collections.Generic;
using System.Linq;

namespace Ternacode.Persistence.Abstractions
{
    public abstract class BaseQuery<TEntity> : IQuery<TEntity>
    {
        /// <summary>
        /// Returns the provided queryable, enumerating all TEntities in the database.
        /// </summary>
        public virtual IQueryable<TEntity> Query(IQueryable<TEntity> queryable)
            => queryable;

        /// <summary>
        /// Returns an empty list, meaning no related sub entities of TEntity will be eagerly loaded.
        /// </summary>
        public virtual IEnumerable<string> GetLoadedProperties()
            => Enumerable.Empty<string>();
    }
}
