using System;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.CameraManaging.CameraInputHandling;
using UIManaging.Localization;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabs.AdvancedCameraTabs
{
    internal abstract class AdvancedCameraTab : MultipleAdvancedOptionTab
    {
        [Inject] protected LevelEditorCameraSettingsLocalization Localization;
        
        protected ICameraInputController CameraInputController;

        public event Action SettingChanged;

        public abstract bool ResetOnSettingsChanged { get; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(ICameraInputController cameraInputController)
        {
            CameraInputController = cameraInputController;
            AdvancedOptionTabView.SettingChanged += OnSettingChanged;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDestroy()
        {
            AdvancedOptionTabView.SettingChanged -= OnSettingChanged;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnSettingChanged()
        {
            SettingChanged?.Invoke();
        }
    }
}
