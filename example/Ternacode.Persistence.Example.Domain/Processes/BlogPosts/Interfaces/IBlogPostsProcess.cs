using System.Collections.Generic;
using System.Threading.Tasks;
using Ternacode.Persistence.Example.Domain.Model;
using Ternacode.Persistence.Example.Domain.Processes.BlogPosts.Dtos;

namespace Ternacode.Persistence.Example.Domain.Processes.BlogPosts.Interfaces
{
    public interface IBlogPostsProcess
    {
        IEnumerable<Post> GetPosts(string authorName);

        Task<IEnumerable<Post>> CreatePostsAsync(IEnumerable<CreatePostDto> dtos);
    }
}