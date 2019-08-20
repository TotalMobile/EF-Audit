using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Phnx.Audit.Models;
using System;

namespace Phnx.Audit
{
    public static class ChangeDetectionServiceExtensions
    {
        public static (AuditedOperationTypeEnum type, string before, string after) SerializeEntityChanges<TContext>(this ChangeDetectionService<TContext> service, object model) where TContext : DbContext
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
