using System;
using System.Collections.Generic;
using Bridge.Models.Common;

namespace Models
{
    public class Level : IEntity, IGroupAccessible, ITimeChangesTrackable
    {
        public Level()
        {
            Event = new HashSet<Event>();
        }

        public long Id { get; set; }
        public long GroupId { get; set; }
        public long OriginalGroupId { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public long LevelTemplateId { get; set; }
        public long VerticalCategoryId { get; set; }
        public long LanguageId { get; set; }
        public long? RemixedFromLevelId { get; set; }
        public string Description { get; set; }
        public long? SchoolTaskId { get; set; }
        public long LevelTypeId { get; set; }
        public virtual ICollection<Event> Event { get; set; }
    }
}
