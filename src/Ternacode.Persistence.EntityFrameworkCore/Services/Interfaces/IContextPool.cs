using Microsoft.EntityFrameworkCore;

namespace Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces
{
    internal interface IContextPool<TContext> where TContext : DbContext
    {
        TContext Get();

        void Return(TContext context);
    }
}