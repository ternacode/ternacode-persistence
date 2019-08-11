using System.Collections.Generic;

namespace Ternacode.Persistence.Example.Domain.Processes.BlogPosts.Dtos
{
    public class CreatePostDto
    {
        public IEnumerable<string> Authors { get; set; }

        public string Title { get; set; }
    }
}