﻿using System.Collections.Generic;
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
        /// Returns a single entity, querying by provided id object.
        /// If no entity is found, null is returned.
        /// </summary>
        Task<TEntity> GetAsync(object id);

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
        /// Queries the database with provided IQuery object, returning all matches.
        /// If no matches, an empty enumerable is returned.
        /// </summary>
        Task<IEnumerable<TEntity>> QueryAsync(IQuery<TEntity> query);

        /// <summary>
        /// Gets the database entity count using the query of provided query.
        /// </summary>
        int Count(IQuery<TEntity> query);


        /// <summary>
        /// Returns whether there is at least one entity matching the provided query.
        /// </summary>
        bool Any(IQuery<TEntity> query);
    }
}
