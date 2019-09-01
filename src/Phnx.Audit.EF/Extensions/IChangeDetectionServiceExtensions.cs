using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Phnx.Audit.EF.Models;
using System;

namespace Phnx.Audit.EF
{
    public static class IChangeDetectionServiceExtensions
    {
        public static (AuditedOperationTypeEnum type, string beforeJson, string afterJson) SerializeEntityChanges(this IChangeDetectionService service, EntityEntry entity)
        {
            if (service is null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            var type = service.GetChangeType(entity);

            var (before, after) = service.SerializeEntityChanges(type, entity);

            return (type, before, after);
        }
    }
}
