using System.Linq;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.Example.Domain.Model;

namespace Ternacode.Persistence.Example.Domain.Processes.Users.Queries
{
    public class GetUsersQuery : BaseQuery<User>
    {
        public override IQueryable<User> Query(IQueryable<User> queryable)
            => queryable;
    }
}