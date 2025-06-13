using Extensions;
using Models;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker
{
    public sealed class SetLocationStateComparer : BaseAssetStateComparer
    {
        private long _id;
        
        public override DbModelType Type => DbModelType.SetLocation;
        
        public override void SaveState(Event targetEvent)
        {
            _id = targetEvent.GetSetLocationId();
        }

        protected override bool CheckInternal(Event targetEvent)
        {
            return targetEvent.GetSetLocationId() != _id;
        }
    }
}