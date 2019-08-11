using Ternacode.Persistence.Example.Domain.Model;

namespace Ternacode.Persistence.Example.API.IntegrationTest.Factories
{
    public static class UserFactory
    {
        public static User CreateUser(string name)
            => new User
               {
                   Name = name
               };
    }
}