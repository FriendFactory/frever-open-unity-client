using Common;
using Modules.InputHandling;
using Navigation.Args;
using Navigation.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using Modules.AssetsManaging.Loaders;
using UnityEngine;
using Zenject;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.AssetsStoraging.Core;
using UIManaging.PopupSystem;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;


namespace UIManaging.Pages.VideoMessage
{
    internal sealed class VideoMessagePage : GenericPage<VideoMessagePageArgs>
    {
        [SerializeField] private VideoMessagePresenter _presenter;

        [Inject] private IInputManager _inputManager;
        [Inject] private ILevelManager _levelManager;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private ISetLocationBackgroundInMemoryCacheControl _setLocationBackgroundInMemoryCache;
        
        public override PageId Id => PageId.VideoMessage;

        protected override void OnInit(PageManager pageManager)
        {
        }

        protected override async void OnDisplayStart(VideoMessagePageArgs args)
        {
            if (!_popupManagerHelper.IsLoadingOverlayShowing)
            {
                _popupManagerHelper.ShowLoadingOverlay(Constants.LoadingPopupMessages.VIDEO_MESSAGE_HEADER);
            }
           
            _setLocationBackgroundInMemoryCache.SetCapacity(Constants.VideoMessage.BACKGROUNDS_CACHE_CAPACITY);
            await SetupLevelAsync();
            await _presenter.Initialize(_levelManager.CurrentLevel, args.OnMoveBackRequested, args.OnMoveNext, args.OnLevelCreationRequested, args.OnNonLevelVideoUploadRequested);
            _popupManagerHelper.HideLoadingOverlay();

            _inputManager.Enable(true);
            base.OnDisplayStart(args);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            ClearBackgroundsInMemoryCache();
            _inputManager.Enable(false);
            _levelManager.StopCurrentPlayMode();
            _presenter.Cleanup();
            base.OnHidingBegin(onComplete);
        }

        private void ClearBackgroundsInMemoryCache()
        {
            var selectedBackground = _levelManager.TargetEvent.GetSetLocationBackgroundId();
            if (selectedBackground.HasValue)
            {
                _setLocationBackgroundInMemoryCache.Clear(selectedBackground.Value);
            }
            else
            {
                _setLocationBackgroundInMemoryCache.Clear();
            }
        }

        private async Task SetupLevelAsync()
        {
            _levelManager.Initialize(_dataFetcher.MetadataStartPack);
            _levelManager.CurrentLevel = OpenPageArgs.Level; 

            var isReady = false;
            _levelManager.EventLoadingCompleted += OnLevelLoaded;

            var targetEvent = OpenPageArgs.Level.Event.First();
            _levelManager.PlayEvent(PlayMode.PreviewLoop, targetEvent);
            _levelManager.SetFaceTracking(false);
            
            while (!isReady)
            {
                await Task.Delay(25);
            }

            void OnLevelLoaded()
            {
                isReady = true;
                _levelManager.EventLoadingCompleted -= OnLevelLoaded;
            }
        }
    }
}
