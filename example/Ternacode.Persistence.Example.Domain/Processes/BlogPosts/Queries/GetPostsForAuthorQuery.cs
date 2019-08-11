using System.Collections.Generic;
using System.Linq;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.Example.Domain.Model;

namespace Ternacode.Persistence.Example.Domain.Processes.BlogPosts.Queries
{
    public class GetPostsForAuthorQuery : BaseQuery<Post>
    {
        private readonly string _authorName;

        public GetPostsForAuthorQuery(string authorName)
        {
            _authorName = authorName;
        }

        public override IQueryable<Post> Query(IQueryable<Post> queryable)
        {
            return queryable.Where(p => p.Authors.Any(a => a.Author.Name == _authorName));
        }

        public override IEnumerable<string> GetLoadedProperties()
        {
            return new[] { nameof(Post.Authors) };
        }
    }
}