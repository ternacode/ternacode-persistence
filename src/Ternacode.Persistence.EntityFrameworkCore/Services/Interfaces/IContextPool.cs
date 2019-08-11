namespace Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces
{
    internal interface IContextPool<TContext>
    {
        TContext Get();

        void Return(TContext context);
    }
}