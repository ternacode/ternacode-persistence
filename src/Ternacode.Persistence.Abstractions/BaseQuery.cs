using System.Collections.Generic;
using System.Linq;

namespace Ternacode.Persistence.Abstractions
{
    public abstract class BaseQuery<TEntity> : IQuery<TEntity>
    {
        public virtual IQueryable<TEntity> Query(IQueryable<TEntity> queryable)
            => queryable;

        public virtual IEnumerable<string> GetLoadedProperties()
            => Enumerable.Empty<string>();
    }
}
