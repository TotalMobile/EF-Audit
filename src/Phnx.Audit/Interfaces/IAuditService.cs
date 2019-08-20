using System;
using Microsoft.EntityFrameworkCore;
using Phnx.Audit.Models;

namespace Phnx.Audit
{
    public interface IAuditService<TContext> where TContext : DbContext
    {
        void AuditChanges<TEntity, TEntityKey, TAuditEntry>(Func<TContext, DbSet<TAuditEntry>> auditsSelector, TEntity entity, string operationName, Func<TEntity, TEntityKey> idSelector = null, string changedByUserId = null)
            where TEntity : class
            where TAuditEntry : AuditEntryDataModel<TEntityKey>, new();
    }
}