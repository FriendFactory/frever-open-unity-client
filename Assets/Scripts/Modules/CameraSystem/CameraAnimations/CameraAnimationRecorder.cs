using System;
using Common;
using Common.TimeManaging;
using Modules.CameraSystem.CameraSystemCore;

namespace Modules.CameraSystem.CameraAnimations
{
    internal sealed class CameraAnimationRecorder
    {
        private readonly CameraAnimationCreator _creator;
        private ITimeSource _timeSource;
        private bool _recording;
        private ICameraController _targetCamera;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CameraAnimationRecorder(CameraAnimationCreator creator)
        {
            _creator = creator;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void StartRecording(ITimeSource timeSource, ICameraController cameraController)
        {
            if(_recording) throw new InvalidOperationException("Camera animation recording is already running");
            
            _timeSource = timeSource;
            _targetCamera = cameraController;
            _targetCamera.AppliedStateToGameObject += SaveFrame;
            _recording = true;
        }

        public CameraAnimationSavingResult StopRecording(bool useUniqueName = false)
        {
            var animationData = _creator.ConstructTextFile(useUniqueName ? null : Constants.FileDefaultNames.CAMERA_ANIMATION_FILE);
            var result = new CameraAnimationSavingResult(animationData.filePath, animationData.animationString);
            OnRecordingStopped();
            return result;
        }

        public void CancelRecording()
        {
            OnRecordingStopped();
            _creator.Reset();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void SaveFrame()
        {
            var time = _timeSource.ElapsedSeconds;
            _creator.SaveValues(time);
        }

        private void OnRecordingStopped()
        {
            _recording = false;
            _targetCamera.AppliedStateToGameObject -= SaveFrame;
        }
    }
}