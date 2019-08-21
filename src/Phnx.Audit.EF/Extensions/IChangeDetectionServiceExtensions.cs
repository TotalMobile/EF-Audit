using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Phnx.Audit.EF.Models;
using System;

namespace Phnx.Audit.EF
{
    public static class IChangeDetectionServiceExtensions
    {
        public static (AuditedOperationTypeEnum type, string beforeJson, string afterJson) SerializeEntityChanges<TContext>(this IChangeDetectionService<TContext> service, object model) where TContext : DbContext
        {
            if (service is null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            EntityEntry entity = service.GetEntity(model);
            var type = service.GetChangeType(entity);

            var (before, after) = service.SerializeEntityChanges(type, entity);

            return (type, before, after);
        }
    }
}
