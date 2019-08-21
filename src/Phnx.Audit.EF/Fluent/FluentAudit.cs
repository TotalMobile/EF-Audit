using Microsoft.EntityFrameworkCore;
using Phnx.Audit.EF.Models;
using System;

namespace Phnx.Audit.EF.Fluent
{
    public class FluentAudit<TContext, TAuditEntry, TEntryKey>
        where TContext : DbContext
        where TAuditEntry : AuditEntryDataModel<TEntryKey>
    {
        public TAuditEntry Entry { get; }
        protected IChangeDetectionService<TContext> ChangeDetectionService { get; }
        protected IAuditWriter<TContext> AuditWriter { get; }

        internal FluentAudit(IChangeDetectionService<TContext> changeDetectionService, IAuditWriter<TContext> auditWriter, TAuditEntry entry)
        {
            Entry = entry;
            ChangeDetectionService = changeDetectionService ?? throw new ArgumentNullException(nameof(changeDetectionService));
            AuditWriter = auditWriter ?? throw new ArgumentNullException(nameof(auditWriter));
        }

        public FluentAudit<TContext, TAuditEntry, TEntryKey> WithUserId(string userId)
        {
            Entry.UserId = userId;

            return this;
        }

        public FluentAudit<TContext, TAuditEntry, TEntryKey> WithDescription(string description)
        {
            Entry.Description = description;

            return this;
        }

        public FinalizedFluentAudit<TContext, TAuditEntry, TEntryKey> WithChangesFor(object model)
        {
            var entity = ChangeDetectionService.GetEntity(model);
            var (type, beforeJson, afterJson) = ChangeDetectionService.SerializeEntityChanges(entity);

            return WithChanges(type, beforeJson, afterJson);
        }

        public FinalizedFluentAudit<TContext, TAuditEntry, TEntryKey> WithChanges(AuditedOperationTypeEnum type, string beforeJson, string afterJson)
        {
            Entry.Type = type;
            Entry.EntityBeforeJson = beforeJson;
            Entry.EntityAfterJson = afterJson;

            return new FinalizedFluentAudit<TContext, TAuditEntry, TEntryKey>(ChangeDetectionService, AuditWriter, Entry);
        }

        public static implicit operator TAuditEntry(FluentAudit<TContext, TAuditEntry, TEntryKey> fluent)
        {
            return fluent?.Entry;
        }
    }
}
