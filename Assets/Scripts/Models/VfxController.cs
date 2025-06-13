using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;

namespace Models
{
    public class VfxController : IEntity
    {
        public long Id { get; set; }
        public long EventId { get; set; }
        public long VfxId { get; set; }
        public int AnimationSpeed { get; set; }
        public int ActivationCue { get; set; }
        public int EndCue { get; set; }
        public bool Looping { get; set; }

        public virtual VfxInfo Vfx { get; set; }
    }
}
