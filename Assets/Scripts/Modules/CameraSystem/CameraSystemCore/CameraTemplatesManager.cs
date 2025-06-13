using System;
using System.Collections.Generic;
using System.Linq;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraAnimations.Template;
using Modules.CameraSystem.CameraSettingsHandling;
using Modules.CameraSystem.PlayerCamera;
using Bridge.Models.ClientServer.StartPack.Metadata;

namespace Modules.CameraSystem.CameraSystemCore
{
    internal sealed class CameraTemplatesManager : ICameraTemplatesManager
    {
        private readonly CinemachineBasedController _cinemachineBasedController;
        private readonly CameraSettingControl _cameraSettingControl;
        private readonly CameraAnimationHolder _cameraAnimationHolder;

        private long _currentClipId;

        public CameraTemplatesManager(CameraAnimationHolder cameraAnimationHolder, CinemachineBasedController cinemachineBasedController, CameraSettingControl cameraSettingControl)
        {
            _cinemachineBasedController = cinemachineBasedController;
            _cameraSettingControl = cameraSettingControl;
            _cameraAnimationHolder = cameraAnimationHolder;
            _cameraAnimationHolder.LoadAllLocalTemplateAnimationClips();
        }
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public CameraAnimationFrame TemplatesStartFrame { get; private set; }
        public TemplateCameraAnimationClip CurrentTemplateClip => TemplateClips.First(x => x.Id == _currentClipId);
        private TemplateCameraAnimationClip[] TemplateClips => _cameraAnimationHolder.GetAllTemplateCameraAnimationClips();
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<TemplateCameraAnimationClip> TemplateAnimationChanged;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void SetupCameraAnimationTemplates(CameraAnimationTemplate[] models, long defaultTemplateId)
        {
            _cameraAnimationHolder.SetIdsForTemplates(models);
            _currentClipId = defaultTemplateId;
        }

        public void SetStartFrameForTemplates(CameraAnimationFrame frame)
        {
            TemplatesStartFrame = frame;
        }

        public void SaveCurrentCameraStateAsStartFrameForTemplates()
        {
            var currentState = GetCurrentFrameData();
            SetStartFrameForTemplates(currentState);
        }

        public void UpdateStartPositionForTemplate(TemplateCameraAnimationClip targetClip)
        {
            SetTemplateStartPointIfNull();
            var orbitRadiusMax = _cameraSettingControl.CurrentlyAppliedSetting.OrbitRadiusMax;
            UpdateTemplate(targetClip, TemplatesStartFrame, orbitRadiusMax);
        }

        public void ChangeTemplateAnimation(long id)
        {
            if (id == _currentClipId) return;
            
            _currentClipId = id;
            TemplateAnimationChanged?.Invoke(CurrentTemplateClip);
        }

        public TemplateCameraAnimationClip GetTemplateClipById(long id)
        {
            return TemplateClips.First(clip => clip.Id == id);
        }
        
        public void PrepareClipForSimulationFrame(CameraAnimationClip clip)
        {
            if(clip.AnimationType == AnimationType.TransformBased || !(clip is TemplateCameraAnimationClip template)) return;
            SetTemplateStartPointIfNull();
            var orbitRadiusMax = _cameraSettingControl.CurrentlyAppliedSetting.OrbitRadiusMax;
            UpdateTemplate(template, TemplatesStartFrame, orbitRadiusMax);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private CameraAnimationFrame GetCurrentFrameData()
        {
            // to get actual FreeLook camera state it should be active, otherwise we need to force updating on disabled component
            if(!_cinemachineBasedController.IsActive)
                _cinemachineBasedController.UpdateStateBasedOnCurrentPosition();
            
            var frameKeysData = new Dictionary<CameraAnimationProperty, float>();
            foreach (CameraAnimationProperty prop in Enum.GetValues(typeof(CameraAnimationProperty)))
            {
                var value = _cinemachineBasedController.GetValue(prop);
                frameKeysData.Add(prop, value);
            }

            return new CameraAnimationFrame(frameKeysData);
        }
        
        private void SetTemplateStartPointIfNull()
        {
            if (TemplatesStartFrame != null) return;
            _cameraSettingControl.SetupCameraStartSettings();
            SaveCurrentCameraStateAsStartFrameForTemplates();
        }

        private void UpdateTemplate(TemplateCameraAnimationClip clip, CameraAnimationFrame startFrame, float orbitRadiusMax)
        {
            clip.MaxOrbitRadius = orbitRadiusMax;
            clip.StartFrom(startFrame);
        }
    }
}
