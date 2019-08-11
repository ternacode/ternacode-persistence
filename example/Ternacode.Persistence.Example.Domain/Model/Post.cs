using System;
using System.Collections.Generic;

namespace Ternacode.Persistence.Example.Domain.Model
{
    public class Post
    {
        public Guid PostId { get; set; }

        public string Title { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public IEnumerable<PostAuthor> Authors { get; set; }
    }
}