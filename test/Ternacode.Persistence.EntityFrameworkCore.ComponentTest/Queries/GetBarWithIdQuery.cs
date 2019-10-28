using System;
using System.Linq;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Model;

namespace Ternacode.Persistence.EntityFrameworkCore.ComponentTest.Queries
{
    public class GetBarWithIdQuery : BaseQuery<Bar>
    {
        private readonly Guid _id;

        public GetBarWithIdQuery(Guid id)
        {
            _id = id;
        }

        public override IQueryable<Bar> Query(IQueryable<Bar> queryable)
            => queryable.Where(b => b.BarId == _id);
    }
}