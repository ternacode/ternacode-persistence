using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using Ternacode.Persistence.Example.API.Contracts.CreateUser;
using Ternacode.Persistence.Example.API.Contracts.GetUsers;
using Ternacode.Persistence.Example.API.Controllers;
using Ternacode.Persistence.Example.API.UnitTest.Extensions;
using Ternacode.Persistence.Example.Domain.Model;
using Ternacode.Persistence.Example.Domain.Processes.Users.Interfaces;

// ReSharper disable PossibleMultipleEnumeration

namespace Ternacode.Persistence.Example.API.UnitTest.Controllers
{
    [TestFixture]
    public class UsersController_Test
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoNSubstituteCustomization());

            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Test]
        public void When_getting_users_Then_the_expected_object_result_is_returned()
        {
            var users = _fixture.CreateMany<User>();
            var expectedUsers = users.Select(u => (u.UserId, u.Name));

            var process = _fixture.Freeze<IUsersProcess>();
            process.GetUsers().Returns(users);

            using (var sut = _fixture.CreateController<UsersController>())
            {
                var actionResult = sut.Get();
                var okObjectResult = actionResult?.Result as OkObjectResult;
                var response = okObjectResult?.Value as GetUsersResponse;

                var actualUsers = response?.Users?.Select(u => (u.UserId, u.Name));

                Assert.Multiple(() =>
                {
                    Assert.That(okObjectResult, Is.Not.Null, "OkObjectResult result is null");
                    Assert.That(response, Is.Not.Null, "Response is null");
                    CollectionAssert.AreEquivalent(expectedUsers, actualUsers, "Invalid user data returned");
                });
            }
        }

        [Test]
        public void When_getting_users_Then_the_process_get_users_is_called_once()
        {
            var process = _fixture.Freeze<IUsersProcess>();
            
            using (var sut = _fixture.CreateController<UsersController>())
            {
                sut.Get();

                process.Received(1).GetUsers();
            }
        }

        [Test]
        public async Task When_creating_user_Then_the_expected_object_result_is_returned()
        {
            var user = _fixture.Create<User>();

            var process = _fixture.Freeze<IUsersProcess>();
            process.CreateUserAsync(Arg.Any<string>()).Returns(user);

            using (var sut = _fixture.CreateController<UsersController>())
            {
                var request = _fixture.Create<CreateUserRequest>();
                request.Name = user.Name;

                var actionResult = await sut.PostAsync(request);
                var okObjectResult = actionResult?.Result as OkObjectResult;
                var response = okObjectResult?.Value as CreateUserResponse;

                Assert.Multiple(() =>
                {
                    Assert.That(okObjectResult, Is.Not.Null, "OkObjectResult result is null");
                    Assert.That(response, Is.Not.Null, "Response is null");
                    Assert.That(response.UserId, Is.EqualTo(user.UserId), "Invalid user id returned");
                });
            }
        }

        [Test]
        public async Task When_creating_user_Then_the_process_create_user_async_is_called_once_with_expected_name()
        {
            var process = _fixture.Freeze<IUsersProcess>();
            process.CreateUserAsync(Arg.Any<string>())
                .Returns(_fixture.Create<User>());

            using (var sut = _fixture.CreateController<UsersController>())
            {
                var request = _fixture.Create<CreateUserRequest>();
                await sut.PostAsync(request);

                await process.Received(1).CreateUserAsync(Arg.Any<string>());
                await process.Received().CreateUserAsync(Arg.Is(request.Name));
            }
        }
    }
}