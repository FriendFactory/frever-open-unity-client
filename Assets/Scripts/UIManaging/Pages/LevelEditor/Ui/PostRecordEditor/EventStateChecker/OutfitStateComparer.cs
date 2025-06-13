using System.Linq;
using Extensions;
using Models;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker
{
    public sealed class OutfitStateComparer : BaseAssetStateComparer
    {
        private long?[] _ids;
        
        public override DbModelType Type => DbModelType.Outfit;
        
        public override void SaveState(Event targetEvent)
        {
            _ids = targetEvent.CharacterController.Select(x => x.OutfitId).ToArray();
        }

        protected override bool CheckInternal(Event targetEvent)
        {
            var outfitIds = targetEvent.CharacterController.Select(x => x.OutfitId).ToArray();
            return outfitIds.Where((id, i) => id != _ids[i]).Any();
        }
    }
}