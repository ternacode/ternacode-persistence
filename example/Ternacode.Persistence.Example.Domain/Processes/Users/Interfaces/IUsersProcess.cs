using System.Collections.Generic;
using System.Threading.Tasks;
using Ternacode.Persistence.Example.Domain.Model;

namespace Ternacode.Persistence.Example.Domain.Processes.Users.Interfaces
{
    public interface IUsersProcess
    {
        Task<User> CreateUserAsync(string name);

        IEnumerable<User> GetUsers();
    }
}