using System.Linq;
using Common.TimeManaging;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraAnimations.Template;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.LevelManagement;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    /// <summary>
    /// Provides API to run/simulate all frames of camera for particular event
    /// </summary>
    [UsedImplicitly]
    internal sealed class CameraAnimationSimulator
    {
        private const float AVERAGE_DELTA_TIME = 1f / TARGET_FPS;
        private const float TARGET_FPS = 30;
        
        private readonly ICameraSystem _cameraSystem;
        private readonly ICameraTemplatesManager _cameraTemplatesManager;
        private readonly ILevelManager _levelManager;
        private readonly CameraAnimationDependOnAssetsUpdater _dependOnAssetsUpdater;

        public CameraAnimationSimulator(ICameraSystem cameraSystem, ICameraTemplatesManager cameraTemplatesManager, ILevelManager levelManager)
        {
            _cameraSystem = cameraSystem;
            _cameraTemplatesManager = cameraTemplatesManager;
            _levelManager = levelManager;
            _dependOnAssetsUpdater = new CameraAnimationDependOnAssetsUpdater(levelManager);
        }

        public void SimulateAllFrames(TemplateCameraAnimationClip animationClip, float animationLengthSec, ITimeSourceControl timeSource)
        {
            PrepareAssetsWhichImpactsCameraPosition(); //DWC

            _cameraTemplatesManager.UpdateStartPositionForTemplate(animationClip);

            float time = 0;

            var framesCount = (int)(animationLengthSec / AVERAGE_DELTA_TIME);
            if (animationLengthSec % AVERAGE_DELTA_TIME > 0)
            {
                framesCount++;
            }
            
            _cameraSystem.AllowCameraForMultiTimeSimulationInSingleFrame(true);

            for (var i = 0; i < framesCount; i++)
            {
                var deltaTime = GetDeltaTime(i, framesCount, animationLengthSec, time);
                time += deltaTime;
                
                timeSource.SetElapsed(time);

                _dependOnAssetsUpdater.Update(deltaTime);
                
                _cameraSystem.SimulateTemplate(animationClip, time, false);
            }

            _dependOnAssetsUpdater.Complete();
            _cameraSystem.AllowCameraForMultiTimeSimulationInSingleFrame(false);
        }

        private float GetDeltaTime(int frameIndex, int totalFrames, float totalAnimationLength, float currentAnimTime)
        {
            if (frameIndex == 0) //first frame
               return 0;
            if(frameIndex == totalFrames - 1)//last frame
                return totalAnimationLength - currentAnimTime; //remained time to fill anim length
            
            return AVERAGE_DELTA_TIME;
        }

        private void PrepareAssetsWhichImpactsCameraPosition()
        {
            var allCharacters = _levelManager.GetCurrentCharactersAssets();
            var characterAnimators = allCharacters.Where(x => x.Animator != null).Select(x => x.Animator).ToArray();
            
            _dependOnAssetsUpdater.Prepare(characterAnimators);
        }
    }
}