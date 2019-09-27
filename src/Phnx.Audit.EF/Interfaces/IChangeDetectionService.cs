using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Phnx.Audit.EF.Models;

namespace Phnx.Audit.EF
{
    public interface IChangeDetectionService
    {
        AuditedOperationTypeEnum GetChangeType(EntityEntry entity);
        (string original, string updated) SerializeEntityChanges(AuditedOperationTypeEnum changeType, EntityEntry entity);
    }
}
