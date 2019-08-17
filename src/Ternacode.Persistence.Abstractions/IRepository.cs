using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ternacode.Persistence.Abstractions
{
    public interface IRepository<TEntity>
    {
        /// <summary>
        /// Adds an entity.
        /// </summary>
        void Add(TEntity entity);

        /// <summary>
        /// Adds an entity.
        /// </summary>
        Task AddAsync(TEntity entity);

        /// <summary>
        /// Returns a single entity, querying by provided id object.
        /// If no entity is found, null is returned.
        /// </summary>
        TEntity Get(object id);

        /// <summary>
        /// Updates an entity and returns an updated object.
        /// </summary>
        TEntity Update(TEntity entity);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        void Delete(TEntity entity);

        /// <summary>
        /// Queries the database with provided IQuery object, returning all matches.
        /// If no matches, an empty enumerable is returned.
        /// </summary>
        IEnumerable<TEntity> Query(IQuery<TEntity> query);


        /// <summary>
        /// Gets the database TEntity count using the query of provided IQuery object.
        /// </summary>
        int Count(IQuery<TEntity> query);
    }
}
