using System;
using System.Collections.Generic;
using System.Threading;
using Bridge;
using Modules.Amplitude;
using JetBrains.Annotations;
using Modules.VideoStreaming.Remix.Selection;
using Modules.VideoStreaming.Remix.UI;
using UI.UIAnimators;
using UIManaging.Localization;
using UIManaging.Pages.Feed.Remix.Collection;
using UIManaging.Pages.Feed.Remix.Loaders;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace UIManaging.Pages.Feed.Remix
{
    internal abstract class CharacterSelectionPopupBase : BasePopup<CharacterSelectionPopupConfiguration>
    {
        protected const int PAGE_SIZE = 20;
        
        [SerializeField] private NextButton _nextButton;
        [SerializeField] private SelectedCharacters _selectedCharacterManager;
        [SerializeField] private List<Button> _closeButtons;
        [SerializeField] private CharacterSelectionButtonList _characterSelectionButtonList;
        [SerializeField] private BaseUiAnimationPlayer _animationPlayer;

        [Inject] private FeedLocalization _localization;
        
        protected int CharacterCount;
        protected SelectedCharacters SelectedCharacterManager => _selectedCharacterManager;
        protected abstract bool CanDeselectCharacters { get; }
        
        protected IBridge _bridge;
        protected CharacterSelectionListModel _characterSelectionListModel;
        
        private List<CharacterInfo> _friendsCharacters;
        private Action<Dictionary<long, CharacterInfo>> _selectionDone;
        private Action _selectionCancelled;

        private HashSet<long> _presetIds;

        private AmplitudeManager _amplitudeManager;
        private CancellationTokenSource _cancellationSource;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(IBridge bridge, AmplitudeManager amplitudeManager)
        {
            _bridge = bridge;
            _amplitudeManager = amplitudeManager;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {
            _closeButtons.ForEach(x => x.onClick.AddListener(AnimateCloseWindow));
        }

        protected void OnEnable()
        {
            _animationPlayer.PlayHideAnimationInstant();
        }

        protected virtual void OnDestroy()
        {
            _closeButtons.ForEach(x => x.onClick.RemoveListener(AnimateCloseWindow));
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Hide(object result)
        {
            base.Hide(result);
            CloseWindow();
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnConfigure(CharacterSelectionPopupConfiguration configuration)
        {
            CharacterCount = configuration.UniqueIds.Count;
            _presetIds = configuration.PresetIds;
            _selectionDone = configuration.SelectionDone;
            _selectionCancelled = configuration.OnCancelled;
            
            _selectedCharacterManager.Initialize(configuration.UniqueIds);
            _nextButton.Initialize(OnClickedNext, CharacterCount);

            UpdateNumberOfSelectedCharacters(0);

            var paginatedCharactersLoader = new PaginatedCharactersLoader(_bridge, configuration.UniverseId, PAGE_SIZE);
            _characterSelectionListModel =
                new CharacterSelectionListModel(SelectedCharacterManager, paginatedCharactersLoader, _bridge, CanDeselectCharacters, _localization);
            _characterSelectionButtonList.Initialize(_characterSelectionListModel);

            SelectedCharacterManager.SelectionChanged += SelectionChanged;

            OpenWindow();
        }

        protected void UpdateNumberOfSelectedCharacters(int count)
        {
            _nextButton.UpdateText(count);
        }

        protected virtual void CleanUp()
        {
            _characterSelectionListModel?.Dispose();
            _characterSelectionListModel = null;

            SelectedCharacterManager.SelectionChanged -= SelectionChanged;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OpenWindow()
        {
            _cancellationSource?.Cancel();
            _cancellationSource = new CancellationTokenSource();

            _animationPlayer.PlayShowAnimation();

            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.REMIX_CHARACTER_SELECTION);
        }

        private void SelectionChanged()
        {
            UpdateNumberOfSelectedCharacters(SelectedCharacterManager.SelectedCharacterModels.Count);
        }

        private void OnClickedNext()
        {
            var characters = _selectedCharacterManager.GetOrderedCharacters();
            _selectionDone.Invoke(characters);
        }

        private void AnimateCloseWindow()
        {
            _animationPlayer.PlayHideAnimation(OnHideCompete);

            void OnHideCompete()
            {
                _selectionCancelled?.Invoke();
                Hide();
            }
        }

        protected virtual void CloseWindow()
        {
            _cancellationSource?.Cancel();

            CleanUp();
            
            gameObject.SetActive(false);
        }
    }
}