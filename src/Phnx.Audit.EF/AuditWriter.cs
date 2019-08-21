using Microsoft.EntityFrameworkCore;
using Phnx.Audit.EF.Models;

namespace Phnx.Audit.EF
{
    public class AuditWriter<TContext> : IAuditWriter<TContext> where TContext : DbContext
    {
        public AuditWriter(TContext dbContext)
        {
            DbContext = dbContext;
        }

        protected TContext DbContext { get; }

        public void Write<TAuditEntry, TEntityKey>(TAuditEntry auditEntry) where TAuditEntry : AuditEntryDataModel<TEntityKey>
        {
            DbContext.Add(auditEntry);
        }
    }
}
