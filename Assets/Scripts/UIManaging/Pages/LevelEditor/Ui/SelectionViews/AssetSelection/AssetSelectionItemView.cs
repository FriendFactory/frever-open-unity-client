using System;
using System.Threading;
using Abstract;
using Bridge;
using DG.Tweening;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Color = UnityEngine.Color;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    public abstract class AssetSelectionItemView : BaseContextDataButton<AssetSelectionItemModel>
    {
        [SerializeField] private float _loadingFadeDuration = 0.3f;
        [SerializeField] private float _greyOutAlpha = 0.35f;
        [SerializeField] private GameObject _selectedGameObject;
        [SerializeField] private CanvasGroup _loadingCircleCanvasGroup;
        [SerializeField] private GameObject _loadingCircle;
        [SerializeField] protected RawImage _rawImage;
        [SerializeField] protected GameObject _newIcon;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Color _backgroundColor = new Color(0.21176471f, 0.21176471f, 0.21568628f, 1);

        private float _transparency;
        protected ILevelManager LevelManager;
        protected long LastItemId;
        protected Type LastItemType;
        protected ViewState State { get; private set; }
        private bool _hasImage;

        protected float Transparency
        {
            get => _transparency;
            set
            {
                if (_rawImage.texture != null)
                {
                    _rawImage.SetAlpha(value);
                }
                
                _backgroundImage.SetAlpha(value);
                _transparency = value;
            }
        }

        private IBridge _bridge;
        private CancellationTokenSource _cancellationSource;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected Button Button => _button;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(IBridge bridge, ILevelManager levelManager)
        {
            _bridge = bridge;
            LevelManager = levelManager;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {
            _backgroundImage.color = _backgroundColor;
            SetState(ViewState.Active);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _button.onClick.AddListener(OnClicked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _rawImage.DOKill(true);
            _button.onClick.RemoveListener(OnClicked);
            CancelThumbnailLoading();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CleanupRawImage();
            _loadingCircleCanvasGroup.DOKill();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            RefreshSelectionGameObjects();
            _loadingCircleCanvasGroup.alpha = 0f;
            SubscribeToEvents();
            if (GetCurrentItemType() == DbModelType.Outfit)
            {
                if (LevelManager.IsLoadingAssetsOfType(DbModelType.Character))
                {
                    OnStartUpdatingAsset(DbModelType.Character, -1);
                }
            }

            if (GetCurrentItemType() == DbModelType.Character)
            {
                if (LevelManager.IsLoadingAssetsOfType(DbModelType.Outfit))
                {
                    OnStartUpdatingAsset(DbModelType.Outfit, -1);
                }
            }
            
            SetIsNew();
            gameObject.SetActive(!ContextData.HideOnInitialize);
        }

        protected virtual void OnStopUpdatingAsset(DbModelType type, long id)
        {
            if (!ShouldBlockInteractionForType(type)) return;

            OnLoadingFinished();
            HideLoadingCircle(id);
            
            LevelManager.StartUpdatingAsset -= OnStartUpdatingAsset;
            LevelManager.StopUpdatingAsset -= OnStopUpdatingAsset;
            LevelManager.SetLocationChangeFinished -= OnSetLocationChangeFinished;
            LevelManager.CharactersOutfitsUpdated -= OnCharactersOutfitsUpdated;
            LevelManager.CharacterReplaced -= OnCharacterReplaced;

            LevelManager.StartUpdatingAsset += OnStartUpdatingAsset;
            LevelManager.StopUpdatingAsset += OnStopUpdatingAsset;
            LevelManager.SetLocationChangeFinished += OnSetLocationChangeFinished;
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            UnsubscribeFromEvents();
        }

        protected async void DownloadThumbnail()
        {
            CancelThumbnailLoading();
            _cancellationSource = new CancellationTokenSource();
            var id = ContextData.ThumbnailOwner.Id;
            
            var result = await _bridge.GetThumbnailAsync(ContextData.ThumbnailOwner, ContextData.Resolution, true, _cancellationSource.Token);
            if (result != null && result.IsSuccess)
            {
                _hasImage = true;
                OnThumbnailLoaded(id, result.Object);
            }
            else
            {
                Debug.LogWarning(result?.ErrorMessage);
            }

            _cancellationSource = null;
        }

        protected abstract void OnThumbnailLoaded(long id, object downloadedTexture);

        protected virtual void RefreshSelectionGameObjects()
        {
            _selectedGameObject.SetActive(ContextData.IsSelected);
        }

        protected void ShowRawImage()
        {
            _rawImage.DOKill();
            _rawImage.DOColor(Color.white.SetAlpha(Transparency), 0.2f).SetUpdate(true);
        }

        protected void CleanupRawImage()
        {
            _rawImage.DOKill();
            _rawImage.color = Color.white.SetAlpha(0f);
            if (_hasImage && _rawImage.texture != null)
            {
                Destroy(_rawImage.texture);
                _rawImage.texture = null;
                _hasImage = false;
            }
        }

        protected bool IsSameItem()
        {
            return LastItemId == ContextData.ItemId && LastItemType == ContextData.GetType();
        }
        
        protected void OnCharactersOutfitsUpdated()
        {
            HideLoadingCircle();
            OnLoadingFinished();
        }

        protected virtual void OnClicked()
        {
            if (LevelManager.IsLoadingAssetsOfType(GetCurrentItemType())) return;
            ContextData.TrySetIsSelectedByUser();
        }

        protected DbModelType GetCurrentItemType()
        {
            return ContextData.RepresentedObject.GetModelType();
        }

        protected void SetState(ViewState state)
        {
            State = state;
            switch (state)
            {
                case ViewState.Active:
                    Transparency = 1;
                    break;
                case ViewState.GreyOut:
                    Transparency = _greyOutAlpha;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnCharacterReplaced(ICharacterAsset characterAsset)
        {
            HideLoadingCircle();
            OnLoadingFinished();
        }
        
        private void OnSetLocationChangeFinished(ISetLocationAsset setLocationAsset)
        {
            OnStopUpdatingAsset(DbModelType.SetLocation, setLocationAsset.RepresentedModel.Id);
        }

        private bool ShouldBlockInteractionForType(DbModelType type)
        {
            var currentType = GetCurrentItemType();
            return currentType == DbModelType.CharacterSpawnPosition && type == DbModelType.SetLocation ||
                   currentType == DbModelType.Outfit && type == DbModelType.Character ||
                   currentType == DbModelType.Character && type == DbModelType.Outfit ||
                   currentType == type;
        }
        
        private void OnStartUpdatingAsset(DbModelType type, long id)
        {
            if (!ShouldBlockInteractionForType(type)) return;
            
            ShowLoadingCircle(id);
            if(type == DbModelType.Character) _button.interactable = false;

            LevelManager.StartUpdatingAsset -= OnStartUpdatingAsset;
        }

        private void OnLoadingFinished()
        {
            if (IsDestroyed) return;
            
            _button.interactable = true;
        }

        private void OnModelApplied(AssetSelectionItemModel model)
        {
            HideLoadingCircle(model.ItemId);
        }

        private void ShowLoadingCircle(long id)
        {
            if (id != ContextData.ItemId) return;
            
            var isAssetAlreadyLoaded = LevelManager.IsAssetLoaded(ContextData.RepresentedObject);

            if (isAssetAlreadyLoaded) return;
            
            _loadingCircleCanvasGroup.alpha = 0f;
            _loadingCircle.SetActive(true);
            _loadingCircleCanvasGroup.DOKill();
            _loadingCircleCanvasGroup.DOFade(1f, _loadingFadeDuration).SetUpdate(true);
        }
        
        private void HideLoadingCircle(long id)
        {
            if (id != ContextData.ItemId) return;
            HideLoadingCircle();
        }

        private void HideLoadingCircle()
        {
            if (!_loadingCircle.activeInHierarchy)
            {
                return;
            }
            
            _loadingCircleCanvasGroup.DOKill();
            _loadingCircleCanvasGroup.DOFade(0f, _loadingFadeDuration).SetUpdate(true).OnComplete(() => _loadingCircle.SetActive(false));
        }

        private void SubscribeToEvents()
        {
            UnsubscribeFromEvents();
            if (GetCurrentItemType() == DbModelType.Outfit)
            {
                LevelManager.CharactersOutfitsUpdated += OnCharactersOutfitsUpdated;
                LevelManager.CharacterReplaced += OnCharacterReplaced;
            }
            
            LevelManager.SetLocationChangeFinished += OnSetLocationChangeFinished;
            LevelManager.StartUpdatingAsset += OnStartUpdatingAsset;
            LevelManager.StopUpdatingAsset += OnStopUpdatingAsset;
            ContextData.OnIsSelectedChangedEvent += OnIsSelectedChanged;
            ContextData.OnIsSelectedChangedSilentEvent += OnIsSelectedChangedSilent;
            ContextData.OnModelApplied += OnModelApplied;
        }

        private void UnsubscribeFromEvents()
        {
            LevelManager.CharacterReplaced -= OnCharacterReplaced;
            LevelManager.StartUpdatingAsset -= OnStartUpdatingAsset;
            LevelManager.AssetUpdateStarted -= OnStartUpdatingAsset;
            LevelManager.CharactersOutfitsUpdated -= OnCharactersOutfitsUpdated;
            LevelManager.SetLocationChangeFinished -= OnSetLocationChangeFinished;
            LevelManager.StopUpdatingAsset -= OnStopUpdatingAsset;
            ContextData.OnIsSelectedChangedEvent -= OnIsSelectedChanged;
            ContextData.OnIsSelectedChangedSilentEvent -= OnIsSelectedChangedSilent;
            ContextData.OnModelApplied -= OnModelApplied;
        }

        private void OnIsSelectedChanged(AssetSelectionItemModel model)
        {
            ShowLoadingCircle(model.ItemId);
        }

        private void OnIsSelectedChangedSilent(AssetSelectionItemModel model)
        {
            RefreshSelectionGameObjects();
        }

        private void SetIsNew()
        {
            if (!_newIcon) return;
            _newIcon.SetActive(ContextData.IsNew);
        }

        private void CancelThumbnailLoading()
        {
            if (_cancellationSource == null) return;
            _cancellationSource.Cancel();
            _cancellationSource = null;
        }
        
        protected enum ViewState
        {
            Active,
            GreyOut
        }
    }
}