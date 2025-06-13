using Extensions;
using Models;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker
{
    public sealed class CharacterSpawnPositionStateComparer : BaseAssetStateComparer
    {
        private long _id;
        
        public override DbModelType Type => DbModelType.CharacterSpawnPosition;
        
        public override void SaveState(Event targetEvent)
        {
            _id = targetEvent.CharacterSpawnPositionId;
        }

        protected override bool CheckInternal(Event targetEvent)
        {
            return targetEvent.CharacterSpawnPositionId != _id;
        }
    }
}