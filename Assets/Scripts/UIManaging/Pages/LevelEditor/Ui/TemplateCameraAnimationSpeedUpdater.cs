using Extensions;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraAnimations.Template;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.CameraManaging.CameraInputHandling;
using UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews;
using Event = Models.Event;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class TemplateCameraAnimationSpeedUpdater
    {
        private CameraAnimationSpeedAssetView _cameraAnimationSpeedAssetView;
        private readonly ICameraInputController _cameraInputController;
        private readonly ICameraSystem _cameraSystem;
        private readonly ICameraTemplatesManager _cameraTemplatesManager;
        private float _clipRelativeSpeed;
        private TemplateCameraAnimationClip _currentClip;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        private float ClipMinSpeed => _currentClip.MinSpeed;
        private float ClipSpeedRange => _cameraAnimationSpeedAssetView.SpeedRange;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public TemplateCameraAnimationSpeedUpdater(ICameraSystem cameraSystem, ICameraTemplatesManager cameraTemplatesManager, ICameraInputController cameraInputController)
        {
            _cameraSystem = cameraSystem;
            _cameraInputController = cameraInputController;
            _cameraTemplatesManager = cameraTemplatesManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Initialize(CameraAnimationSpeedAssetView cameraAnimationSpeedAssetView)
        {
            _cameraAnimationSpeedAssetView = cameraAnimationSpeedAssetView;
            _cameraAnimationSpeedAssetView.ScrollBarHorizontalPositionUpdated += SaveRelativeSpeed;
            _cameraTemplatesManager.TemplateAnimationChanged += OnCameraAnimationCameraTemplateChanged;
        }
        
        public void CleanUp()
        {
            _cameraAnimationSpeedAssetView.ScrollBarHorizontalPositionUpdated -= SaveRelativeSpeed;
            _cameraTemplatesManager.TemplateAnimationChanged -= OnCameraAnimationCameraTemplateChanged;
            UnSubscribeOrbitRadiusUpdatedEvents();
        }

        public void UpdateSpeedUIStateWithEvent(Event targetEvent)
        {
            var speed = targetEvent.GetCameraAnimationTemplateSpeed().ToKilo();
            _cameraAnimationSpeedAssetView.SetValueSilent(speed);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SaveRelativeSpeed(float position)
        {
            _clipRelativeSpeed = position;
        }

        private void OnCameraAnimationCameraTemplateChanged(TemplateCameraAnimationClip clip)
        {
            _currentClip = clip;
            UnSubscribeOrbitRadiusUpdatedEvents();

            if (clip.DoesAnimateProperty(CameraAnimationProperty.OrbitRadius))
            {
                SubscribeOrbitRadiusUpdatedEvents();
            }
        }

        private void UpdateZoomAnimationFirstAndLastFrame()
        {
            //Required in order to re-calculate max speed for distance dependent camera template animations (Zoom animations)
            _cameraTemplatesManager.UpdateStartPositionForTemplate(_currentClip);
        }

        private float ConvertRelativeSpeed(float speed)
        {
            var convertedValue = ClipSpeedRange * speed + ClipMinSpeed;
            return convertedValue;
        }
        
        private void RefreshSpeed()
        {
            var newSpeed = ConvertRelativeSpeed(_clipRelativeSpeed);
            _cameraSystem.SetPlaybackSpeed(newSpeed);
        }

        private void SubscribeOrbitRadiusUpdatedEvents()
        {
            _cameraInputController.OrbitRadiusFinishedUpdating += UpdateZoomAnimationFirstAndLastFrame;
            _cameraInputController.OrbitRadiusFinishedUpdating += RefreshSpeed;
        }
        
        private void UnSubscribeOrbitRadiusUpdatedEvents()
        {
            _cameraInputController.OrbitRadiusFinishedUpdating -= UpdateZoomAnimationFirstAndLastFrame;
            _cameraInputController.OrbitRadiusFinishedUpdating -= RefreshSpeed;
        }
    }
}
