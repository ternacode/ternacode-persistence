using System.Collections.Generic;

namespace Ternacode.Persistence.Example.API.Contracts.GetUsers
{
    public class GetUsersResponse
    {
        public IEnumerable<UserResponse> Users { get; set; }
    }
}