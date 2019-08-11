using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ternacode.Persistence.Abstractions
{
    public interface IRepository<TEntity>
    {
        void Add(TEntity entity);

        Task AddAsync(TEntity entity);

        TEntity Get(object id);

        TEntity Update(TEntity entity);

        void Delete(TEntity entity);

        IEnumerable<TEntity> Query(IQuery<TEntity> query);

        int Count(IQuery<TEntity> query);
    }
}
