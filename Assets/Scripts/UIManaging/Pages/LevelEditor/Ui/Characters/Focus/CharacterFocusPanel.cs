using System.Collections.Generic;
using System.Linq;
using Common.Abstract;
using Extensions;
using Modules.Amplitude;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.Permissions;
using UIManaging.Pages.LevelEditor.Ui.RecordingButton;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.Characters
{
    public sealed class CharacterFocusPanel : BaseContextlessPanel
    {
        private static readonly DbModelType[] TYPES_REQUIRES_GROUP_TARGET_ROTATION_UPDATE = {DbModelType.SetLocation, DbModelType.CharacterSpawnPosition};

        [SerializeField] private FocusPanelToggleButton _toggleButton;
        [SerializeField] private RectTransform _focusButtonsContainer;
        [Header("Focus Buttons")]
        [SerializeField] private CharacterFocusGroupButton _targetGroupButton;
        [SerializeField] private CharacterFocusButtonBase[] _characterFocusButtons;

        [Inject] private ILevelManager _levelManager;
        [Inject] private ICameraSystem _cameraSystem;
        [Inject] private EventRecordingService _eventRecordingService;
        [Inject] private ICameraTemplatesManager _templatesManager;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private MicrophonePermissionHelper _microphonePermissionHelper;
        [Inject] private AudioRecordingStateController _audioRecordingStateController;
        [Inject] private LevelEditorPageModel _levelEditorPageModel;

        private bool _isInitialized;
        private bool _isCollapsed;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        internal CharacterFocusButtonBase[] Buttons => _characterFocusButtons;
        internal List<CharacterFocusButton> CharacterButtons { get; private set; }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Update()
        {
            СheckIfPressedOutside();
        }

        private void OnDisable()
        {
            Expand(false);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            CharacterButtons = new List<CharacterFocusButton>();

            for (var i = 0; i < Buttons.Length; i++)
            {
                var characterButton = Buttons[i];
                characterButton.Initialize(_levelManager, _cameraSystem);
                characterButton.PositionId = i;

                if (characterButton is CharacterFocusButton focusButton)
                {
                    CharacterButtons.Add(focusButton);
                }

                characterButton.RemoveOnClickListener(OnCharacterButtonClicked);
                characterButton.AddOnClickListener(OnCharacterButtonClicked);

                void OnCharacterButtonClicked()
                {
                    SetTargetCharacterSequenceNumber(characterButton.TargetSequenceNumber);
                    UpdateSelectedCharacterButton(characterButton);
                    _cameraSystem.FocusOnTargetForce();
                    _templatesManager.SaveCurrentCameraStateAsStartFrameForTemplates();
                }
            }

            Expand(false);
            _toggleButton.SetListener(OnToggleClicked);

            SubscribeToEvents();
        }
        
        protected override void BeforeCleanUp()
        {
            UnSubscribeFromEvents();
        }

        public void Refresh()
        {
            var activeCharacterButtons = CharacterButtons.Where(x => x.gameObject.activeSelf).ToArray();
            var isMoreThanOneCharacter = activeCharacterButtons.Length > 1;
            _targetGroupButton.SetActive(isMoreThanOneCharacter);

            var activeFocusTargetButtons = Buttons.Where(x => x.gameObject.activeSelf).ToArray();
            _targetGroupButton.UpdateTargetGroups();

            var targetButton = activeFocusTargetButtons.FirstOrDefault(x => x.TargetSequenceNumber == _levelManager.EditingCharacterSequenceNumber);
            targetButton = targetButton == null ? activeFocusTargetButtons.First() : targetButton;

            UpdateSelectedCharacterButton(targetButton);

            this.SetActive(isMoreThanOneCharacter);
        }

        public void UpdateCharactersReferencesAndThumbnails()
        {
            var characterControllers = _levelManager.TargetEvent.CharacterController.ToArray();
            for (var i = 0; i < CharacterButtons.Count; i++)
            {
                var isActive = i < characterControllers.Length;
                CharacterButtons[i].SetActive(isActive);

                if (!isActive) continue;

                var character = characterControllers[i].Character;
                var isSelected = CharacterButtons[i].IsSelected && CharacterButtons[i].Character?.Id == character.Id;
                CharacterButtons[i].SetSelected(isSelected);
                CharacterButtons[i].Character = character.ToCharacterInfo();
                CharacterButtons[i].UpdateThumbnail(isSelected ? OnSelectedThumbnailUpdated : null);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void СheckIfPressedOutside()
        {
            if (_isCollapsed) return;

            #if UNITY_EDITOR
                if (!Input.GetMouseButtonUp(0)) return;
            #else
                if (Input.touchCount == 0 || Input.GetTouch(0).phase != TouchPhase.Ended) return;
            #endif

            if (RectTransformUtility.RectangleContainsScreenPoint(_focusButtonsContainer, Input.mousePosition)) return;
            if (RectTransformUtility.RectangleContainsScreenPoint(_toggleButton.RectTransform, Input.mousePosition)) return;

            Expand(false);
            _levelEditorPageModel.ReturnToPrevState();
        }


        private void Expand(bool expand)
        {
            _focusButtonsContainer.SetActive(expand);
            _isCollapsed = !expand;
            _toggleButton.SetBackground(_isCollapsed);
        }

        private void OnToggleClicked()
        {
            ToggleVisibility();
            if (_isCollapsed)
            {
                _levelEditorPageModel.ReturnToPrevState();
            }
            else
            {
                _levelEditorPageModel.ChangeState(LevelEditorState.FocusCharacterSelection);
            }
        }
        
        private void ToggleVisibility()
        {
            Expand(_isCollapsed);
        }

        private void SubscribeToEvents()
        {
            _levelManager.AssetUpdateCompleted += OnAssetUpdated;
            _levelManager.CharacterReplaced += OnCharacterReplaced;
            _levelManager.CharactersPositionsSwapped += Refresh;
            _levelManager.EventLoadingCompleted += _targetGroupButton.UpdateGroupTargetRotation;
        }

        private void UnSubscribeFromEvents()
        {
            _levelManager.AssetUpdateCompleted -= OnAssetUpdated;
            _levelManager.CharacterReplaced -= OnCharacterReplaced;
            _levelManager.CharactersPositionsSwapped -= Refresh;
            _levelManager.EventLoadingCompleted -= _targetGroupButton.UpdateGroupTargetRotation;
        }

        private void SetTargetCharacterSequenceNumber(int index)
        {
            _levelManager.TargetCharacterSequenceNumber = index;
            var focusCharacterChangedMetaData = new Dictionary<string, object> { [AmplitudeEventConstants.EventProperties.CHARACTER_SEQUENCE_NUMBER] = index};
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.CHARACTER_FOCUS_CHANGED, focusCharacterChangedMetaData);
        }

        private void UpdateSelectedCharacterButton(CharacterFocusButtonBase characterButton)
        {
            if (characterButton == null || characterButton.IsSelected) return;

            ShowButtonAsSelected(characterButton);
            _toggleButton.UpdateThumbnail(characterButton.Thumbnail?.sprite);

            var characterController = _levelManager.TargetEvent.GetCharacterController(characterButton.TargetSequenceNumber);
            _levelManager.TargetCharacterSequenceNumber = characterController?.ControllerSequenceNumber ?? -1;

            characterButton.FocusOnTarget();
            _templatesManager.SaveCurrentCameraStateAsStartFrameForTemplates();
        }

        private void ShowButtonAsSelected(CharacterFocusButtonBase currentButton)
        {
            foreach (var button in Buttons)
            {
                var shouldBeSelected = button == currentButton;
                SetSelectedCharacterButton(button, shouldBeSelected);
            }
        }

        private void SetSelectedCharacterButton(CharacterFocusButtonBase button, bool selected)
        {
            if (button.IsSelected && selected) return;
            button.SetSelected(selected);
        }

        private void OnCharacterReplaced(ICharacterAsset characterAsset)
        {
            UpdateCharactersReferencesAndThumbnails();
            _targetGroupButton.UpdateTargetGroups();
        }

        private void OnAssetUpdated(DbModelType type)
        {
            if (!TYPES_REQUIRES_GROUP_TARGET_ROTATION_UPDATE.Contains(type)) return;
            _targetGroupButton.UpdateGroupTargetRotation();
        }

        private void OnSelectedThumbnailUpdated(CharacterFocusButton button, Sprite sprite)
        {
            if (!button.IsSelected) return;
            _toggleButton.UpdateThumbnail(sprite);
        }
    }
}