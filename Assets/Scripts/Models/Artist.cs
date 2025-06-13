using Bridge.Models.Common;

namespace Models
{
    public class Artist : IEntity, INamed
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}