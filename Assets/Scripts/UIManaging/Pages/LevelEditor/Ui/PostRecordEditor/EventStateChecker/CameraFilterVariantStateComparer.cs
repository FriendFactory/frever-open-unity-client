using Extensions;
using Models;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker
{
    public sealed class CameraFilterVariantStateComparer : BaseAssetStateComparer
    {
        private long? _id;
        
        public override DbModelType Type => DbModelType.CameraFilterVariant;
        
        public override void SaveState(Event targetEvent)
        {
            var cameraFilterController = targetEvent.GetCameraFilterController();
            _id = cameraFilterController?.CameraFilterVariantId;
        }

        protected override bool CheckInternal(Event targetEvent)
        {
            var cameraFilterController = targetEvent.GetCameraFilterController();
            return cameraFilterController?.CameraFilterVariantId != _id;
        }
    }
}