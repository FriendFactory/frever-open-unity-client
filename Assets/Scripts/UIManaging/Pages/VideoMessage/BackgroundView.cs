using System;
using System.Threading;
using Bridge.Models.ClientServer.Assets;
using EnhancedUI.EnhancedScroller;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.VideoMessage
{
    internal sealed class BackgroundView: EnhancedScrollerCellView
    {
        [SerializeField] private Button _button;
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private GameObject _selectionOutline;
        [SerializeField] private Image _aiIcon;

        private CancellationTokenSource _cancellationTokenSource;
        
        private IBackgroundOption _backgroundModel;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public Texture2D Texture
        {
            get => _rawImage.texture as Texture2D;
            private set => _rawImage.texture = value;
        }

        public long BackgroundId => _backgroundModel.Id;
        public event Action<IBackgroundOption> Clicked;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.CancelAndDispose();
            _cancellationTokenSource = null;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void Init(IBackgroundOption background, SetLocationBackgroundThumbnailProvider thumbnailProvider)
        {
            _cancellationTokenSource?.CancelAndDispose();
            _cancellationTokenSource = new CancellationTokenSource();
            
            _backgroundModel = background;
            Texture = await thumbnailProvider.GetThumbnailAsync(background, _cancellationTokenSource.Token);

            _aiIcon.gameObject.SetActive(background.Type == BackgroundOptionType.GenerationSettings);
        }

        public void SetSelected(bool isSelected)
        {
            _selectionOutline.SetActive(isSelected);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnClicked()
        {
            Clicked?.Invoke(_backgroundModel);
        }
    }
}