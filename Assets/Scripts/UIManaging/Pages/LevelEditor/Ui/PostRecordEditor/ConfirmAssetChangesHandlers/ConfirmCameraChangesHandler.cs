using Extensions;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.ConfirmAssetChangesHandlers
{
    internal sealed class ConfirmCameraChangesHandler : GenerateCameraAnimOnConfirmAssetChangesHandler
    {
        private readonly ILevelManager _levelManager;
        private readonly ICameraSystem _cameraSystem;
        private readonly EventSettingsStateChecker _eventSettingsStateChecker;

        public ConfirmCameraChangesHandler(DbModelType type, ICameraAnimationRegenerator cameraAnimationGenerator, ICameraSystem cameraSystem, ILevelManager levelManager, EventSettingsStateChecker eventSettingsStateChecker) : base(type, cameraAnimationGenerator)
        {
            _levelManager = levelManager;
            _eventSettingsStateChecker = eventSettingsStateChecker;
            _cameraSystem = cameraSystem;
        }

        public override void Run()
        {
            if (_eventSettingsStateChecker.HasAnyChangeWhichAffectCameraAnimationPath(_levelManager.TargetEvent))
            {
                RegenerateCameraAnimationFile();
            }
            else
            {
                UpdatePostProcessingValuesInCameraAnimationFile();
            }
            PlaceCameraOnEventStartPosition();   //DWC is it that we are not doing this in the ConfirmSetLocationChangesHandler?
        }

        private void PlaceCameraOnEventStartPosition()
        {
            var currentEventCameraAnimation = _levelManager.GetCurrentCameraAnimationAsset();  //DWC
            var clip = currentEventCameraAnimation.Clip;
            _cameraSystem.PutCinemachineToState(clip.GetFrame(0));
        }

        private void UpdatePostProcessingValuesInCameraAnimationFile()
        {
            CameraAnimationGenerator.ReGenerateAnimationProperties(CameraAnimationProperty.DepthOfField);
        }
    }
}
