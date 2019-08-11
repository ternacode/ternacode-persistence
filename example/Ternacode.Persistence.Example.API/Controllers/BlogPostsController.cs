using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ternacode.Persistence.Example.API.Contracts.CreateBlogPosts;
using Ternacode.Persistence.Example.API.Contracts.GetBlogPosts;
using Ternacode.Persistence.Example.Domain.Processes.BlogPosts.Dtos;
using Ternacode.Persistence.Example.Domain.Processes.BlogPosts.Interfaces;

namespace Ternacode.Persistence.Example.API.Controllers
{
    [Route("api/posts")]
    public class BlogPostsController : Controller
    {
        private readonly IBlogPostsProcess _blogPostsProcess;

        public BlogPostsController(IBlogPostsProcess blogPostsProcess)
        {
            _blogPostsProcess = blogPostsProcess;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        public ActionResult<GetBlogPostsResponse> Get([FromQuery(Name = "author")] string authorName)
        {
            var result = _blogPostsProcess.GetPosts(authorName);

            return Ok(new GetBlogPostsResponse
            {
                Posts = result.Select(p => new BlogPostResponse
                {
                    PostId = p.PostId,
                    Title = p.Title
                })
            });
        }

        [HttpPost]
        [ProducesResponseType(200)]
        public async Task<ActionResult<CreateBlogPostsResponse>> PostAsync([FromBody] CreateBlogPostsRequest request)
        {
            var result = await _blogPostsProcess.CreatePostsAsync(request.Posts.Select(MapToDto));

            return Ok(new CreateBlogPostsResponse
            {
                PostIds = result.Select(p => p.PostId)
            });
        }

        private CreatePostDto MapToDto(CreateBlogPostRequest request)
            => new CreatePostDto
            {
                Authors = request.Authors,
                Title = request.Title
            };
    }
}