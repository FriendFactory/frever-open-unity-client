using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.AdvancedCameraTabViews
{
    internal sealed class FollowingSettingsTabView : AdvancedCameraTabView
    {
        [SerializeField] private Toggle _lookAtToggle;
        [SerializeField] private Toggle _followToggle;
        [SerializeField] private Button _resetButton;

        private bool _savedStateFollow;
        private bool _savedStateLookAt;
        
        public override void Setup()
        {
            _lookAtToggle.onValueChanged.AddListener(EnableCameraLookAt);
            _followToggle.onValueChanged.AddListener(EnableCameraFollow);
            _resetButton.onClick.AddListener(Reset);
        }

        public override void Display()
        {
            base.Display();
            UpdateViewComponents();
        }

        public override void Reset()
        {
            _lookAtToggle.isOn = true;
            _followToggle.isOn = true;
        }

        public override void CleanUp()
        {
            _lookAtToggle.onValueChanged.RemoveAllListeners();
            _followToggle.onValueChanged.RemoveAllListeners();
            _resetButton.onClick.RemoveListener(Reset);
        }

        public override void Discard()
        {
            _lookAtToggle.isOn = _savedStateLookAt;
            _followToggle.isOn = _savedStateFollow;
        }

        public override void SaveSettings()
        {
            _savedStateLookAt = _lookAtToggle.isOn;
            _savedStateFollow = _followToggle.isOn;
        }

        public override void UpdateViewComponents()
        {
            var cameraController = LevelManager.TargetEvent.GetCameraController();
            if (cameraController == null) return;
            
            _lookAtToggle.SetIsOnWithoutNotify(cameraController.LookAtIndex == 1);
            _followToggle.SetIsOnWithoutNotify(cameraController.FollowAll);
        }

        private void EnableCameraLookAt(bool enable)
        {
            CameraSystem.SetLookAt(enable);
            
            if(!enable) OnSettingChanged();

            if (CurrentCameraController == null) return;
            CurrentCameraController.LookAtIndex = enable ? 1 : 2;
        }

        private void EnableCameraFollow(bool enable)
        {
            CameraSystem.SetFollow(enable);
            
            if(!enable) OnSettingChanged();

            if (CurrentCameraController == null) return;
            CurrentCameraController.FollowAll = enable;
        }
    }
}