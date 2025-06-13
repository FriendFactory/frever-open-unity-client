using Extensions;
using Models;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker
{
    public sealed class CharacterSpawnFormationStateComparer : BaseAssetStateComparer
    {
        private long? _id;
        
        public override DbModelType Type => DbModelType.SpawnFormation;
        
        public override void SaveState(Event targetEvent)
        {
            _id = targetEvent.CharacterSpawnPositionFormationId;
        }

        protected override bool CheckInternal(Event targetEvent)
        {
            return _id != targetEvent.CharacterSpawnPositionFormationId;
        }
    }
}