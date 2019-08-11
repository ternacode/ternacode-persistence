using System.Collections.Generic;
using System.Linq;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Model;

namespace Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Queries
{
    public class GetFooWithIdQuery : BaseQuery<Foo>
    {
        private readonly long _id;

        public GetFooWithIdQuery(long id)
        {
            _id = id;
        }

        public override IQueryable<Foo> Query(IQueryable<Foo> queryable)
            => queryable.Where(f => f.FooId == _id);

        public override IEnumerable<string> GetLoadedProperties()
        {
            yield return nameof(Foo.Bar);
        }
    }
}