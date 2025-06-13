using Bridge.Models.Common;

namespace Models
{
    public class Group : IEntity, INamed
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
    }
}