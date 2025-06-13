using System.Linq;
using Extensions;
using Models;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker
{
    public sealed class CharacterStateComparer : BaseAssetStateComparer
    {
        private long _targetId;
        private long[] _ids;
        
        public override DbModelType Type => DbModelType.Character;
        
        public override void SaveState(Event targetEvent)
        {
            _targetId = targetEvent.GetTargetCharacterController().CharacterId;
            _ids = targetEvent.CharacterController.Select(x => x.CharacterId).ToArray();
        }

        protected override bool CheckInternal(Event targetEvent)
        {
            var characterIds = targetEvent.CharacterController.Select(x => x.CharacterId).ToArray();
            return targetEvent.GetTargetCharacterController().CharacterId != _targetId || characterIds.Where((id, i) => id != _ids[i]).Any();
        }
    }
}