using System;
using System.Threading;
using Abstract;
using Bridge;
using Common;
using DG.Tweening;
using Extensions;
using JetBrains.Annotations;
using OldMoatGames;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.ShoppingCart
{
    [RequireComponent(typeof(Button), typeof(CanvasGroup))]
    internal sealed class ShoppingCartAssetView : BaseContextDataView<ShoppingCartAssetModel>
    {
        [SerializeField] private TextMeshProUGUI _ordinalText;
        [SerializeField] private RawImage _thumbnail;
        [SerializeField] private AnimatedGifPlayer _gifPlayer;
        [Space]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private Image _priceIcon;
        [Space]
        [SerializeField] private Sprite _softCurrencyIcon;
        [SerializeField] private Sprite _hardCurrencyIcon;

        private Button _assetButton;
        private CanvasGroup _canvasGroup;

        private IBridge _bridge;
        private CancellationTokenSource _cancellationSource;

        private int _ordinal;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<ShoppingCartItemModel> ItemClicked;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(IBridge bridge)
        {
            _bridge = bridge;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _assetButton = GetComponent<Button>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            _assetButton.onClick.AddListener(OnItemClicked);
        }

        private void OnDisable()
        {
            _assetButton.onClick.RemoveListener(OnItemClicked);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize(ShoppingCartAssetModel model, int ordinal)
        {
            _ordinal = ordinal;
            base.Initialize(model);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _canvasGroup.alpha = (ContextData.IsConfirmed) ? 1f : 0.5f;

            _ordinalText.text = _ordinal.ToString();
            _nameText.text = ContextData.Name;

            if (ContextData.HardPrice != null)
            {
                _priceText.text = ContextData.HardPrice.ToString();
                _priceIcon.sprite = _hardCurrencyIcon;
            }
            else
            {
                _priceText.text = ContextData.SoftPrice.ToString();
                _priceIcon.sprite = _softCurrencyIcon;
            }

            HideRawImage();
            InitGifPlayer();
            DownloadThumbnail();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            ItemClicked = null;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void DownloadThumbnail()
        {
            CancelThumbnailLoading();
            _cancellationSource = new CancellationTokenSource();
            var id = ContextData.ThumbnailOwner.Id;

            var result = await _bridge.GetThumbnailAsync(ContextData.ThumbnailOwner, ContextData.Resolution, true, _cancellationSource.Token);
            if (result != null && result.IsSuccess)
            {
                OnThumbnailLoaded(id, result.Object);
            }
            else
            {
                Debug.LogWarning(result?.ErrorMessage);
            }

            _cancellationSource = null;
        }

        private void CancelThumbnailLoading()
        {
            if (_cancellationSource == null) return;
            _cancellationSource.Cancel();
            _cancellationSource = null;
        }

        private void OnThumbnailLoaded(long id, object downloadedTexture)
        {
            if (IsDestroyed || !gameObject.activeInHierarchy || ContextData.ThumbnailOwner.Id != id) return;

            switch (downloadedTexture)
            {
                case Texture2D texture:
                    _thumbnail.texture = texture;
                    ShowRawImage();
                    break;
                case byte[] byteArray:
                    _gifPlayer.GifBytes = byteArray;
                    _gifPlayer.OnReady -= DisplayGif;
                    _gifPlayer.OnReady += DisplayGif;
                    _gifPlayer.Init();
                    break;
            }
        }

        private void OnItemClicked()
        {
            ItemClicked?.Invoke(ContextData);
        }

        private void ShowRawImage()
        {
            _thumbnail.DOKill();
            _thumbnail.DOColor(Color.white, 0.2f).SetUpdate(true);
        }

        private void HideRawImage()
        {
            _thumbnail.DOKill();
            _thumbnail.color = Color.white.SetAlpha(0f);
            _thumbnail.texture = null;
        }

        private void InitGifPlayer()
        {
            _gifPlayer.OnReady -= DisplayGif;
            _gifPlayer.StopAllCoroutines();
            _gifPlayer.Path = GifPath.PersistentDataPath;

            if (_gifPlayer.State == GifPlayerState.Playing)
            {
                _gifPlayer.Pause();
            }
        }

        private void DisplayGif()
        {
            _gifPlayer.OnReady -= DisplayGif;
            if (!gameObject.activeInHierarchy) return;
            CoroutineSource.Instance.ExecuteWithFrameDelay(ShowRawImage);
        }
    }
}