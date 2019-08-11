using System.Collections.Generic;
using System.Linq;

namespace Ternacode.Persistence.Abstractions
{
    public interface IQuery<TEntity>
    {
        IQueryable<TEntity> Query(IQueryable<TEntity> queryable);

        IEnumerable<string> GetLoadedProperties();
    }
}
