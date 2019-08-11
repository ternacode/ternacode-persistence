using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Model;

namespace Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Factories
{
    public static class FooFactory
    {
        public static Foo Create(string fooName = "foo", string barName = "bar")
            => new Foo
            {
                Name = fooName,
                Bar = new Bar
                {
                    Name = barName
                }
            };
    }
}