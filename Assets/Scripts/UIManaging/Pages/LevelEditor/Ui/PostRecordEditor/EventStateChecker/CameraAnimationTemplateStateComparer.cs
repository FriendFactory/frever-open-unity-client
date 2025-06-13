using System;
using Extensions;
using Models;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.CameraSystem.Extensions;
using Modules.LevelManaging.Editing.LevelManagement;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker
{
    internal interface ICameraAnimationStateComparer : IAssetStateComparer
    {
        bool IsCameraPathAffectedByChanges { get; }
    }
    
    internal sealed class CameraAnimationTemplateStateComparer : BaseAssetStateComparer, ICameraAnimationStateComparer
    {
        private readonly ILevelManager _levelManager;
        private readonly ICameraTemplatesManager _cameraTemplatesManager;
        
        private long _id;
        private int _speed;
        private long? _cameraNoiseIndex;
        private CameraAnimationFrame _cameraAnimationFrameBeforeChange;

        public override DbModelType Type => DbModelType.CameraAnimationTemplate;
        public bool IsCameraPathAffectedByChanges { get; private set; }

        public CameraAnimationTemplateStateComparer(ILevelManager levelManager, ICameraTemplatesManager cameraTemplatesManager)
        {
            _levelManager = levelManager;
            _cameraTemplatesManager = cameraTemplatesManager;
        }

        public override void SaveState(Event targetEvent)
        {
            _id = targetEvent.GetCameraAnimationTemplateId();
            _speed = targetEvent.GetCameraAnimationTemplateSpeed();
            _cameraNoiseIndex = targetEvent.GetCameraAnimationNoiseIndex();
            _cameraAnimationFrameBeforeChange = _levelManager.GetCurrentCameraAnimationAsset().Clip.GetFrame(0);
        }

        protected override bool CheckInternal(Event targetEvent)
        {
            IsCameraPathAffectedByChanges = targetEvent.GetCameraAnimationTemplateId() != _id ||
                                            targetEvent.GetCameraAnimationTemplateSpeed() != _speed ||
                                            targetEvent.GetCameraAnimationNoiseIndex() != _cameraNoiseIndex ||
                                            IsAngeChanged() || IsZoomChanged();

            return IsCameraPathAffectedByChanges || IsDepthOfFieldChanged();
        }
        
        private bool IsAngeChanged()
        {
            var initialFrameSetup = _cameraTemplatesManager.TemplatesStartFrame;
            var isAngleChanged = !initialFrameSetup.HasEqualCinemachineValues(_cameraAnimationFrameBeforeChange);
            return isAngleChanged;
        }

        private bool IsDepthOfFieldChanged()
        {
            var initialFrameSetup = _cameraTemplatesManager.TemplatesStartFrame;
            var dofBefore = _cameraAnimationFrameBeforeChange.GetValue(CameraAnimationProperty.DepthOfField);
            var dofAfter = initialFrameSetup.GetValue(CameraAnimationProperty.DepthOfField);
            return Math.Abs(dofAfter - dofBefore) > 0.0001f;
        }

        private bool IsZoomChanged()
        {
            var initialFrameSetup = _cameraTemplatesManager.TemplatesStartFrame;
            var zoomBefore = _cameraAnimationFrameBeforeChange.GetValue(CameraAnimationProperty.FieldOfView);
            var zoomAfter = initialFrameSetup.GetValue(CameraAnimationProperty.FieldOfView);
            return Math.Abs(zoomAfter - zoomBefore) > 0.0001f;
        }
    }
}