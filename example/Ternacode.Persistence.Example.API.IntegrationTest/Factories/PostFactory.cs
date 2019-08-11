using System;
using System.Collections.Generic;
using System.Linq;
using Ternacode.Persistence.Example.Domain.Model;

namespace Ternacode.Persistence.Example.API.IntegrationTest.Factories
{
    public static class PostFactory
    {
        public static Post CreatePost(string title, IEnumerable<User> authors)
        {
            var post = new Post
            {
                CreatedOn = DateTimeOffset.UtcNow,
                Title = title
            };

            post.Authors = authors.Select(a => new PostAuthor
            {
                Post = post,
                Author = a
            }).ToList();

            return post;
        }
    }
}