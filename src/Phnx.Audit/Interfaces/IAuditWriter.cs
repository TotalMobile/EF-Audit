using Microsoft.EntityFrameworkCore;
using Phnx.Audit.Models;

namespace Phnx.Audit
{
    public interface IAuditWriter<TContext> where TContext : DbContext
    {
        void Write<TAuditEntry, TEntityKey>(TAuditEntry auditEntry) where TAuditEntry : AuditEntryDataModel<TEntityKey>;
    }
}