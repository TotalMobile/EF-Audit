﻿using Microsoft.EntityFrameworkCore;
using Phnx.Audit.EF.Fluent;
using Phnx.Audit.EF.Models;
using System;

namespace Phnx.Audit.EF
{
    public class AuditFactory<TContext, TEntity, TAuditEntry, TEntityKey> where TContext : DbContext
        where TAuditEntry : AuditEntryDataModel<TEntityKey>, new()
        where TEntity : class
    {
        internal AuditFactory(IEntityKeyService<TContext> entityKeyService, IAuditWriter<TContext> auditWriter, IChangeDetectionService<TContext> changeDetectionService)
        {
            EntityKeyService = entityKeyService;
            AuditWriter = auditWriter ?? throw new ArgumentNullException(nameof(auditWriter));
            ChangeDetectionService = changeDetectionService ?? throw new ArgumentNullException(nameof(changeDetectionService));
        }

        protected IEntityKeyService<TContext> EntityKeyService { get; }
        protected IAuditWriter<TContext> AuditWriter { get; }
        protected IChangeDetectionService<TContext> ChangeDetectionService { get; }

        public FinalizedFluentAudit<TContext, TAuditEntry, TEntityKey> GenerateEntry(TEntity entity)
        {
            return GenerateEntry(entity, DateTime.UtcNow);
        }

        public FinalizedFluentAudit<TContext, TAuditEntry, TEntityKey> GenerateEntry(TEntity entity, DateTime auditedOn)
        {
            var key = EntityKeyService.GetPrimaryKey<TEntity, TEntityKey>(entity);

            var entry = new TAuditEntry
            {
                AuditedOn = auditedOn,
                EntityId = key
            };

            var fluent = new FluentAudit<TContext, TAuditEntry, TEntityKey>(ChangeDetectionService, AuditWriter, entry);

            return fluent.WithChangesFor(entity);
        }
    }
}
