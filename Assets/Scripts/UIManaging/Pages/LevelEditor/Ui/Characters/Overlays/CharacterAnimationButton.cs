using System.Collections.Generic;
using Modules.Amplitude;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.LevelManagement;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.AssetButtons
{
    internal sealed class CharacterAnimationButton : BaseAssetButton
    {
        [Inject] private ILevelManager _levelManager;
        [Inject] private ICameraSystem _cameraSystem;
        [Inject] private AmplitudeManager _amplitudeManager;

        private int _characterSequenceNumber;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(int characterSequenceNumber)
        {
            _characterSequenceNumber = characterSequenceNumber;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnClicked()
        {
            SetTargetSequenceNumber(_characterSequenceNumber);
            SwitchCharacterCameraFocus();

            LevelEditorPageModel.OnBodyAnimationsButtonClicked();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.BODY_ANIMATION_BUTTON_CLICKED);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetTargetSequenceNumber(int sequenceNumber)
        {
            _levelManager.EditingCharacterSequenceNumber = sequenceNumber;

            //also change target event character
            _levelManager.TargetCharacterSequenceNumber = sequenceNumber;

            var focusCharacterChangedMetaData = new Dictionary<string, object> {[AmplitudeEventConstants.EventProperties.CHARACTER_SEQUENCE_NUMBER] = sequenceNumber};
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.CHARACTER_FOCUS_CHANGED, focusCharacterChangedMetaData);
        }

        private void SwitchCharacterCameraFocus()
        {
            var currentCharacter = _levelManager.TargetCharacterAsset;
            var lookAt = currentCharacter != null ? currentCharacter.LookAtBoneGameObject : _cameraSystem.GetCinemachineLookAtTargetGroup().gameObject;
            var follow = currentCharacter != null ? currentCharacter.GameObject : _cameraSystem.GetCinemachineFollowTargetGroup().gameObject;

            _cameraSystem.SetTargets(lookAt, follow, false);
        }
    }
}