using Bridge.Models.Common;

namespace Models
{
    public class Album : IEntity, INamed
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}