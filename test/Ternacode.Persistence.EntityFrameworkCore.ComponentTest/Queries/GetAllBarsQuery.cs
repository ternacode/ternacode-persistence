using System.Linq;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Model;

namespace Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Queries
{
    public class GetAllBarsQuery : BaseQuery<Bar>
    {
        public override IQueryable<Bar> Query(IQueryable<Bar> queryable)
            => queryable;
    }
}