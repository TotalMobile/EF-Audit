using System;
using Microsoft.EntityFrameworkCore;
using Phnx.Audit.EF.Fluent;
using Phnx.Audit.EF.Models;

namespace Phnx.Audit.EF
{
    public interface IAuditService<TContext> where TContext : DbContext
    {
        FluentAudit<TContext, TAuditEntry, TEntity, TEntityKey> GenerateEntry<TAuditEntry, TEntity, TEntityKey>(TEntity entity)
            where TAuditEntry : AuditEntryDataModel<TEntity, TEntityKey>, new()
            where TEntity : class;

        FluentAudit<TContext, TAuditEntry, TEntity, TEntityKey> GenerateEntry<TAuditEntry, TEntity, TEntityKey>(TEntity entity, DateTime auditedOn)
            where TAuditEntry : AuditEntryDataModel<TEntity, TEntityKey>, new()
            where TEntity : class;
    }
}