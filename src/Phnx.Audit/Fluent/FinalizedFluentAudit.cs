using Microsoft.EntityFrameworkCore;
using Phnx.Audit.Models;

namespace Phnx.Audit.Fluent
{
    public class FinalizedFluentAudit<TContext, TAuditEntry, TEntryKey>
        : FluentAudit<TContext, TAuditEntry, TEntryKey>
        where TContext : DbContext
        where TAuditEntry : AuditEntryDataModel<TEntryKey>
    {
        internal FinalizedFluentAudit(IChangeDetectionService<TContext> changeDetectionService, IAuditWriter<TContext> auditWriter, TAuditEntry entry) : base(changeDetectionService, auditWriter, entry)
        {
        }

        public new FinalizedFluentAudit<TContext, TAuditEntry, TEntryKey> WithUserId(string userId)
        {
            base.WithUserId(userId);

            return this;
        }

        public new FinalizedFluentAudit<TContext, TAuditEntry, TEntryKey> WithDescription(string description)
        {
            base.WithDescription(description);

            return this;
        }

        public void Write()
        {
            AuditWriter.Write<TAuditEntry, TEntryKey>(Entry);
        }

        public static implicit operator TAuditEntry(FinalizedFluentAudit<TContext, TAuditEntry, TEntryKey> fluent)
        {
            return fluent?.Entry;
        }
    }
}
