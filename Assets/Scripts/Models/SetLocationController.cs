using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Level.Full;
using Bridge.Models.Common;

namespace Models
{
    public class SetLocationController : IEntity
    {
        public long Id { get; set; }
        public long EventId { get; set; }
        public long SetLocationId { get; set; }
        public int ActivationCue { get; set; }
        public int EndCue { get; set; }
        public long? TimeOfDay { get; set; }
        public long WeatherId { get; set; }
        public long? TimelapseSpeed { get; set; }
        public long? VideoClipId { get; set; }
        public long? PhotoId { get; set; }
        public long? SetLocationBackgroundId { get; set; }
        public int? VideoActivationCue { get; set; }
        public int? VideoEndCue { get; set; }
        public int VideoSoundVolume { get; set; }

        public SetLocationFullInfo SetLocation { get; set; }
        public VideoClipFullInfo VideoClip { get; set; }
        public PhotoFullInfo Photo { get; set; }
        public SetLocationBackground SetLocationBackground { get; set; }
        
        public PictureInPictureSettings PictureInPictureSettings { get; set; }
    }
}
