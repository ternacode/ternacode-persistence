using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.Example.Domain.Model;
using Ternacode.Persistence.Example.Domain.Processes.BlogPosts.Dtos;
using Ternacode.Persistence.Example.Domain.Processes.BlogPosts.Interfaces;
using Ternacode.Persistence.Example.Domain.Processes.BlogPosts.Queries;

namespace Ternacode.Persistence.Example.Domain.Processes.BlogPosts
{
    public class BlogPostsProcess : IBlogPostsProcess
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Post> _postRepository;
        private readonly IRepository<User> _userRepository;

        public BlogPostsProcess(
            IUnitOfWork unitOfWork,
            IRepository<Post> postRepository,
            IRepository<User> userRepository)
        {
            _unitOfWork = unitOfWork;
            _postRepository = postRepository;
            _userRepository = userRepository;
        }

        public IEnumerable<Post> GetPosts(string authorName)
        {
            if (string.IsNullOrWhiteSpace(authorName))
                throw new ArgumentException("Empty author name");

            if (_userRepository.Count(new GetAuthorQuery(authorName)) == 0)
                throw new ArgumentException("Author does not exist");

            return _postRepository.Query(new GetPostsForAuthorQuery(authorName));
        }

        public async Task<IEnumerable<Post>> CreatePostsAsync(IEnumerable<CreatePostDto> dtos)
        {
            if (dtos == null)
                throw new ArgumentNullException(nameof(dtos));

            if (!dtos.Any())
                throw new ArgumentException("No posts to create");

            return await _unitOfWork.RunAsync(async () =>
            {
                var posts = new List<Post>();

                foreach (var dto in dtos)
                {
                    var post = await CreatePostAsync(dto);
                    posts.Add(post);
                }

                return posts;
            });
        }

        private async Task<Post> CreatePostAsync(CreatePostDto dto)
        {
            ValidateDto(dto);

            var authors = _userRepository.Query(new GetAuthorsQuery(dto.Authors));
            var missingAuthors = dto.Authors
                .Where(authorName => authors.All(a => a.Name != authorName))
                .ToList();

            if (missingAuthors.Any())
            {
                throw new ArgumentException($"User(s) not found: {string.Join(",", missingAuthors)}");
            }

            var post = new Post
            {
                CreatedOn = DateTime.UtcNow,
                Title = dto.Title
            };

            var postAuthors = authors.Select(a => new PostAuthor
            {
                Post = post,
                Author = a
            }).ToList();

            post.Authors = postAuthors;

            await _postRepository.AddAsync(post);

            return post;
        }

        private void ValidateDto(CreatePostDto dto)
        {
            if (dto.Authors == null || !dto.Authors.Any())
                throw new ArgumentException("Post must have at least one author");

            if (dto.Authors.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("User name(s) must be non-empty");

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Post title must be non-empty");
        }
    }
}