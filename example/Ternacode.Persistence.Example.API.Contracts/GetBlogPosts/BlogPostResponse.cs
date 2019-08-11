using System;

namespace Ternacode.Persistence.Example.API.Contracts.GetBlogPosts
{
    public class BlogPostResponse
    {
        public Guid PostId { get; set; }

        public string Title { get; set; }
    }
}