using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phnx.Audit.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore
{
    public static class EntityTypeBuilderExtensions
    {
        /// <summary>
        /// Adds the audit table as a related table, with a foreign key constraint. The key is set to RESTRICT on delete, so that deleted items do not clear old audit entries
        /// </summary>
        /// <typeparam name="TEntity">The type of entity being audited</typeparam>
        /// <typeparam name="TAuditType">The type of entity tracking the audit</typeparam>
        /// <typeparam name="TKey">The primary key for <typeparamref name="TEntity"/></typeparam>
        /// <param name="typeBuilder">The entity framework type builder to extend</param>
        /// <param name="auditSelector">The selector for the foreign reference to the audit</param>
        /// <returns>The entity framework type builder</returns>
        public static EntityTypeBuilder<TEntity> HasAudit<TEntity, TAuditType, TKey>(this EntityTypeBuilder<TEntity> typeBuilder, Expression<Func<TEntity, IEnumerable<TAuditType>>> auditSelector)
            where TAuditType : AuditEntryDataModel<TEntity, TKey>
            where TEntity : class
        {
            if (typeBuilder is null)
            {
                throw new ArgumentNullException(nameof(typeBuilder));
            }
            if (auditSelector is null)
            {
                throw new ArgumentNullException(nameof(auditSelector));
            }

            typeBuilder
                .HasMany(auditSelector)
                .WithOne(audit => audit.Entity)
                .HasForeignKey(audit => audit.EntityId)
                .OnDelete(DeleteBehavior.Restrict);

            return typeBuilder;
        }
    }
}
