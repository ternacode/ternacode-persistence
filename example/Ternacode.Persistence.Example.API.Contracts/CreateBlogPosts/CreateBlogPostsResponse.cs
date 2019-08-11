using System;
using System.Collections.Generic;

namespace Ternacode.Persistence.Example.API.Contracts.CreateBlogPosts
{
    public class CreateBlogPostsResponse
    {
        public IEnumerable<Guid> PostIds { get; set; }
    }
}