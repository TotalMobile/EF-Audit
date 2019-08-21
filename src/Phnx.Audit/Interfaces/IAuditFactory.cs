using Microsoft.EntityFrameworkCore;
using Phnx.Audit.Fluent;
using Phnx.Audit.Models;
using System;

namespace Phnx.Audit
{
    public interface IAuditFactory<TContext, TEntity, TAuditEntry, TEntityKey>
        where TContext : DbContext
        where TAuditEntry : AuditEntryDataModel<TEntityKey>, new()
    {
        FinalizedFluentAudit<TContext, TAuditEntry, TEntityKey> GenerateEntry(TEntity entity, DateTime auditedOn);
    }
}