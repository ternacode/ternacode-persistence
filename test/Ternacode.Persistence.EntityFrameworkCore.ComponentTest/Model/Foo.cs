using System;

namespace Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Model
{
    public class Foo
    {
        public long FooId { get; set; }

        public string Name { get; set; }

        public Guid? BarId { get; set; }

        public Bar Bar { get; set; }
    }
}
