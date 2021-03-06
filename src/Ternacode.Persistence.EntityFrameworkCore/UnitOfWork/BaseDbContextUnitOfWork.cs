﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ternacode.Persistence.Abstractions;
using Ternacode.Persistence.EntityFrameworkCore.Exceptions;
using Ternacode.Persistence.EntityFrameworkCore.Services.Interfaces;

namespace Ternacode.Persistence.EntityFrameworkCore.UnitOfWork
{
    internal abstract class BaseDbContextUnitOfWork<TContext, TTransaction> : IUnitOfWork
        where TContext : DbContext
        where TTransaction : IDisposable
    {
        protected readonly IContextService<TContext> _contextService;
        private readonly IContextFlushService<TContext> _contextFlushService;

        protected BaseDbContextUnitOfWork(
            IContextService<TContext> contextService,
            IContextFlushService<TContext> contextFlushService)
        {
            _contextService = contextService;
            _contextFlushService = contextFlushService;
        }

        protected abstract TTransaction BeginTransaction();

        protected abstract void CommitTransaction(TTransaction transaction);

        public virtual void Run(Action action)
        {
            EnsureNoContext();

            try
            {
                BeginContext();
                using (var transaction = BeginTransaction())
                {
                    action();

                    _contextFlushService.FlushChanges(_contextService.GetCurrentContext());
                    CommitTransaction(transaction);
                }
            }
            finally
            {
                EndContext();
            }
        }

        public virtual T Run<T>(Func<T> func)
        {
            EnsureNoContext();

            try
            {
                BeginContext();
                using (var transaction = BeginTransaction())
                {
                    var result = func();
                    if (result is Task task)
                    {
                        task.Wait();
                    }

                    _contextFlushService.FlushChanges(_contextService.GetCurrentContext());
                    CommitTransaction(transaction);

                    return result;
                }
            }
            finally
            {
                EndContext();
            }
        }

        public virtual async Task RunAsync(Func<Task> funcAsync)
        {
            EnsureNoContext();

            try
            {
                BeginContext();
                using (var transaction = BeginTransaction())
                {
                    await funcAsync();

                    await _contextFlushService.FlushChangesAsync(_contextService.GetCurrentContext());
                    CommitTransaction(transaction);
                }
            }
            finally
            {
                EndContext();
            }
        }

        public virtual async Task<T> RunAsync<T>(Func<Task<T>> funcAsync)
        {
            EnsureNoContext();

            try
            {
                BeginContext();
                using (var transaction = BeginTransaction())
                {
                    var result = await funcAsync();

                    await _contextFlushService.FlushChangesAsync(_contextService.GetCurrentContext());
                    CommitTransaction(transaction);

                    return result;
                }
            }
            finally
            {
                EndContext();
            }
        }

        protected void EnsureNoContext()
        {
            if (_contextService.HasCurrentContext())
                throw new CurrentContextAlreadySetException();
        }

        protected void BeginContext()
            => _contextService.InitContext();

        protected void EndContext()
            => _contextService.ClearCurrentContext();
    }
}