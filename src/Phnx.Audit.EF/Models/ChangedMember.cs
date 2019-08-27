namespace Phnx.Audit.EF.Models
{
    internal struct ChangedMember
    {
        public string Name { get; set; }

        public object Before { get; set; }

        public object After { get; set; }
    }
}
