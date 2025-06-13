using System.Collections.Generic;
using Bridge.Models.ClientServer.Level.Full;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common;
using Bridge.Models.Common.Files;

namespace Models
{
    public class Event : IOrderable, IEntity, IEventInfo
    {
        public Event()
        {
            CameraController = new HashSet<CameraController>();
            CameraFilterController = new HashSet<CameraFilterController>();
            CharacterController = new HashSet<CharacterController>();
            MusicController = new HashSet<MusicController>();
            SetLocationController = new HashSet<SetLocationController>();
            VfxController = new HashSet<VfxController>();
            Caption = new HashSet<CaptionFullInfo>();
        }

        public long Id { get; set; }
        public long LevelId { get; set; }
        public int LevelSequence { get; set; }
        public int TargetCharacterSequenceNumber { get; set; }
        public int Length { get; set; }
        public long CharacterSpawnPositionId { get; set; }
        public long? CharacterSpawnPositionFormationId { get; set; }
        public long? TemplateId { get; set; }
        public bool HasActualThumbnail { get; set; }

        public virtual CharacterSpawnPositionFormation CharacterSpawnPositionFormation { get; set; }
        public virtual ICollection<CameraController> CameraController { get; set; }
        public virtual ICollection<CameraFilterController> CameraFilterController { get; set; }
        public virtual ICollection<CaptionFullInfo> Caption { get; set; }
        public virtual ICollection<CharacterController> CharacterController { get; set; }
        public virtual ICollection<MusicController> MusicController { get; set; }
        public virtual ICollection<SetLocationController> SetLocationController { get; set; }
        public virtual ICollection<VfxController> VfxController { get; set; }
        public List<FileInfo> Files { get; set; }
        public int OrderNumber => LevelSequence;
    }
}