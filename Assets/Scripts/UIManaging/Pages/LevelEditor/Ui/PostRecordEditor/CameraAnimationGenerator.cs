using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.Common.Files;
using Common.TimeManaging;
using Extensions;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.LevelManagement;
using FileInfo = Bridge.Models.Common.Files.FileInfo;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    [UsedImplicitly]
    internal sealed class CameraAnimationGenerator: ICameraAnimationRegenerator
    {
        private static CameraAnimationProperty[] _allAnimationProps = Enum.GetValues(typeof(CameraAnimationProperty)).Cast<CameraAnimationProperty>().ToArray();
        
        private readonly ICameraSystem _cameraSystem;
        private readonly ILevelManager _levelManager;
        private readonly CameraAnimationSimulator _cameraAnimationSimulator;
        private readonly ITimeSourceControl _timeSourceControl;
        private readonly ICameraTemplatesManager _cameraTemplatesManager;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CameraAnimationGenerator(ICameraSystem cameraSystem, ILevelManager levelManager, 
            CameraAnimationSimulator cameraAnimationSimulator, ICameraTemplatesManager cameraTemplatesManager)
        {
            _cameraSystem = cameraSystem;
            _levelManager = levelManager;
            _cameraAnimationSimulator = cameraAnimationSimulator;
            _timeSourceControl = new TimeSourceControl();
            _cameraTemplatesManager = cameraTemplatesManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void ReGenerateAnimationForTargetEvent()
        {
            ReGenerateAnimationProperties(_allAnimationProps);
        }

        public void ReGenerateAnimationProperties(params CameraAnimationProperty[] props)
        {
            var previousAnimation = _levelManager.GetCurrentCameraAnimationAsset().Clip; //DWC
            var previousAnimationLength = previousAnimation.Length;

            PutCameraOnStartPosition();
            
            _timeSourceControl.Reset();
            StartRecording(_timeSourceControl);

            PrepareForSimulatingFrames();
            SimulateAllCameraFramesForCurrentEvent(previousAnimationLength, _timeSourceControl); //DWC
            
            var regeneratedAnimationData = StopRecording();
            var requestedAllPropertiesToUpdate = props.Length == _allAnimationProps.Length;
            if (!requestedAllPropertiesToUpdate)
            {
                regeneratedAnimationData = ReplaceNotRequestedPropertiesFromOriginal(regeneratedAnimationData, props);
            }
            ReplaceCurrentEventAnimation(regeneratedAnimationData);
            
            _levelManager.Simulate(0);
            _cameraSystem.Enable(false);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void StartRecording(ITimeSource timeSource)
        {
            _cameraSystem.StartRecording(timeSource);
        }
        
        private void PutCameraOnStartPosition()
        {
            var template = _cameraTemplatesManager.CurrentTemplateClip;
            _cameraSystem.Simulate(template, 0);
        }

        private void SimulateAllCameraFramesForCurrentEvent(float animationLengthSec, ITimeSourceControl timeSource)
        {
            var cameraAnimTemplate = _cameraTemplatesManager.CurrentTemplateClip;
            _cameraAnimationSimulator.SimulateAllFrames(cameraAnimTemplate, animationLengthSec, timeSource); //DWC
        }

        private CameraAnimationSavingResult StopRecording()
        {
            return _cameraSystem.StopRecording(true);
        }

        private void ReplaceCurrentEventAnimation(CameraAnimationSavingResult savingResult)
        {
            var cameraAnimationFiles = new List<FileInfo>
            {
                new FileInfo(savingResult.FilePath, FileType.MainFile)
            };
            
            var cameraAnimationModel = _levelManager.TargetEvent.GetCameraAnimation();
            cameraAnimationModel.Files = cameraAnimationFiles;
            
            _levelManager.ChangeCameraAnimation(cameraAnimationModel, savingResult.AnimationString);
        }

        private CameraAnimationSavingResult ReplaceNotRequestedPropertiesFromOriginal(CameraAnimationSavingResult savingResult, CameraAnimationProperty[] propertiesToReplace)
        {
            var originAnimation = _levelManager.GetCurrentCameraAnimationAsset().Clip;
            var newAnimation = _cameraSystem.ReplaceProperties(originAnimation, savingResult.AnimationString, propertiesToReplace);
            return newAnimation;
        }

        private void PrepareForSimulatingFrames()
        {
            var playBackSpeed = _levelManager.TargetEvent.GetCameraController().TemplateSpeed.ToKilo();
            _cameraSystem.SetPlaybackSpeed(playBackSpeed); 
        }
    }
}
