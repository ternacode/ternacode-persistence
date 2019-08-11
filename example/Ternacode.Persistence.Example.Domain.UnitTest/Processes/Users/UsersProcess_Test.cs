using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.Example.Domain.Model;
using Ternacode.Persistence.Example.Domain.Processes.Users;
using Ternacode.Persistence.Example.Domain.UnitTest.Extensions;

// ReSharper disable PossibleMultipleEnumeration

namespace Ternacode.Persistence.Example.Domain.UnitTest.Processes.Users
{
    [TestFixture]
    public class UsersProcess_Test
    {
        private CustomAutoFixture _fixture { get; set; }

        [SetUp]
        public void SetUp()
        {
            _fixture = new CustomAutoFixture();
            _fixture.StubPersistence();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void When_creating_user_async_with_empty_name_Then_an_exception_is_thrown(string name)
        {
            var sut = _fixture.Create<UsersProcess>();

            Assert.That(() => sut.CreateUserAsync(name), Throws.ArgumentException);
        }

        [Test]
        public void When_creating_user_async_for_an_existing_name_Then_an_exception_is_thrown()
        {
            var name = _fixture.Create<string>();

            _fixture.Entities.Add(new User { Name = name });

            var sut = _fixture.Create<UsersProcess>();

            Assert.That(() => sut.CreateUserAsync(name), Throws.ArgumentException);
        }

        [Test]
        public async Task When_creating_user_async_Then_an_entity_with_expected_name_is_added_async_to_the_repository()
        {
            var name = _fixture.Create<string>();
            var repository = _fixture.Create<IRepository<User>>();

            var sut = _fixture.Create<UsersProcess>();

            var result = await sut.CreateUserAsync(name);

            Assert.Multiple(() =>
            {
                Assert.That(_fixture.Entities.OfType<User>().Count(), Is.EqualTo(1), "Invalid entity count");
                Assert.That(result, Is.Not.Null, "Result is null");
                Assert.That(result.Name, Is.EqualTo(name), "Invalid entity name");

                repository.Received(1).AddAsync(Arg.Is<User>(u => u.Name == name));
            });
        }

        [Test]
        public void When_getting_users_Then_the_expected_users_are_returned()
        {
            var users = _fixture.CreateMany<User>();
            _fixture.AddEntities(users);

            var sut = _fixture.Create<UsersProcess>();

            var result = sut.GetUsers();

            var expectedUsers = users.Select(u => (u.UserId, u.Name));
            var actualUsers = result.Select(u => (u.UserId, u.Name));

            CollectionAssert.AreEquivalent(expectedUsers, actualUsers);
        }

        [Test]
        public void When_getting_users_and_none_exist_Then_an_empty_list_is_returned()
        {
            var sut = _fixture.Create<UsersProcess>();

            var result = sut.GetUsers();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null, "Result is null");
                Assert.That(result, Is.Empty, "Result is non-empty");
            });
        }
    }
}