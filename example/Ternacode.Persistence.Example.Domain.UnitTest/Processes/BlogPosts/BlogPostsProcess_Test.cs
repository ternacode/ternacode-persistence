using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.Example.Domain.Model;
using Ternacode.Persistence.Example.Domain.Processes.BlogPosts;
using Ternacode.Persistence.Example.Domain.Processes.BlogPosts.Dtos;
using Ternacode.Persistence.Example.Domain.UnitTest.Extensions;
using Ternacode.Persistence.Example.Domain.UnitTest.Factories;

// ReSharper disable PossibleMultipleEnumeration

namespace Ternacode.Persistence.Example.Domain.UnitTest.Processes.BlogPosts
{
    public class BlogPostsProcess_Test
    {
        private CustomAutoFixture _fixture { get; set; }

        [SetUp]
        public void SetUp()
        {
            _fixture = new CustomAutoFixture();
            _fixture.StubPersistence();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void When_getting_posts_with_empty_author_name_Then_an_exception_is_thrown(string authorName)
        {
            var sut = _fixture.Create<BlogPostsProcess>();

            Assert.That(() => sut.GetPosts(authorName), Throws.ArgumentException);
        }

        [Test]
        public void When_getting_posts_with_non_existing_author_Then_an_exception_is_thrown()
        {
            var sut = _fixture.Create<BlogPostsProcess>();

            Assert.That(() => sut.GetPosts(_fixture.Create<string>()), Throws.ArgumentException);
        }

        [Test]
        public void When_getting_posts_Then_the_post_repository_is_queried_once()
        {
            var authorName = _fixture.Create<string>();
            var repository = _fixture.Create<IRepository<Post>>();

            _fixture.Entities.Add(_fixture.CreateUser(authorName));

            var sut = _fixture.Create<BlogPostsProcess>();

            sut.GetPosts(authorName);

            repository.Received(1).Query(Arg.Any<IQuery<Post>>());
        }

        [Test]
        public void When_getting_posts_Then_the_expected_data_is_returned()
        {
            var authorName = _fixture.Create<string>();
            var otherAuthorName = _fixture.Create<string>();

            var user = _fixture.CreateUser(authorName);
            var otherUser = _fixture.CreateUser(otherAuthorName);

            var p1 = _fixture.CreatePost(otherUser);
            var p2 = _fixture.CreatePost(user);
            var p3 = _fixture.CreatePost(otherUser, user);

            _fixture.AddEntities(new object[] { user, otherUser, p1, p2, p3 });

            var sut = _fixture.Create<BlogPostsProcess>();

            var result = sut.GetPosts(authorName).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result.Count, Is.EqualTo(2));
                Assert.That(result.Contains(p2));
                Assert.That(result.Contains(p3));
            });
        }

        [Test]
        public void When_creating_posts_async_with_null_dtos_Then_an_exception_is_thrown()
        {
            var sut = _fixture.Create<BlogPostsProcess>();

            Assert.That(() => sut.CreatePostsAsync(null), Throws.ArgumentNullException);
        }

        [Test]
        public void When_creating_posts_async_with_empty_dtos_Then_an_exception_is_thrown()
        {
            var sut = _fixture.Create<BlogPostsProcess>();

            Assert.That(() => sut.CreatePostsAsync(Enumerable.Empty<CreatePostDto>()), Throws.ArgumentException);
        }

        [Test]
        public async Task When_creating_posts_async_Then_unit_of_work_run_async_is_called_once()
        {
            var unitOfWork = _fixture.Create<IUnitOfWork>();

            var dtos = _fixture.CreateMany<CreatePostDto>();

            var users = dtos.SelectMany(d => d.Authors)
                .Select(a => _fixture.CreateUser(a));

            _fixture.AddEntities(users);

            var sut = _fixture.Create<BlogPostsProcess>();

            await sut.CreatePostsAsync(dtos);

            await unitOfWork.ReceivedWithAnyArgs(1).RunAsync(default);
        }

        [Test]
        public async Task When_creating_posts_async_Then_the_expected_posts_are_returned()
        {
            var dtos = _fixture.CreateMany<CreatePostDto>();

            var users = dtos.SelectMany(d => d.Authors)
                .Select(a => _fixture.CreateUser(a));

            _fixture.AddEntities(users);

            var sut = _fixture.Create<BlogPostsProcess>();

            var posts = await sut.CreatePostsAsync(dtos);

            Assert.Multiple(() =>
            {
                Assert.That(posts, Is.Not.Null, "Result is null");
                Assert.That(posts.Count(), Is.EqualTo(dtos.Count()), "Invalid post count returned");

                foreach (var post in posts)
                {
                    var dto = dtos.SingleOrDefault(d => d.Title == post.Title);
                    Assert.That(dto, Is.Not.Null, "No matching dto found for returned post");

                    var postAuthors = post.Authors.Select(a => a.Author.Name);
                    CollectionAssert.AreEquivalent(postAuthors, dto.Authors, "Author lists not equivalent");
                }
            });
        }
    }
}