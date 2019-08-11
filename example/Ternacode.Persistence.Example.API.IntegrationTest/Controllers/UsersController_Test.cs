using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Ternacode.Persistence.Example.API.Contracts.CreateUser;
using Ternacode.Persistence.Example.API.Contracts.GetUsers;
using Ternacode.Persistence.Example.API.IntegrationTest.Extensions;
using Ternacode.Persistence.Example.API.IntegrationTest.Factories;

// ReSharper disable PossibleMultipleEnumeration

namespace Ternacode.Persistence.Example.API.IntegrationTest.Controllers
{
    public class UsersController_Test : ExampleContextIntegrationTest
    {
        private const string BASE_ROUTE = "api/users";

        private HttpClient _client;

        [SetUp]
        public void SetUp()
        {
            _client = CreateClient();
        }

        [Test]
        public async Task When_GET_users_Then_200_OK_with_expected_users_are_returned()
        {
            var user1 = UserFactory.CreateUser("User 1");
            var user2 = UserFactory.CreateUser("User 2");

            using (var context = CreateContext())
            {
                context.Users.Add(user1);
                context.Users.Add(user2);
                await context.SaveChangesAsync();
            }

            var expectedUsers = new[]
            {
                (user1.UserId, user1.Name),
                (user2.UserId, user2.Name)
            };

            var (getUsersResponse, httpResponse) = await _client.GetWithResponseAsync<GetUsersResponse>(BASE_ROUTE);

            var actualUsers = getUsersResponse?.Users?.Select(u => (u.UserId, u.Name));

            Assert.Multiple(() =>
            {
                Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Invalid status code");
                Assert.That(getUsersResponse?.Users, Is.Not.Null, "Null users returned");

                CollectionAssert.AreEquivalent(expectedUsers, actualUsers);
            });
        }

        [Test]
        public async Task When_POST_user_Then_200_OK_with_a_non_default_user_id_is_returned()
        {
            var request = new CreateUserRequest
            {
                Name = "User 1"
            };

            var (createUserResponse, httpResponse) = await _client.PostWithResponseAsync<CreateUserResponse>(BASE_ROUTE, request);

            Assert.Multiple(() =>
            {
                Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Invalid status code");
                Assert.That(createUserResponse?.UserId, Is.Not.EqualTo(default), "Invalid user id");
            });
        }

        [Test]
        public async Task When_creating_user_and_getting_all_users_Then_the_expected_user_is_created_and_returned()
        {
            var request = new CreateUserRequest
            {
                Name = "User 1"
            };

            var (createUserResponse, _) = await _client.PostWithResponseAsync<CreateUserResponse>(BASE_ROUTE, request);
            var (getUsersResponse, _) = await _client.GetWithResponseAsync<GetUsersResponse>(BASE_ROUTE);

            var actualUsers = getUsersResponse?.Users;

            Assert.Multiple(() =>
            {
                Assert.That(actualUsers, Is.Not.Null, "Users is null");
                Assert.That(actualUsers.Count(), Is.EqualTo(1), "Invalid user count");
                Assert.That(actualUsers.Single().UserId, Is.EqualTo(createUserResponse?.UserId), "Invalid user id");
                Assert.That(actualUsers.Single().Name, Is.EqualTo(request.Name), "Invalid user name");
            });
        }
    }
}