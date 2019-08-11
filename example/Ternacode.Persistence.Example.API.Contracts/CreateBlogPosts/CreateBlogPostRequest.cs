using System.Collections.Generic;

namespace Ternacode.Persistence.Example.API.Contracts.CreateBlogPosts
{
    public class CreateBlogPostRequest
    {
        public IEnumerable<string> Authors { get; set; }

        public string Title { get; set; }
    }
}