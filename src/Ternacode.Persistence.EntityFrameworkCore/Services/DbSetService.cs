using System;
using Microsoft.EntityFrameworkCore;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.Services
{
    internal class DbSetService<TContext, TEntity> : IDbSetService<TContext, TEntity>
        where TContext : DbContext
        where TEntity : class
    {
        private readonly Func<TContext, DbSet<TEntity>> _dbSetGetter;

        public DbSetService(Func<TContext, DbSet<TEntity>> dbSetGetter)
            => _dbSetGetter = dbSetGetter;

        public DbSet<TEntity> GetDbSet(TContext context)
            => _dbSetGetter(context);
    }
}