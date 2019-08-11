using System.Collections.Generic;

namespace Ternacode.Persistence.Example.API.Contracts.GetBlogPosts
{
    public class GetBlogPostsResponse
    {
        public IEnumerable<BlogPostResponse> Posts { get; set; }
    }
}