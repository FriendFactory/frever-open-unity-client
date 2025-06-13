using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;

namespace Models
{
    public class CameraController : IEntity
    {
        public long Id { get; set; }
        public long EventId { get; set; }
        public int ActivationCue { get; set; }
        public int EndCue { get; set; }
        public long CameraAnimationTemplateId { get; set; }
        public int EndDepthOfFieldOffset { get; set; }
        public int StartFocusDistance { get; set; }
        public int EndFocusDistance { get; set; }
        public int? FollowSpawnPositionIndex { get; set; }
        public bool FollowAll { get; set; }
        public int? LookAtIndex { get; set; }
        public int CameraNoiseSettingsIndex { get; set; }
        public bool FollowZoom { get; set; }
        public long CameraAnimationId { get; set; }
        public int TemplateSpeed { get; set; }

        public virtual CameraAnimationFullInfo CameraAnimation { get; set; }
    }
}
