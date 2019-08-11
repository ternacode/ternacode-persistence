using System.Collections.Generic;
using System.Linq;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.Example.Domain.Model;

namespace Ternacode.Persistence.Example.Domain.Processes.BlogPosts.Queries
{
    public class GetAuthorsQuery : BaseQuery<User>
    {
        private readonly string[] _authorNames;

        public GetAuthorsQuery(IEnumerable<string> authorNames)
        {
            _authorNames = authorNames.ToArray();
        }

        public override IQueryable<User> Query(IQueryable<User> queryable)
        {
            return queryable.Where(a => _authorNames.Contains(a.Name));
        }
    }
}