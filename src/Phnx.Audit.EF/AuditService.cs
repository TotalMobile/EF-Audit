using Microsoft.EntityFrameworkCore;
using Phnx.Audit.EF.Fluent;
using Phnx.Audit.EF.Models;
using System;

namespace Phnx.Audit.EF
{
    public class AuditService<TContext> : IAuditService<TContext> where TContext : DbContext
    {
        public AuditService(IAuditWriter<TContext> auditWriter, IChangeDetectionService<TContext> changeDetectionService, IEntityKeyService<TContext> entityKeyService)
        {
            AuditWriter = auditWriter ?? throw new ArgumentNullException(nameof(auditWriter));
            ChangeDetectionService = changeDetectionService ?? throw new ArgumentNullException(nameof(changeDetectionService));
            EntityKeyService = entityKeyService ?? throw new ArgumentNullException(nameof(entityKeyService));
        }

        protected IAuditWriter<TContext> AuditWriter { get; }
        protected IChangeDetectionService<TContext> ChangeDetectionService { get; }
        protected IEntityKeyService<TContext> EntityKeyService { get; }

        public FluentAudit<TContext, TAuditEntry, TEntityKey> GenerateEntry<TAuditEntry, TEntityKey>(TEntityKey entityId)
            where TAuditEntry : AuditEntryDataModel<TEntityKey>, new()
        {
            return GenerateEntry<TAuditEntry, TEntityKey>(entityId, DateTime.UtcNow);
        }

        public FluentAudit<TContext, TAuditEntry, TEntityKey> GenerateEntry<TAuditEntry, TEntityKey>(TEntityKey entityId, DateTime auditedOn)
            where TAuditEntry : AuditEntryDataModel<TEntityKey>, new()
        {
            var entry = new TAuditEntry
            {
                AuditedOn = auditedOn,
                EntityId = entityId
            };

            return new FluentAudit<TContext, TAuditEntry, TEntityKey>(ChangeDetectionService, AuditWriter, entry);
        }

        public FinalizedFluentAudit<TContext, TAuditEntry, TEntityKey> GenerateEntry<TEntity, TAuditEntry, TEntityKey>(TEntity entity)
            where TEntity : class
            where TAuditEntry : AuditEntryDataModel<TEntityKey>, new()
        {
            return GenerateEntry<TEntity, TAuditEntry, TEntityKey>(entity, DateTime.UtcNow);
        }

        public FinalizedFluentAudit<TContext, TAuditEntry, TEntityKey> GenerateEntry<TEntity, TAuditEntry, TEntityKey>(TEntity entity, DateTime auditedOn)
            where TEntity : class
            where TAuditEntry : AuditEntryDataModel<TEntityKey>, new()
        {
            var key = EntityKeyService.GetKey<TEntity, TEntityKey>(entity);

            var entry = new TAuditEntry
            {
                AuditedOn = auditedOn,
                EntityId = key
            };

            var fluent = new FluentAudit<TContext, TAuditEntry, TEntityKey>(ChangeDetectionService, AuditWriter, entry);

            return fluent.WithChangesFor(entity);
        }
    }
}
