using Extensions;
using Models;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.LevelManagement;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.AdvancedCameraTabViews
{
    public abstract class AdvancedCameraTabView : AdvancedOptionTabView
    {
        protected ICameraSystem CameraSystem;
        protected CameraController CurrentCameraController => LevelManager.TargetEvent.GetCameraController();
        protected ILevelManager LevelManager;

        [Inject]
        private void Construct(ICameraSystem cameraSystem, ILevelManager levelManager)
        {
            CameraSystem = cameraSystem;
            LevelManager = levelManager;
        }
    }
}
