using AutoFixture;
using Ternacode.Persistence.Example.Domain.Model;

namespace Ternacode.Persistence.Example.Domain.UnitTest.Factories
{
    public static class UserFactory
    {
        public static User CreateUser(this Fixture fixture, string name)
            => new User
            {
                Name = name,
                UserId = fixture.Create<int>()
            };
    }
}