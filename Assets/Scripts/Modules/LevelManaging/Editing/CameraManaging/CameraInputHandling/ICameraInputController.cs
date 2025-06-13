using System;

namespace Modules.LevelManaging.Editing.CameraManaging.CameraInputHandling
{
    public interface ICameraInputController
    {
        bool AutoActivateOnCameraSystemActivation { get; set; }
        event Action<float> OrbitRadiusUpdated;
        event Action OrbitRadiusFinishedUpdating;
        event Action CameraModificationStarted;
        event Action CameraModificationCompleted;
        void Activate(bool activate);
    }
}
