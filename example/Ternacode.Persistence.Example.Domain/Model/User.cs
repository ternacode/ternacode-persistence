using System.Collections.Generic;

namespace Ternacode.Persistence.Example.Domain.Model
{
    public class User
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public IEnumerable<PostAuthor> AuthoredPosts { get; set; }
    }
}