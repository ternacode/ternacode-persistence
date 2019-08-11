using System;
using System.Linq;
using AutoFixture;
using Ternacode.Persistence.Example.Domain.Model;

namespace Ternacode.Persistence.Example.Domain.UnitTest.Factories
{
    public static class PostFactory
    {
        public static Post CreatePost(this Fixture fixture, params User[] authors)
        {
            var post = new Post
            {
                Title = fixture.Create<string>(),
                CreatedOn = fixture.Create<DateTimeOffset>(),
                PostId = fixture.Create<Guid>()
            };

            post.Authors = authors.Select(a => new PostAuthor
            {
                Post = post,
                Author = a,
                PostAuthorId = fixture.Create<Guid>()
            }).ToList();

            return post;
        }
    }
}