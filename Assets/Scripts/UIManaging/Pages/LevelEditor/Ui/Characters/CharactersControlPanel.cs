using System;
using System.Linq;
using Common.Abstract;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetHelpers;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.FeatureControls;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace UIManaging.Pages.LevelEditor.Ui.Characters
{
    internal sealed class CharactersControlPanel : BaseContextlessPanel
    {
        [SerializeField] private Button[] _openCharactersListButton;
        [SerializeField] private CharacterFocusPanel _characterFocusPanel;
        [SerializeField] private CharacterSwitch _characterSwitch;
        
        private ILevelManager _levelManager;
        private VfxBinder _vfxBinder;
        private ICharacterSelectionFeatureControl _characterSelectionFeatureControl; 

        public event Action OpenCharactersViewButtonClicked;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(ILevelManager levelManager, VfxBinder vfxBinder, ICharacterSelectionFeatureControl characterSelectionFeatureControl)
        {
            _levelManager = levelManager;
            _vfxBinder = vfxBinder;
            _characterSelectionFeatureControl = characterSelectionFeatureControl;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            SubscribeToEvents();
            _openCharactersListButton.ForEach(button => button.SetActive(_characterSelectionFeatureControl.IsFeatureEnabled));
            _characterFocusPanel.Initialize();
        }

        protected override void BeforeCleanUp()
        {
            UnSubscribeFromEvents();
            
            if (_characterFocusPanel.IsInitialized)
            {
                _characterFocusPanel.CleanUp();
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateCharacterButtons()
        {
            if (_levelManager.CurrentPlayMode == PlayMode.Preview) return;
            
            _characterFocusPanel.UpdateCharactersReferencesAndThumbnails();
            RefreshTargetCharacter();
            _characterFocusPanel.Refresh();
            UpdateVfxBindingTarget();
        }

        private void UpdateVfxBindingTarget()
        {
            var targetCharacter = _levelManager.TargetCharacterAsset;
            if (targetCharacter == null) return;

            var vfx = _levelManager.GetCurrentVfxAsset();
            if (vfx == null) return;
            _vfxBinder.Setup(vfx,targetCharacter);
        }

        private void RefreshTargetCharacter()
        {
            if (_levelManager.TargetCharacterSequenceNumber < 0) return;
            
            var activeCharacterButtons = _characterFocusPanel.CharacterButtons.Where(x => x.gameObject.activeSelf).ToArray();
            var targetCharacterButton = activeCharacterButtons.FirstOrDefault(x => x.TargetSequenceNumber == _levelManager.TargetCharacterSequenceNumber) ?? activeCharacterButtons.First();
            _levelManager.TargetCharacterSequenceNumber = targetCharacterButton.TargetSequenceNumber;
            _levelManager.EditingCharacterSequenceNumber = targetCharacterButton.TargetSequenceNumber;
        }

        private void OnCharacterSpawned(ICharacterAsset characterAsset)
        {
            UpdateCharacterButtons();
        }

        private void OnCharacterDestroyed()
        {
            UpdateCharacterButtons();
        }
        
        private void OnEventStart()
        {
            UpdateCharacterButtons();
        }

        private void OnLevelPreviewCompleted()
        {
            UpdateCharacterButtons();
        }

        private void SubscribeToEvents()
        {
            _levelManager.CharacterSpawned += OnCharacterSpawned;
            _levelManager.CharacterDestroyed += OnCharacterDestroyed;
            _levelManager.LevelPreviewCompleted += OnLevelPreviewCompleted;
            _levelManager.PreviewCancelled += OnLevelPreviewCompleted;
            _levelManager.EventStarted += OnEventStart;
            _levelManager.TemplateApplyingCompleted += UpdateCharacterButtons;
            _openCharactersListButton.ForEach(button => button.onClick.AddListener(OnOpenCharacterViewButtonClicked));
            _characterSwitch.AnyButtonClicked += _characterFocusPanel.Refresh;
            _levelManager.RecordingStarted += HideAddButton;
            _levelManager.RecordingEnded += ShowAddButtonIfFeatureAvailable;
            _levelManager.RecordingCancelled += ShowAddButtonIfFeatureAvailable;
        }

        private void UnSubscribeFromEvents()
        {
            _levelManager.CharacterSpawned -= OnCharacterSpawned;
            _levelManager.CharacterDestroyed -= OnCharacterDestroyed;
            _levelManager.EventStarted -= OnEventStart;
            _levelManager.LevelPreviewCompleted -= OnLevelPreviewCompleted;
            _levelManager.PreviewCancelled -= OnLevelPreviewCompleted;
            _levelManager.TemplateApplyingCompleted -= UpdateCharacterButtons;
            _openCharactersListButton.ForEach(button => button.onClick.RemoveListener(OnOpenCharacterViewButtonClicked));
            _characterSwitch.AnyButtonClicked -= _characterFocusPanel.Refresh;
            _levelManager.RecordingStarted -= HideAddButton;
            _levelManager.RecordingEnded -= ShowAddButtonIfFeatureAvailable;
            _levelManager.RecordingCancelled -= ShowAddButtonIfFeatureAvailable;
        }

        private void OnOpenCharacterViewButtonClicked()
        {
            OpenCharactersViewButtonClicked?.Invoke();
        }

        private void HideAddButton()
        {
            _openCharactersListButton.ForEach(button => button.SetActive(false));
        }

        private void ShowAddButtonIfFeatureAvailable()
        {
            _openCharactersListButton.ForEach(button => button.SetActive(_characterSelectionFeatureControl.IsFeatureEnabled));
        }
    }
}