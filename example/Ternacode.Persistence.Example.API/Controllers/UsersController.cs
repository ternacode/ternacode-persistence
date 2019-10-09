using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ternacode.Persistence.Example.API.Contracts.CreateUser;
using Ternacode.Persistence.Example.API.Contracts.GetUsers;
using Ternacode.Persistence.Example.Domain.Processes.Users.Interfaces;

namespace Ternacode.Persistence.Example.API.Controllers
{
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersProcess _usersProcess;

        public UsersController(IUsersProcess usersProcess)
        {
            _usersProcess = usersProcess;
        }

        [HttpGet]
        public ActionResult<GetUsersResponse> Get()
        {
            var result = _usersProcess.GetUsers();

            return Ok(new GetUsersResponse
            {
                Users = result.Select(u => new UserResponse
                {
                    UserId = u.UserId,
                    Name = u.Name
                })
            });
        }

        [HttpPost]
        [ProducesResponseType(200)]
        public async Task<ActionResult<CreateUserResponse>> PostAsync([FromBody] CreateUserRequest request)
        {
            var result = await _usersProcess.CreateUserAsync(request.Name);

            return Ok(new CreateUserResponse
            {
                UserId = result.UserId
            });
        }
    }
}