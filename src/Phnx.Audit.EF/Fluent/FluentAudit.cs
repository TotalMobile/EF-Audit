using Microsoft.EntityFrameworkCore;
using Phnx.Audit.EF.Models;

namespace Phnx.Audit.EF.Fluent
{
    public class FluentAudit<TContext, TAuditEntry, TEntity, TEntryKey>
        where TContext : DbContext
        where TAuditEntry : AuditEntryDataModel<TEntity, TEntryKey>
        where TEntity : class
    {
        public TAuditEntry Entry { get; }

        internal FluentAudit(TAuditEntry entry)
        {
            Entry = entry;
        }

        public FluentAudit<TContext, TAuditEntry, TEntity, TEntryKey> WithUserId(string userId)
        {
            Entry.UserId = userId;

            return this;
        }

        public FluentAudit<TContext, TAuditEntry, TEntity, TEntryKey> WithDescription(string description)
        {
            Entry.Description = description;

            return this;
        }

        public static implicit operator TAuditEntry(FluentAudit<TContext, TAuditEntry, TEntity, TEntryKey> fluent)
        {
            return fluent?.Entry;
        }
    }
}
