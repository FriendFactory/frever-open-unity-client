using System;
using System.Linq;
using Bridge;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.AssetChangers.SpawnFormations;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.Characters
{
    internal sealed class CharacterFormationButton : MonoBehaviour
    {
        [SerializeField] private Image _thumbnail;
        [SerializeField] private CanvasGroup bodyCanvasGroup;
        
        private Button _button;
        private ILevelManager _levelManager;
        private IBridge _bridge;
        private ISpawnFormationProvider _spawnFormationProvider;

        private CharacterSpawnPositionFormation _currentFormation;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private int CharacterCount => _levelManager.TargetEvent?.CharacterController.Count ?? 1;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(ILevelManager levelManager, IBridge bridge, ISpawnFormationProvider spawnFormationProvider)
        {
            _levelManager = levelManager;
            _bridge = bridge;
            _spawnFormationProvider = spawnFormationProvider;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _levelManager.CharacterSpawned += OnCharacterSpawned;
            _levelManager.CharacterDestroyed += OnCharacterDestroyed;
            _levelManager.EventLoadingCompleted += OnEventLoadingCompleted;
            _levelManager.SpawnFormationSetup += UpdateThumbnails;
            _levelManager.StartUpdatingAsset += OnStartUpdatingAsset;
            _levelManager.StopUpdatingAsset += OnStopUpdatingAsset;

            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);

            RefreshThumbnail();
        }

        private void OnEnable()
        {
            RefreshButtonVisibility();  
            RefreshThumbnail();
        }

        private void OnDestroy()
        {
            _levelManager.CharacterSpawned -= OnCharacterSpawned;
            _levelManager.CharacterDestroyed -= OnCharacterDestroyed;
            _levelManager.EventLoadingCompleted -= OnEventLoadingCompleted;
            _levelManager.SpawnFormationSetup -= UpdateThumbnails;
            _levelManager.StartUpdatingAsset -= OnStartUpdatingAsset;
            _levelManager.StopUpdatingAsset -= OnStopUpdatingAsset;

            _button.onClick.RemoveListener(OnClick);
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
            RefreshButtonVisibility();
        }

        private void OnClick()
        {
            var availableFormations = GetAvailableSpawnFormations();
            var currentIndex = Array.IndexOf(availableFormations, _currentFormation);
            var nextIndex = (currentIndex < availableFormations.Length - 1) ? currentIndex + 1 : 0;

            _currentFormation = availableFormations[nextIndex];
            ApplyFormation(_currentFormation);
        }

        private void ApplyFormation(CharacterSpawnPositionFormation newFormation)
        {
            _levelManager.ApplyFormation(newFormation);
        }

        private async void UpdateThumbnails(long? formationId)
        {
            if (!formationId.HasValue) return;
            
            var availableFormations = GetAvailableSpawnFormations();

            var newFormation = availableFormations?.FirstOrDefault(formation => formation.Id == formationId);

            if (newFormation == null) return;
            _currentFormation = newFormation;
            
            var result = await _bridge.GetThumbnailAsync(newFormation, Resolution._128x128);
            if (result.IsSuccess)
            {
                OnThumbnailLoaded(result.Object);
            }
            else
            {
                Debug.LogWarning(result.ErrorMessage);
            }
        }

        private void OnThumbnailLoaded(object obj)
        {
            if (obj is Texture2D tex)
            {
                if(_thumbnail == null) return;
                _thumbnail.sprite = tex.ToSprite();
            }
            else
            {
                Debug.LogWarning("Wrong thumbnail format");
            }
        }
        
        private void OnCharacterSpawned(ICharacterAsset characterAsset)
        {
            RefreshButtonVisibility();
            RefreshThumbnail();
        }

        private void OnCharacterDestroyed()
        {
            RefreshButtonVisibility();
            RefreshThumbnail();
        }

        private void OnEventLoadingCompleted()
        {
            if (_levelManager.CurrentPlayMode == PlayMode.Preview) return;
            RefreshButtonVisibility();
            RefreshThumbnail();
        }

        private void RefreshButtonVisibility()
        {
            var makeButtonVisible = CharacterCount > 1 && IsTargetSpawnPositionSupportFormationChanges();
            bodyCanvasGroup.interactable = makeButtonVisible;
            bodyCanvasGroup.blocksRaycasts = makeButtonVisible;
            bodyCanvasGroup.alpha = makeButtonVisible ? 1f : 0f;
        }
        
        private CharacterSpawnPositionFormation[] GetAvailableSpawnFormations()
        {
            return _spawnFormationProvider.GetSpawnPositionFormations(CharacterCount)
                .OrderBy(x => x.SortOrder).ToArray();
        }

        private bool IsTargetSpawnPositionSupportFormationChanges()
        {
            var focusedSpawnPositionId = _levelManager.TargetEvent.CharacterSpawnPositionId;
            var focusedSpawnPositionModel =
                _levelManager.TargetEvent.GetSetLocation().GetSpawnPosition(focusedSpawnPositionId);
            return focusedSpawnPositionModel.SupportFormation;
        }
        
        private void RefreshThumbnail()
        {
            UpdateThumbnails(_levelManager.TargetEvent.CharacterSpawnPositionFormationId);
        }
    }
}