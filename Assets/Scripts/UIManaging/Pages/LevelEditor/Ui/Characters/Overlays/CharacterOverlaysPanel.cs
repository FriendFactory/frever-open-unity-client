using System.Collections.Generic;
using Common;
using Common.Abstract;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.Characters
{
    internal sealed class CharacterOverlaysPanel : BaseContextlessPanel
    {
        private const float SORTING_INTERVAL = 0.5f;

        [SerializeField] private RectTransform _renderViewPort;
        [SerializeField] private CharacterOverlay _overlayPrefab;
        [Space]
        [SerializeField] private RectTransform _topAnchor;
        [SerializeField] private float _topAnchorOffset;
        [SerializeField] private RectTransform _bottomAnchor;
        [SerializeField] private float _bottomAnchorOffset;

        private IAssetManager _assetManager;
        private ILevelManager _levelManager;
        private LevelEditorPageModel _pageModel;
        private RectTransform _rectTransform;
        private float _sortingTimer;

        private readonly List<CharacterOverlay> _overlays = new();
        private readonly Vector3[] _corners = new Vector3[4];

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(IAssetManager assetManager, ILevelManager levelManager, LevelEditorPageModel pageModel)
        {
            _assetManager = assetManager;
            _levelManager = levelManager;
            _pageModel = pageModel;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Update()
        {
            _sortingTimer += Time.deltaTime;
            if (_sortingTimer < SORTING_INTERVAL) return;

            _sortingTimer = 0;
            SortOverlaysByDistance();
        }
        
        //---------------------------------------------------------------------
        // Protected 
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            UnSubscribeFromEvents();
            SubscribeToEvents();
            CoroutineSource.Instance.ExecuteWithFramesDelay(5, AdjustSafeArea);
        }

        protected override void BeforeCleanUp()
        {
            UnSubscribeFromEvents();
            CleanUpOverlays();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void AdjustSafeArea()
        {
            var parent = (RectTransform) RectTransform.parent;

            _topAnchor.GetWorldCorners(_corners);
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, _corners[0]);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, null, out var localPoint);
            RectTransform.SetTop(parent.GetHeight() - localPoint.y + _topAnchorOffset);

            _bottomAnchor.GetWorldCorners(_corners);
            screenPoint = RectTransformUtility.WorldToScreenPoint(null, _corners[0]);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, null, out localPoint);
            RectTransform.SetBottom(localPoint.y + _bottomAnchorOffset);
        }

        private void UpdateOverlays()
        {
            HideAllOverlays();
            CleanUpOverlays();

            var characters = _levelManager.TargetEvent.GetOrderedCharacterControllers();

            for (var i = 0; i < characters.Length; i++)
            {
                CharacterOverlay overlay;

                if (_overlays.Count > i)
                {
                    overlay = _overlays[i];
                    overlay.SetActive(true);
                }
                else
                {
                    overlay = Instantiate(_overlayPrefab, transform);
                    _overlays.Add(overlay);
                }

                var asset = GetCharacterAssetIfLoaded(characters[i].CharacterId);
                var location = _levelManager.GetCurrentActiveSetLocationAsset();

                overlay.Init(location, asset, i, _renderViewPort);
                overlay.Update();
            }
        }

        private void HideAllOverlays()
        {
            foreach (var overlay in _overlays)
            {
                if (overlay.IsDestroyed()) return;
                overlay.SetActive(false);
            }
        }
        
        private void CleanUpOverlays()
        {
            foreach (var overlay in _overlays)
            {
                overlay.CleanUp();
            }
        }

        private void SortOverlaysByDistance()
        {
            _overlays.Sort((a, b) => b.Distance.CompareTo(a.Distance));

            for (var i = 0; i < _overlays.Count; i++)
            {
                _overlays[i].transform.SetSiblingIndex(i);
            }
        }

        private ICharacterAsset GetCharacterAssetIfLoaded(long characterId)
        {
            return _assetManager.GetActiveAssetOfType<ICharacterAsset>(characterId);
        }

        private void SubscribeToEvents()
        {
            _levelManager.EventStarted += OnEventStart;
            _levelManager.CharacterSpawned += OnCharacterSpawned;
            _levelManager.CharacterDestroyed += OnCharacterDestroyed;
            _levelManager.LevelPreviewCompleted += OnLevelPreviewCompleted;
            _levelManager.PreviewCancelled += OnLevelPreviewCompleted;
            _levelManager.TemplateApplyingCompleted += OnTemplateApplyingCompleted;
            _levelManager.SetLocationChangeFinished += OnLocationChangeFinished;
            _levelManager.ShufflingBegun += OnShufflingBegun;
            _levelManager.ShufflingDone += OnShufflingFinished;

            _pageModel.StateChanged += UpdateOverlaysAccordingToState;
        }

        private void UnSubscribeFromEvents()
        {
            _levelManager.EventStarted -= OnEventStart;
            _levelManager.CharacterSpawned -= OnCharacterSpawned;
            _levelManager.CharacterDestroyed -= OnCharacterDestroyed;
            _levelManager.LevelPreviewCompleted -= OnLevelPreviewCompleted;
            _levelManager.PreviewCancelled -= OnLevelPreviewCompleted;
            _levelManager.TemplateApplyingCompleted -= OnTemplateApplyingCompleted;
            _levelManager.SetLocationChangeFinished -= OnLocationChangeFinished;
            _levelManager.ShufflingBegun -= OnShufflingBegun;
            _levelManager.ShufflingDone -= OnShufflingFinished;

            _pageModel.StateChanged -= UpdateOverlaysAccordingToState;
        }

        private void UpdateOverlaysAccordingToState(LevelEditorState state)
        {
            switch (state)
            {
                case LevelEditorState.Recording
                  or LevelEditorState.AssetSelection
                  or LevelEditorState.PurchasableAssetSelection
                  or LevelEditorState.Dressing
                  or LevelEditorState.Preview
                  or LevelEditorState.FocusCharacterSelection:
                    HideAllOverlays();
                    break;
                default:
                    UpdateOverlays();
                    break;
            }
        }

        private void UpdateOverlaysAccordingToState()
        {
            var state = _pageModel.EditorState;
            UpdateOverlaysAccordingToState(state);
        }

        private void OnEventStart()
        {
            UpdateOverlaysAccordingToState();
        }

        private void OnCharacterSpawned(ICharacterAsset characterAsset)
        {
            UpdateOverlaysAccordingToState();
        }

        private void OnCharacterDestroyed()
        {
            UpdateOverlaysAccordingToState();
        }

        private void OnLevelPreviewCompleted()
        {
            UpdateOverlaysAccordingToState();
        }

        private void OnTemplateApplyingCompleted()
        {
            UpdateOverlaysAccordingToState();
        }

        private void OnLocationChangeFinished(ISetLocationAsset setLocation)
        {
            UpdateOverlaysAccordingToState();
        }

        private void OnShufflingBegun()
        {
            HideAllOverlays();
        }

        private void OnShufflingFinished()
        {
            UpdateOverlaysAccordingToState();
        }
    }
}