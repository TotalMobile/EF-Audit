﻿using Microsoft.EntityFrameworkCore;
using Phnx.Audit.Fluent;
using Phnx.Audit.Models;
using System;

namespace Phnx.Audit
{
    public class AuditService<TContext> : IAuditService<TContext> where TContext : DbContext
    {
        public AuditService(IAuditWriter<TContext> auditWriter, IChangeDetectionService<TContext> changeDetectionService)
        {
            AuditWriter = auditWriter ?? throw new ArgumentNullException(nameof(auditWriter));
            ChangeDetectionService = changeDetectionService ?? throw new ArgumentNullException(nameof(changeDetectionService));
        }

        protected IAuditWriter<TContext> AuditWriter { get; }
        protected IChangeDetectionService<TContext> ChangeDetectionService { get; }

        public FluentAudit<TContext, TAuditEntry, TEntityKey> GenerateEntry<TAuditEntry, TEntityKey>(TEntityKey entityId, DateTime auditedOn)
            where TAuditEntry : AuditEntryDataModel<TEntityKey>, new()
        {
            var entry = new TAuditEntry
            {
                AuditedOn = auditedOn,
                EntityId = entityId
            };

            return new FluentAudit<TContext, TAuditEntry, TEntityKey>(ChangeDetectionService, AuditWriter, entry);
        }

        public AuditFactory<TContext, TEntity, TAuditEntry, TEntityKey> GenerateForEntries<TEntity, TAuditEntry, TEntityKey>(
            Func<TEntity, TEntityKey> keySelector)
            where TAuditEntry : AuditEntryDataModel<TEntityKey>, new()
        {
            return new AuditFactory<TContext, TEntity, TAuditEntry, TEntityKey>(AuditWriter, ChangeDetectionService, keySelector);
        }
    }
}
