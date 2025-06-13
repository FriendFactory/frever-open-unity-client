using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;

namespace Models
{
    public class CharacterControllerBodyAnimation : IEntity
    {
        public long Id { get; set; }
        public long CharacterControllerId { get; set; }
        public long BodyAnimationId { get; set; }
        public int AnimationSpeed { get; set; }
        public int ActivationCue { get; set; }
        public int EndCue { get; set; }
        public bool Locomotion { get; set; }
        public bool Looping { get; set; }

        public virtual BodyAnimationInfo BodyAnimation { get; set; }
    }
}