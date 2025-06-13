using Bridge.Models.Common;

namespace Models
{
    public class Country : IEntity
    {
        public long Id { get; set; }
        public string Isoname { get; set; }
        public string DisplayName { get; set; }
    }
}