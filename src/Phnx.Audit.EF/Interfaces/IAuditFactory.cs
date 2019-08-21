using Microsoft.EntityFrameworkCore;
using Phnx.Audit.EF.Fluent;
using Phnx.Audit.EF.Models;
using System;

namespace Phnx.Audit.EF
{
    public interface IAuditFactory<TContext, TEntity, TAuditEntry, TEntityKey>
        where TContext : DbContext
        where TAuditEntry : AuditEntryDataModel<TEntityKey>, new()
    {
        FinalizedFluentAudit<TContext, TAuditEntry, TEntityKey> GenerateEntry(TEntity entity, DateTime auditedOn);
    }
}