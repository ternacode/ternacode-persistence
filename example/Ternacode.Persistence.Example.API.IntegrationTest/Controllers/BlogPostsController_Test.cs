using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Ternacode.Persistence.Example.API.Contracts.CreateBlogPosts;
using Ternacode.Persistence.Example.API.Contracts.GetBlogPosts;
using Ternacode.Persistence.Example.API.IntegrationTest.Extensions;
using Ternacode.Persistence.Example.API.IntegrationTest.Factories;

namespace Ternacode.Persistence.Example.API.IntegrationTest.Controllers
{
    public class BlogPostsController_Test : ExampleContextIntegrationTest
    {
        private const string BASE_ROUTE = "api/posts";

        private HttpClient _client;

        [SetUp]
        public void SetUp()
        {
            _client = CreateClient();
        }

        [Test]
        public async Task When_GET_blog_posts_for_author_name_Then_200_OK_with_expected_posts_are_returned()
        {
            var user1 = UserFactory.CreateUser("User 1");
            var user2 = UserFactory.CreateUser("User 2");
            var user3 = UserFactory.CreateUser("User 3");

            var post1 = PostFactory.CreatePost("Post 1", new[] { user2, user3 });
            var post2 = PostFactory.CreatePost("Post 2", new[] { user1, user2 });
            var post3 = PostFactory.CreatePost("Post 3", new[] { user1, user3 });

            using (var context = CreateContext())
            {
                context.Users.Add(user1);
                context.Users.Add(user2);
                context.Users.Add(user3);

                context.Posts.Add(post1);
                context.Posts.Add(post2);
                context.Posts.Add(post3);

                await context.SaveChangesAsync();
            }

            var (getBlogPostsResponse, httpResponse) = await _client.GetWithResponseAsync<GetBlogPostsResponse>($"{BASE_ROUTE}?author={user1.Name}");

            Assert.Multiple(() =>
            {
                Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Invalid status code");
                Assert.That(getBlogPostsResponse, Is.Not.Null, "Response is null");
                Assert.That(getBlogPostsResponse.Posts, Is.Not.Null, "Post result is null");
                Assert.That(getBlogPostsResponse.Posts.Count(), Is.EqualTo(2), "Invalid post count");

                Assert.That(getBlogPostsResponse.Posts.Any(p => (p.PostId, p.Title) == (post2.PostId, post2.Title)), "Expected post not returned");
                Assert.That(getBlogPostsResponse.Posts.Any(p => (p.PostId, p.Title) == (post3.PostId, post3.Title)), "Expected post not returned");
            });
        }

        [Test]
        public async Task When_POST_blog_post_Then_200_OK_with_non_default_id_is_returned()
        {
            var user = UserFactory.CreateUser("User 1");

            using (var context = CreateContext())
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            var createRequest = new CreateBlogPostRequest
            {
                Authors = new[] { user.Name },
                Title = "Post 1"
            };

            var request = new CreateBlogPostsRequest
            {
                Posts = new[] { createRequest }
            };

            var (createBlogPostsResponse, httpResponse) = await _client.PostWithResponseAsync<CreateBlogPostsResponse>($"{BASE_ROUTE}", request);

            Assert.Multiple(() =>
            {
                Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Invalid status code");
                Assert.That(createBlogPostsResponse, Is.Not.Null, "Response is null");
                Assert.That(createBlogPostsResponse.PostIds, Is.Not.Null, "Post ids result is null");
                Assert.That(createBlogPostsResponse.PostIds.Count(), Is.EqualTo(1), "Invalid post ids count");
                Assert.That(createBlogPostsResponse.PostIds.Single(), Is.Not.EqualTo(default), "Post id has default value");
            });
        }

        [Test]
        public async Task When_creating_blog_post_and_getting_it_Then_the_expected_post_is_created_and_returned()
        {
            var user = UserFactory.CreateUser("User 1");

            using (var context = CreateContext())
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            var createRequest = new CreateBlogPostRequest
            {
                Authors = new [] { user.Name },
                Title = "Post 1"
            };

            var request = new CreateBlogPostsRequest
            {
                Posts = new[] { createRequest }
            };

            var (createBlogPostsResponse, _) = await _client.PostWithResponseAsync<CreateBlogPostsResponse>($"{BASE_ROUTE}", request);
            var (getBlogPostsResponse, _) = await _client.GetWithResponseAsync<GetBlogPostsResponse>($"{BASE_ROUTE}?author={user.Name}");

            Assert.Multiple(() =>
            {
                Assert.That(createBlogPostsResponse.PostIds?.Count(), Is.EqualTo(1), "Invalid blog post ids returned");
                Assert.That(getBlogPostsResponse.Posts?.Count(), Is.EqualTo(1), "Invalid blog posts returned");
                
                Assert.That(createBlogPostsResponse.PostIds?.Single(), Is.EqualTo(getBlogPostsResponse.Posts?.Single().PostId), "Invalid post id");
                Assert.That(getBlogPostsResponse.Posts?.Single().Title, Is.EqualTo(createRequest.Title), "Invalid post title");
            });
        }
    }
}