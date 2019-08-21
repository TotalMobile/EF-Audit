using System;
using Microsoft.EntityFrameworkCore;
using Phnx.Audit.EF.Fluent;
using Phnx.Audit.EF.Models;

namespace Phnx.Audit.EF
{
    public interface IAuditService<TContext> where TContext : DbContext
    {
        FluentAudit<TContext, TAuditEntry, TEntityKey> GenerateEntry<TAuditEntry, TEntityKey>(TEntityKey entityId, DateTime auditedOn) where TAuditEntry : AuditEntryDataModel<TEntityKey>, new();
        AuditFactory<TContext, TEntity, TAuditEntry, TEntityKey> GenerateForEntries<TEntity, TAuditEntry, TEntityKey>(Func<TEntity, TEntityKey> keySelector) where TAuditEntry : AuditEntryDataModel<TEntityKey>, new();
    }
}