using System.Linq;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.Example.Domain.Model;

namespace Ternacode.Persistence.Example.Domain.Processes.Users.Queries
{
    public class GetUserWithNameQuery : BaseQuery<User>
    {
        private readonly string _name;

        public GetUserWithNameQuery(string name)
        {
            _name = name;
        }

        public override IQueryable<User> Query(IQueryable<User> queryable)
        {
            return queryable.Where(u => u.Name == _name);
        }
    }
}