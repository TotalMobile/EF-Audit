namespace Phnx.Audit.Models
{
    public class ChangedMember
    {
        public string Name { get; set; }

        public object Before { get; set; }

        public object After { get; set; }
    }
}
