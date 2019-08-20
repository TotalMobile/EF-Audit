using System.Collections.Generic;

namespace Phnx.Audit.Tests.Fakes
{
    public class ManyToManyChildModel
    {
        public string Id { get; set; }

        public string Contents { get; set; }

        public IEnumerable<ModelToAudit> Parents { get; set; }
    }
}
