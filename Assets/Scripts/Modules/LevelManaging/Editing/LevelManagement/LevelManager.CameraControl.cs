using System.Linq;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.CameraManaging;
using UnityEngine;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    internal sealed partial class LevelManager
    {
        private readonly PreviousEventLastCameraFrameProvider _previousEventLastCameraFrameProvider;
        
        public Camera GetActiveCamera()
        {
            return _assetManager.GetActiveAssets<ISetLocationAsset>().FirstOrDefault()?.Camera;
        }

        public Camera GetCurrentEventCamera()
        {
            return _assetManager.GetAllLoadedAssets<ISetLocationAsset>().FirstOrDefault()?.Camera;
        }

        public void RefreshCameraGroupFocus()
        {
            _cameraFocusManager.RefreshCameraGroupPosition(TargetEvent);
        }

        public CameraAnimationFrame GetCurrentCameraAnimationFirstFrame()
        {
            var clip = GetCurrentCameraAnimationClip();
            return clip.GetFrame(0);
        }

        public CameraAnimationFrame GetCurrentCameraAnimationLastFrame()
        {
            var clip = GetCurrentCameraAnimationClip();
            var length = clip.Length;
            return clip.GetFrame(length);
        }

        public void PutCameraOnLastCameraAnimationFrame()
        {
            var clip = GetCurrentCameraAnimationClip();
            _cameraSystem.SetCameraOnEndPosition(clip);
           
            var lastFrame = clip.GetFrame(clip.Length);
            _cameraTemplatesManager.SetStartFrameForTemplates(lastFrame);
        }

        public void PutCameraOnFirstCameraAnimationFrame()
        {
            var currentCameraAnimation = _assetManager.GetActiveAssets<ICameraAnimationAsset>()?.FirstOrDefault();
            _cameraSystem.SetCameraOnStartPosition(currentCameraAnimation?.Clip);
        }

        public void StopCameraAnimation()
        {
            _cameraSystem.StopAnimation();
        }

        public void PlayTeaserClip(long templateId)
        {
            var templateClip = _cameraTemplatesManager.GetTemplateClipById(templateId);
            _cameraSystem.PlayTemplate(templateClip, TemplatePlayingMode.Teaser);
        }

        public CameraAnimationFrame GetPreviousEventCameraAnimationLastFrame()
        {
            return _previousEventLastCameraFrameProvider.GetPreviousEventCameraAnimationLastFrame(CurrentLevel, TargetEvent);
        }

        private CameraAnimationClip GetCurrentCameraAnimationClip()
        {
            return GetCurrentCameraAnimationAsset().Clip;
        }
    }
}