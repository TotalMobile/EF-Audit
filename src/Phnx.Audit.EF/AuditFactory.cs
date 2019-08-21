using Microsoft.EntityFrameworkCore;
using Phnx.Audit.EF.Fluent;
using Phnx.Audit.EF.Models;
using System;

namespace Phnx.Audit.EF
{
    public class AuditFactory<TContext, TEntity, TAuditEntry, TEntityKey> : IAuditFactory<TContext, TEntity, TAuditEntry, TEntityKey> where TContext : DbContext
        where TAuditEntry : AuditEntryDataModel<TEntityKey>, new()
    {
        internal AuditFactory(IAuditWriter<TContext> auditWriter, IChangeDetectionService<TContext> changeDetectionService, Func<TEntity, TEntityKey> keySelector)
        {
            AuditWriter = auditWriter ?? throw new ArgumentNullException(nameof(auditWriter));
            ChangeDetectionService = changeDetectionService ?? throw new ArgumentNullException(nameof(changeDetectionService));
            KeySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        }

        protected Func<TEntity, TEntityKey> KeySelector { get; }
        protected IAuditWriter<TContext> AuditWriter { get; }
        protected IChangeDetectionService<TContext> ChangeDetectionService { get; }

        public FinalizedFluentAudit<TContext, TAuditEntry, TEntityKey> GenerateEntry(TEntity entity, DateTime auditedOn)
        {
            var entry = new TAuditEntry
            {
                AuditedOn = auditedOn,
                EntityId = KeySelector(entity)
            };

            var fluent = new FluentAudit<TContext, TAuditEntry, TEntityKey>(ChangeDetectionService, AuditWriter, entry);

            return fluent.WithChangesFor(entity);
        }
    }
}
