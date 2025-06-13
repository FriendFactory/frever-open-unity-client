using System;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.Characters
{
    internal sealed class CharacterSwapButton : MonoBehaviour
    {
        [SerializeField] private CanvasGroup bodyCanvasGroup;
        
        private Button _button;
        private ILevelManager _levelManager;

        private int CharacterCount => _levelManager.TargetEvent?.CharacterController.Count ?? 1;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(ILevelManager levelManager, IDataFetcher dataFetcher)
        {
            _levelManager = levelManager;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _levelManager.CharacterSpawned += OnCharacterSpawned;
            _levelManager.CharacterDestroyed += OnCharacterDestroyed;
            _levelManager.EventLoadingCompleted += OnEventLoadingCompleted;
            _levelManager.StartUpdatingAsset += OnStartUpdatingAsset;
            _levelManager.StopUpdatingAsset += OnStopUpdatingAsset;

            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnEnable()
        {
            RefreshButtonVisibility();
        }

        private void OnDestroy()
        {
            _levelManager.CharacterSpawned -= OnCharacterSpawned;
            _levelManager.CharacterDestroyed -= OnCharacterDestroyed;
            _levelManager.EventLoadingCompleted -= OnEventLoadingCompleted;
            _levelManager.StartUpdatingAsset -= OnStartUpdatingAsset;
            _levelManager.StopUpdatingAsset -= OnStopUpdatingAsset;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnStartUpdatingAsset(DbModelType type, long id)
        {
            _button.interactable = false;
        }

        private void OnStopUpdatingAsset(DbModelType type, long id)
        {
            _button.interactable = true;
        }

        private void OnClick()
        {
            _levelManager.SwapCharacters();
        }

        private void OnCharacterSpawned(ICharacterAsset characterAsset)
        {
            RefreshButtonVisibility();
        }

        private void OnCharacterDestroyed()
        {
            RefreshButtonVisibility();
        }

        private void OnEventLoadingCompleted()
        {
            if (_levelManager.CurrentPlayMode == PlayMode.Preview) return;
            RefreshButtonVisibility();
        }

        private void RefreshButtonVisibility()
        {
            var makeButtonVisible = CharacterCount > 1;
            bodyCanvasGroup.interactable = makeButtonVisible;
            bodyCanvasGroup.blocksRaycasts = makeButtonVisible;
            bodyCanvasGroup.alpha = makeButtonVisible ? 1f : 0f;
        }
    }
}