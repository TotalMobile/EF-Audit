using Microsoft.EntityFrameworkCore;
using Phnx.Audit.EF.Models;

namespace Phnx.Audit.EF
{
    public interface IAuditWriter<TContext> where TContext : DbContext
    {
        void AddToDatabase<TAuditEntry, TEntityKey>(TAuditEntry auditEntry) where TAuditEntry : AuditEntryDataModel<TEntityKey>;
    }
}