using System.Collections.Generic;

namespace Ternacode.Persistence.Example.API.Contracts.CreateBlogPosts
{
    public class CreateBlogPostsRequest
    {
        public IEnumerable<CreateBlogPostRequest> Posts { get; set; }
    }
}