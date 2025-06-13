using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;

namespace Models
{
    public class CharacterController : IEntity, IOrderable
    {
        public CharacterController()
        {
            CharacterControllerBodyAnimation = new HashSet<CharacterControllerBodyAnimation>();
            CharacterControllerFaceVoice = new HashSet<CharacterControllerFaceVoice>();
        }

        public long Id { get; set; }
        public long EventId { get; set; }
        public int ControllerSequenceNumber { get; set; }
        public long CharacterId { get; set; }
        public int ActivationCue { get; set; }
        public int EndCue { get; set; }
        public long? OutfitId { get; set; }
        public long CharacterSpawnPositionId { get; set; }

        public virtual CharacterFullInfo Character { get; set; }
        public virtual OutfitFullInfo Outfit { get; set; }

        public virtual ICollection<CharacterControllerBodyAnimation> CharacterControllerBodyAnimation { get; set; }
        public virtual ICollection<CharacterControllerFaceVoice> CharacterControllerFaceVoice { get; set; }
        public int OrderNumber => ControllerSequenceNumber;
        public bool UnsavedOutfitEnabled => Outfit is { Id: <= 0, Wardrobes: { Count: > 0 } };
    }
}
