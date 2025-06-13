using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;

namespace Models
{
    public class CharacterControllerFaceVoice : IEntity
    {
        public long Id { get; set; }
        public long CharacterControllerId { get; set; }
        public long? VoiceTrackId { get; set; }
        public long? FaceAnimationId { get; set; }
        public long? VoiceFilterId { get; set; }
        public int VoiceSoundVolume { get; set; }
        public int? VoiceFilterValue { get; set; }

        public virtual FaceAnimationFullInfo FaceAnimation { get; set; }
        public virtual VoiceFilterFullInfo VoiceFilter { get; set; }
        public virtual VoiceTrackFullInfo VoiceTrack { get; set; }
    }
}