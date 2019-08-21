namespace Phnx.Audit.EF.Tests.Fakes
{
    public class OneToOneChildModel
    {
        public string Id { get; set; }

        public string Contents { get; set; }

        public ModelToAudit Parent { get; set; }

        public string ParentId { get; set; }
    }
}
