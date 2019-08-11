using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.Example.Domain.Model;
using Ternacode.Persistence.Example.Domain.Processes.Users.Interfaces;
using Ternacode.Persistence.Example.Domain.Processes.Users.Queries;

namespace Ternacode.Persistence.Example.Domain.Processes.Users
{
    public class UsersProcess : IUsersProcess
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<User> _userRepository;

        public UsersProcess(
            IUnitOfWork unitOfWork,
            IRepository<User> userRepository)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
        }

        public async Task<User> CreateUserAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Empty user name");

            return await _unitOfWork.RunAsync(async () =>
            {
                if (_userRepository.Count(new GetUserWithNameQuery(name)) != 0)
                {
                    throw new ArgumentException($"A user with name '{name}' already exists.");
                }

                var user = new User
                {
                    Name = name,
                    AuthoredPosts = new List<PostAuthor>()
                };

                await _userRepository.AddAsync(user);

                return user;
            });
        }

        public IEnumerable<User> GetUsers()
            => _userRepository.Query(new GetUsersQuery());
    }
}