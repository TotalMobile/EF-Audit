namespace Phnx.Audit.Tests.Fakes
{
    public class OneToManyChildModel
    {
        public int Id { get; set; }

        public string Contents { get; set; }

        public string ParentModelId { get; set; }

        public ModelToAudit ParentModel { get; set; }
    }
}
