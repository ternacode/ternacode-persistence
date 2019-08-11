using System;

namespace Ternacode.Persistence.Example.Domain.Model
{
    public class PostAuthor
    {
        public Guid PostAuthorId { get; set; }

        public Post Post { get; set; }

        public User Author { get; set; }
    }
}