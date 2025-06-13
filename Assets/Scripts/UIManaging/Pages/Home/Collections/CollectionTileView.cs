using Abstract;
using Bridge.Models.ClientServer.ThemeCollection;
using DG.Tweening;
using Extensions;
using Modules.ThemeCollection;
using UIManaging.Common;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Home
{
    internal sealed class CollectionTileView : BaseContextDataView<ThemeCollectionInfo>
    {
        private const float APPEAR_DURATION = 0.25f;
        
        [SerializeField] private Button _button;
        [SerializeField] private RawImage _thumbnailImage;
        [SerializeField] private ThumbnailLoader _thumbnailLoader;

        [Inject] private IThemeCollectionService _collectionService;
        
        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnEnable()
        {
            _button.interactable = true;
        }

        protected override void OnInitialized()
        {
            if (ContextData is null)
            {
                InitEmpty();
                return;
            }
            _thumbnailLoader.Initialize(ContextData);
            _thumbnailLoader.OnThumbnailReady += OnThumbnailReady;
            _thumbnailImage.color = _thumbnailImage.color.SetAlpha(0);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();

            if (ContextData is null)
            {
                return;
            }
            _thumbnailLoader.OnThumbnailReady -= OnThumbnailReady;
            _thumbnailLoader.CleanUp();
        }

        private void OnButtonClick()
        {
            _collectionService.ShowCollection(ContextData.Id);
            _button.interactable = false;
        }

        private void OnThumbnailReady()
        {
            DOTween.To(() => _thumbnailImage.color.a, a => _thumbnailImage.color = _thumbnailImage.color.SetAlpha(a), 1, APPEAR_DURATION)
                   .SetEase(Ease.InOutCubic);
        }

        private void InitEmpty()
        {
            _button.interactable = false;
            foreach (Transform child in transform)
            {
                child.SetActive(false);
            }
        }
    }
}