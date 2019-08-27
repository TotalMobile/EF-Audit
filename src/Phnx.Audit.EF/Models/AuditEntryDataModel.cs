using System;
using System.ComponentModel.DataAnnotations;

namespace Phnx.Audit.EF.Models
{
    public class AuditEntryDataModel<TEntity, TKey>
        where TEntity : class
    {
        public long Id { get; set; }

        public AuditedOperationTypeEnum Type { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime AuditedOn { get; set; }

        public string UserId { get; set; }

        public TEntity Entity { get; set; }

        public TKey EntityId { get; set; }

        public string EntityBeforeJson { get; set; }

        public string EntityAfterJson { get; set; }
    }
}
