using System;
using System.ComponentModel.DataAnnotations;

namespace Phnx.Audit.Models
{
    public class AuditEntryDataModel<TKey>
    {
        public long Id { get; set; }

        public AuditedOperationTypeEnum Type { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime AuditedOn { get; set; }

        public string UserId { get; set; }

        [Required]
        public TKey EntityId { get; set; }

        public string EntityBeforeJson { get; set; }

        public string EntityAfterJson { get; set; }
    }
}
