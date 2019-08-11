using Microsoft.EntityFrameworkCore;

namespace Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces
{
    internal interface IContextService<TContext> where TContext : DbContext
    {
        TContext GetCurrentContext();

        bool HasCurrentContext();

        TContext InitContext();

        void ClearCurrentContext();
    }
}