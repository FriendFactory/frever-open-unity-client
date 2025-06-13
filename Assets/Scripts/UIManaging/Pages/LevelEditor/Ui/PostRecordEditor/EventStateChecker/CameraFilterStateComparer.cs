using Extensions;
using Models;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker
{
    public sealed class CameraFilterStateComparer : BaseAssetStateComparer
    {
        private long? _id;
        private int? _intensity;
        
        public override DbModelType Type => DbModelType.CameraFilter;
        
        public override void SaveState(Event targetEvent)
        {
            var cameraFilterController = targetEvent.GetCameraFilterController();

            _id = cameraFilterController?.CameraFilter?.Id;
            _intensity = cameraFilterController?.CameraFilterValue;
        }

        protected override bool CheckInternal(Event targetEvent)
        {
            var cameraFilterController = targetEvent.GetCameraFilterController();

            return cameraFilterController?.CameraFilter?.Id != _id ||
                   cameraFilterController?.CameraFilterValue != _intensity;
        }
    }
}