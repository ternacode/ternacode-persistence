using System.Collections.Generic;
using System.Linq;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Model;

namespace Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Queries
{
    public class GetAllFoosQuery : BaseQuery<Foo>
    {
        public override IQueryable<Foo> Query(IQueryable<Foo> queryable)
            => queryable;

        public override IEnumerable<string> GetLoadedProperties()
        {
            yield return nameof(Foo.Bar);
        }
    }
}