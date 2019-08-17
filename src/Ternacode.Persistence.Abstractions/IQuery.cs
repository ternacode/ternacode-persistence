using System.Collections.Generic;
using System.Linq;

namespace Ternacode.Persistence.Abstractions
{
    public interface IQuery<TEntity>
    {
        /// <summary>
        /// Gets called by the repository implementation when querying the database.
        /// </summary>
        IQueryable<TEntity> Query(IQueryable<TEntity> queryable);

        /// <summary>
        /// Returns the property names of sub entities referenced by TEntity to be eagerly loaded.
        /// </summary>
        IEnumerable<string> GetLoadedProperties();
    }
}
