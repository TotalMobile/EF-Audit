using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Phnx.Audit.EF.Models;
using System;
using System.Collections.Generic;

namespace Phnx.Audit.EF
{
    public class ChangeDetectionService<TContext> : IChangeDetectionService<TContext> where TContext : DbContext
    {
        public ChangeDetectionService(TContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        protected TContext DbContext { get; }

        /// <summary>
        /// Gets the type of change applied to a model. If the model is not tracked, this will return <see langword="null"/>
        /// </summary>
        /// <param name="entity">The model to get the changes applied</param>
        /// <returns>The type of change applied to a model, or <see langword="null"/> if the model is not tracked</returns>
        public AuditedOperationTypeEnum GetChangeType(EntityEntry entity)
        {
            switch (entity.State)
            {
                case EntityState.Added:
                    return AuditedOperationTypeEnum.Insert;
                case EntityState.Deleted:
                    return AuditedOperationTypeEnum.Delete;
                case EntityState.Modified:
                case EntityState.Unchanged:
                    return AuditedOperationTypeEnum.Update;
                default:
                    throw new ArgumentException($"{nameof(entity)} is not tracked", nameof(entity));
            }
        }

        public (string original, string updated) SerializeEntityChanges(AuditedOperationTypeEnum changeType, EntityEntry entity)
        {
            string jsonEntity;

            switch (changeType)
            {
                case AuditedOperationTypeEnum.Insert:
                    jsonEntity = SerializeEntity(entity);
                    return (null, jsonEntity);

                case AuditedOperationTypeEnum.Update:
                    var updatedMembers = GetUpdatedMembers(entity);
                    return SerializeEntityChanges(updatedMembers);

                case AuditedOperationTypeEnum.Delete:
                    jsonEntity = SerializeEntity(entity);
                    return (jsonEntity, null);

                default:
                    throw new ArgumentOutOfRangeException(nameof(changeType));
            }
        }

        public EntityEntry GetEntity(object model)
        {
            return DbContext.Entry(model);
        }

        private string SerializeEntity(EntityEntry entity)
        {
            return JsonConvert.SerializeObject(entity.Entity, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        private (string original, string updated) SerializeEntityChanges(IEnumerable<ChangedMember> changes)
        {
            var original = new Dictionary<string, object>();
            var updated = new Dictionary<string, object>();

            foreach (var change in changes)
            {
                original.Add(change.Name, change.Before);
                updated.Add(change.Name, change.After);
            }

            var originalAsJson = JsonConvert.SerializeObject(original);
            var updatedAsJson = JsonConvert.SerializeObject(updated);

            return (originalAsJson, updatedAsJson);
        }

        private IEnumerable<ChangedMember> GetUpdatedMembers(EntityEntry entity)
        {
            AuditedOperationTypeEnum changeType = GetChangeType(entity);

            foreach (PropertyEntry prop in entity.Properties)
            {
                if (MemberHasBeenUpdated(prop, out var change))
                {
                    yield return change;
                }
            }
        }

        private bool MemberHasBeenUpdated(PropertyEntry entityMember, out ChangedMember update)
        {
            update = new ChangedMember();

            if (!entityMember.IsModified)
            {
                // IsModified flag is sometimes incorrectly false. Compare original and current member values by reference

                if (entityMember.OriginalValue is null)
                {
                    if (entityMember.CurrentValue is null)
                    {
                        return false;
                    }
                }
                else if (entityMember.OriginalValue.Equals(entityMember.CurrentValue))
                {
                    return false;
                }
            }

            update.Before = entityMember.OriginalValue;
            update.After = entityMember.CurrentValue;
            update.Name = entityMember.Metadata.Name;

            return true;
        }
    }
}
