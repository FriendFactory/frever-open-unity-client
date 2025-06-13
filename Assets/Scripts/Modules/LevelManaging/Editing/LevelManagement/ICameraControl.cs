using Modules.CameraSystem.CameraAnimations;
using UnityEngine;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    public interface ICameraControl
    {
        void PutCameraOnLastCameraAnimationFrame();
        void PutCameraOnFirstCameraAnimationFrame();
        void StopCameraAnimation();
        void PlayTeaserClip(long templateId);
        Camera GetActiveCamera();
        Camera GetCurrentEventCamera();
        void RefreshCameraGroupFocus();
        CameraAnimationFrame GetCurrentCameraAnimationFirstFrame();
        CameraAnimationFrame GetCurrentCameraAnimationLastFrame();
        CameraAnimationFrame GetPreviousEventCameraAnimationLastFrame();
        void SetupCameraFocusAnimationCurve(long? genderId = null);
    }
}