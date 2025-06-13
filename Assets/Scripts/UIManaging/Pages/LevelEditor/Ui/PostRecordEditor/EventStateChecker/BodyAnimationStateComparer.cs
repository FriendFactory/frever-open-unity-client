using System.Linq;
using Extensions;
using Models;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker
{
    public sealed class BodyAnimationStateComparer : BaseAssetStateComparer
    {
        private long[] _ids;
        
        public override DbModelType Type => DbModelType.BodyAnimation;
        
        public override void SaveState(Event targetEvent)
        {
            _ids = targetEvent.GetCharacterBodyAnimationControllers().Select(x => x.BodyAnimationId).ToArray();
        }

        protected override bool CheckInternal(Event targetEvent)
        {
            var bodyAnimations = targetEvent.GetCharacterBodyAnimationControllers().Select(x => x.BodyAnimationId).ToArray();
            return bodyAnimations.Where((id, i) => id != _ids[i]).Any();
        }
    }
}