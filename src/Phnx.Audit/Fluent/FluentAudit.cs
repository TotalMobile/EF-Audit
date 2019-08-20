using Microsoft.EntityFrameworkCore;
using Phnx.Audit.Models;
using System;

namespace Phnx.Audit.Fluent
{
    public class FluentAudit<TContext, TAuditEntry, TEntryKey>
        where TContext : DbContext
        where TAuditEntry : AuditEntryDataModel<TEntryKey>
    {
        protected ChangeDetectionService<TContext> ChangeDetectionService { get; }
        protected AuditWriter<TContext> AuditWriter { get; }
        protected TAuditEntry Entry { get; }

        public FluentAudit(ChangeDetectionService<TContext> changeDetectionService, AuditWriter<TContext> auditWriter, TAuditEntry entry)
        {
            ChangeDetectionService = changeDetectionService ?? throw new ArgumentNullException(nameof(changeDetectionService));
            AuditWriter = auditWriter ?? throw new ArgumentNullException(nameof(auditWriter));
            Entry = entry;
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
            var (type, before, after) = ChangeDetectionService.SerializeEntityChanges(entity);

            Entry.Type = type;
            Entry.EntityBeforeJson = before;
            Entry.EntityAfterJson = after;

            return new FinalizedFluentAudit<TContext, TAuditEntry, TEntryKey>(ChangeDetectionService, AuditWriter, Entry);
        }

        public static implicit operator TAuditEntry(FluentAudit<TContext, TAuditEntry, TEntryKey> fluent)
        {
            return fluent?.Entry;
        }
    }
}
