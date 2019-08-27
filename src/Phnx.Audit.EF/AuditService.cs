﻿using Microsoft.EntityFrameworkCore;
using Phnx.Audit.EF.Fluent;
using Phnx.Audit.EF.Models;
using System;

namespace Phnx.Audit.EF
{
    public class AuditService<TContext> : IAuditService<TContext> where TContext : DbContext
    {
        public AuditService(TContext context, IChangeDetectionService<TContext> changeDetectionService)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            ChangeDetectionService = changeDetectionService ?? throw new ArgumentNullException(nameof(changeDetectionService));
        }

        protected TContext Context { get; }
        protected IChangeDetectionService<TContext> ChangeDetectionService { get; }

        public FluentAudit<TContext, TAuditEntry, TEntity, TEntityKey> GenerateEntry<TAuditEntry, TEntity, TEntityKey>(TEntity entity)
            where TEntity : class
            where TAuditEntry : AuditEntryDataModel<TEntity, TEntityKey>, new()
        {
            return GenerateEntry<TAuditEntry, TEntity, TEntityKey>(entity, DateTime.UtcNow);
        }

        public FluentAudit<TContext, TAuditEntry, TEntity, TEntityKey> GenerateEntry<TAuditEntry, TEntity, TEntityKey>(TEntity entity, DateTime auditedOn)
            where TEntity : class
            where TAuditEntry : AuditEntryDataModel<TEntity, TEntityKey>, new()
        {
            var (type, beforeJson, afterJson) = ChangeDetectionService.SerializeEntityChanges(entity);

            var entry = new TAuditEntry
            {
                AuditedOn = auditedOn,
                Entity = entity,
                Type = type,
                EntityBeforeJson = beforeJson,
                EntityAfterJson = afterJson
            };

            Context.Add(entry);

            var fluent = new FluentAudit<TContext, TAuditEntry, TEntity, TEntityKey>(entry);

            return fluent;
        }
    }
}
