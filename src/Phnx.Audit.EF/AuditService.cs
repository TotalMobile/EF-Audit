using Microsoft.EntityFrameworkCore;
using Phnx.Audit.EF.Fluent;
using Phnx.Audit.EF.Models;
using System;

namespace Phnx.Audit.EF
{
    public class AuditService<TContext> : IAuditService<TContext> where TContext : DbContext
    {
        public AuditService(TContext context, IChangeDetectionService changeDetectionService)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            ChangeDetectionService = changeDetectionService ?? throw new ArgumentNullException(nameof(changeDetectionService));
        }

        protected TContext Context { get; }
        protected IChangeDetectionService ChangeDetectionService { get; }

        public FluentAudit<TContext, TAuditEntry, TEntity> GenerateEntry<TAuditEntry, TEntity>(TEntity entity)
            where TEntity : class
            where TAuditEntry : AuditEntryDataModel<TEntity>, new()
        {
            return GenerateEntry<TAuditEntry, TEntity>(entity, DateTime.UtcNow);
        }

        public FluentAudit<TContext, TAuditEntry, TEntity> GenerateEntry<TAuditEntry, TEntity>(TEntity model, DateTime auditedOn)
            where TEntity : class
            where TAuditEntry : AuditEntryDataModel<TEntity>, new()
        {
            var entity = Context.Entry(model);
            var (type, beforeJson, afterJson) = ChangeDetectionService.SerializeEntityChanges(entity);

            var entry = new TAuditEntry
            {
                AuditedOn = auditedOn,
                Entity = model,
                Type = type,
                EntityBeforeJson = beforeJson,
                EntityAfterJson = afterJson
            };

            Context.Add(entry);

            var fluent = new FluentAudit<TContext, TAuditEntry, TEntity>(entry);

            return fluent;
        }
    }
}
