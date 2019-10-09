using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using Ternacode.Persistence.Example.API.Contracts.CreateBlogPosts;
using Ternacode.Persistence.Example.API.Contracts.GetBlogPosts;
using Ternacode.Persistence.Example.API.Controllers;
using Ternacode.Persistence.Example.API.UnitTest.Extensions;
using Ternacode.Persistence.Example.Domain.Model;
using Ternacode.Persistence.Example.Domain.Processes.BlogPosts.Dtos;
using Ternacode.Persistence.Example.Domain.Processes.BlogPosts.Interfaces;

// ReSharper disable PossibleMultipleEnumeration

namespace Ternacode.Persistence.Example.API.UnitTest.Controllers
{
    [TestFixture]
    public class BlogPostsController_Test
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoNSubstituteCustomization());

            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                    .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Test]
        public void When_getting_blog_posts_Then_the_expected_object_result_is_returned()
        {
            var authorName = _fixture.Create<string>();
            var posts = _fixture.CreateMany<Post>();
            var expectedPosts = posts.Select(p => (p.PostId, p.Title));

            var process = _fixture.Freeze<IBlogPostsProcess>();
            process.GetPosts(Arg.Is(authorName)).Returns(posts);

            var sut = _fixture.CreateController<BlogPostsController>();
            
            var actionResult = sut.Get(authorName);
            var okObjectResult = actionResult?.Result as OkObjectResult;
            var response = okObjectResult?.Value as GetBlogPostsResponse;

            var actualPosts = response?.Posts?.Select(p => (p.PostId, p.Title));

            Assert.Multiple(() =>
            {
                Assert.That(okObjectResult, Is.Not.Null, "OkObjectResult is null");
                Assert.That(response, Is.Not.Null, "Response is null");
                CollectionAssert.AreEquivalent(expectedPosts, actualPosts, "Invalid posts returned");
            });
        }

        [Test]
        public void When_getting_blog_posts_Then_the_process_get_posts_is_called_once_with_expected_name()
        {
            var process = _fixture.Freeze<IBlogPostsProcess>();

            var sut = _fixture.CreateController<BlogPostsController>();
            
            var authorName = _fixture.Create<string>();
            sut.Get(authorName);

            process.Received(1).GetPosts(Arg.Any<string>());
            process.Received().GetPosts(Arg.Is(authorName));
        }

        [Test]
        public async Task When_creating_blog_posts_Then_the_expected_object_result_is_returned()
        {
            var posts = _fixture.CreateMany<Post>();
            var expectedIds = posts.Select(p => p.PostId);

            var process = _fixture.Freeze<IBlogPostsProcess>();
            process.CreatePostsAsync(Arg.Any<IEnumerable<CreatePostDto>>())
                .Returns(posts);

            var sut = _fixture.CreateController<BlogPostsController>();
            var request = _fixture.Create<CreateBlogPostsRequest>();

            var actionResult = await sut.PostAsync(request);
            var okObjectResult = actionResult?.Result as OkObjectResult;
            var response = okObjectResult?.Value as CreateBlogPostsResponse;

            var actualIds = response?.PostIds;

            Assert.Multiple(() =>
            {
                Assert.That(okObjectResult, Is.Not.Null, "OkObjectResult is null");
                Assert.That(response, Is.Not.Null, "Response is null");
                CollectionAssert.AreEquivalent(expectedIds, actualIds, "Invalid ids returned");
            });
        }

        [Test]
        public async Task When_creating_blog_posts_Then_the_process_create_posts_async_is_called_once()
        {
            var process = _fixture.Freeze<IBlogPostsProcess>();

            var sut = _fixture.CreateController<BlogPostsController>();
            
            await sut.PostAsync(_fixture.Create<CreateBlogPostsRequest>());

            await process.Received(1).CreatePostsAsync(Arg.Any<IEnumerable<CreatePostDto>>());
        }
    }
}