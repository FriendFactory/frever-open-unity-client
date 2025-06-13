using Extensions;
using Models;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker
{
    public sealed class VfxStateComparer : BaseAssetStateComparer
    {
        private long? _id;
        
        public override DbModelType Type => DbModelType.Vfx;
        
        public override void SaveState(Event targetEvent)
        {
            var controller = targetEvent.GetVfxController();
            _id = controller?.VfxId;
        }

        protected override bool CheckInternal(Event targetEvent)
        {
            var controller = targetEvent.GetVfxController();
            return controller?.VfxId != _id;
        }
    }
}