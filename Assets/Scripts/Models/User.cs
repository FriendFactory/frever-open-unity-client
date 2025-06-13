using Bridge.Models.Common;

namespace Models
{
    public class User : IEntity
    {
        public long Id { get; set; }
       
        public long? MainCharacterId { get; set; }
        public long MainGroupId { get; set; }
    }
}