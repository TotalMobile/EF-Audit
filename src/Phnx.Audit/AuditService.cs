using Microsoft.EntityFrameworkCore;
using Phnx.Audit.Fluent;
using Phnx.Audit.Models;
using System;

namespace Phnx.Audit
{
    public class AuditService<TContext> where TContext : DbContext
    {
        public AuditService(AuditWriter<TContext> auditWriter, ChangeDetectionService<TContext> changeDetectionService)
        {
            AuditWriter = auditWriter;
            ChangeDetectionService = changeDetectionService;
        }

        protected AuditWriter<TContext> AuditWriter { get; }
        protected ChangeDetectionService<TContext> ChangeDetectionService { get; }

        public FluentAudit<TContext, TAuditEntry, TEntityKey> GenerateEntry<TAuditEntry, TEntityKey>(TEntityKey entityId)
            where TAuditEntry : AuditEntryDataModel<TEntityKey>, new()
        {
            var entry = new TAuditEntry
            {
                AuditedOn = DateTime.UtcNow,
                EntityId = entityId
            };

            return new FluentAudit<TContext, TAuditEntry, TEntityKey>(ChangeDetectionService, AuditWriter, entry);
        }

        public AuditFactory<TContext, TEntity, TAuditEntry, TEntityKey> GenerateForEntries<TEntity, TAuditEntry, TEntityKey>(
            Func<TEntity, TEntityKey> keySelector)
            where TAuditEntry : AuditEntryDataModel<TEntityKey>, new()
        {
            return new AuditFactory<TContext, TEntity, TAuditEntry, TEntityKey>(AuditWriter, ChangeDetectionService, keySelector);
        }
    }

    public class AuditFactory<TContext, TEntity, TAuditEntry, TEntityKey>
        where TContext : DbContext
        where TAuditEntry : AuditEntryDataModel<TEntityKey>, new()
    {
        internal AuditFactory(AuditWriter<TContext> auditWriter, ChangeDetectionService<TContext> changeDetectionService, Func<TEntity, TEntityKey> keySelector)
        {
            AuditWriter = auditWriter ?? throw new ArgumentNullException(nameof(auditWriter));
            ChangeDetectionService = changeDetectionService ?? throw new ArgumentNullException(nameof(changeDetectionService));
            KeySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        }

        protected Func<TEntity, TEntityKey> KeySelector { get; }
        protected AuditWriter<TContext> AuditWriter { get; }
        protected ChangeDetectionService<TContext> ChangeDetectionService { get; }

        public FluentAudit<TContext, TAuditEntry, TEntityKey> GenerateEntry(TEntity entity)
        {
            var entry = new TAuditEntry
            {
                AuditedOn = DateTime.UtcNow,
                EntityId = KeySelector(entity)
            };

            return new FluentAudit<TContext, TAuditEntry, TEntityKey>(ChangeDetectionService, AuditWriter, entry);
        }
    }
}
