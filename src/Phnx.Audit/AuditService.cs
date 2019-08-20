using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Phnx.Audit.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Phnx.Audit
{
    public class AuditService<TContext> : IAuditService<TContext> where TContext : DbContext
    {
        public TContext Context { get; }

        public AuditService(TContext context)
        {
            Context = context;
        }

        public void AuditChanges<TEntity, TEntityKey, TAuditEntry>(Func<TContext, DbSet<TAuditEntry>> auditsSelector, TEntity entity, string operationName, Func<TEntity, TEntityKey> idSelector = null, string changedByUserId = null)
            where TEntity : class
            where TAuditEntry : AuditEntryDataModel<TEntityKey>, new()
        {
            if (auditsSelector is null)
            {
                throw new ArgumentNullException(nameof(auditsSelector));
            }

            DbSet<TAuditEntry> auditDbSet = auditsSelector(Context) ?? throw new ArgumentException($"The result of {nameof(auditsSelector)} cannot be null", nameof(auditsSelector));

            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            TAuditEntry newAudit = GetAuditEntry<TEntity, TEntityKey, TAuditEntry>(entity, operationName, changedByUserId, idSelector);

            OperationFinished<TEntityKey, TAuditEntry>(newAudit, auditDbSet);
        }

        private void OperationFinished<TEntityKey, TAuditEntry>(TAuditEntry auditEntry, DbSet<TAuditEntry> auditsTable)
            where TAuditEntry : AuditEntryDataModel<TEntityKey>
        {
            Debug.Assert(auditEntry != null);
            Debug.Assert(auditsTable != null);

            auditsTable.Add(auditEntry);
        }

        private TAuditEntry GetAuditEntry<TEntity, TEntityKey, TAuditEntry>(TEntity changedEntity, string operationName, string changedByUserId, Func<TEntity, TEntityKey> idSelector)
            where TAuditEntry : AuditEntryDataModel<TEntityKey>, new()
        {
            var auditEntry = new TAuditEntry
            {
                AuditedOn = DateTime.UtcNow,
                Description = operationName,
                UserId = changedByUserId
            };

            if (idSelector != null)
            {
                auditEntry.EntityId = idSelector(changedEntity);
            }

            EntityEntry tracker = Context.Entry(changedEntity);

            switch (tracker.State)
            {
                case EntityState.Added:
                    auditEntry.Type = AuditedOperationTypeEnum.Insert;
                    auditEntry.EntityAfterJson = SerializeObject(changedEntity);
                    break;
                case EntityState.Deleted:
                    auditEntry.Type = AuditedOperationTypeEnum.Delete;
                    auditEntry.EntityBeforeJson = SerializeObject(changedEntity);
                    break;
                case EntityState.Modified:
                case EntityState.Unchanged:
                    auditEntry.Type = AuditedOperationTypeEnum.Update;
                    var changed = GetChangedProperties(tracker).ToList();
                    var beforeChanges = changed.ToDictionary(c => c.Name, c => c.Before);
                    var afterChanges = changed.ToDictionary(c => c.Name, c => c.After);

                    auditEntry.EntityBeforeJson = SerializeProperties(beforeChanges);
                    auditEntry.EntityAfterJson = SerializeProperties(afterChanges);
                    break;
                default:
                    // Not tracked
                    return null;
            }

            return auditEntry;
        }

        private IEnumerable<ChangedMember> GetChangedProperties(EntityEntry tracker)
        {
            foreach (PropertyEntry prop in tracker.Properties)
            {
                if (prop.OriginalValue is null && prop.CurrentValue is null)
                {
                    continue;
                }

                if (prop.IsModified ||
                    prop.OriginalValue is null && prop.CurrentValue != null ||
                    !prop.OriginalValue.Equals(prop.CurrentValue))
                {
                    yield return new ChangedMember
                    {
                        Name = prop.Metadata.Name,
                        After = prop.CurrentValue,
                        Before = prop.OriginalValue
                    };
                }
            }
        }

        private string SerializeObject(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        private string SerializeProperties(Dictionary<string, object> properties)
        {
            return JsonConvert.SerializeObject(properties);
        }
    }
}
