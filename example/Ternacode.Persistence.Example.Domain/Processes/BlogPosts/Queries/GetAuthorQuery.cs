using System.Linq;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.Example.Domain.Model;

namespace Ternacode.Persistence.Example.Domain.Processes.BlogPosts.Queries
{
    public class GetAuthorQuery : BaseQuery<User>
    {
        private readonly string _authorName;

        public GetAuthorQuery(string authorName)
        {
            _authorName = authorName;
        }

        public override IQueryable<User> Query(IQueryable<User> queryable)
        {
            return queryable.Where(a => _authorName == a.Name);
        }
    }
}