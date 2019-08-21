using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Phnx.Audit.Models;

namespace Phnx.Audit
{
    public interface IChangeDetectionService<TContext> where TContext : DbContext
    {
        AuditedOperationTypeEnum GetChangeType(EntityEntry entity);
        EntityEntry GetEntity(object model);
        (string original, string updated) SerializeEntityChanges(AuditedOperationTypeEnum changeType, EntityEntry entity);
    }
}