using System.Threading.Tasks;
using Bridge.Models.VideoServer;
using Common.BridgeAdapter;
using Models;
using Modules.Notifications.NotificationItemModels;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Common;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Event = Models.Event;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public abstract class NotificationVideoItemView<T> : UserBasedNotificationItemView<T> where T : NotificationVideoItemModel
    {
        [SerializeField] protected LevelThumbnail _levelThumbnail;
        [SerializeField] private Button _videoButton;
        [SerializeField] private GameObject _contentNotAvailableOverlay;
        
        [Inject] private IBlockedAccountsManager _blockedAccountsManager;
        [Inject] protected VideoManager VideoManager;
        [Inject] private ILevelService _levelService;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected Video Video { get; private set; }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            _videoButton.onClick.AddListener(GoToVideo);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _videoButton.onClick.RemoveListener(GoToVideo);
            _levelThumbnail.CleanUp();
        }

        protected override async Task LoadContextData()
        {
            var tasks = new[] {base.LoadContextData(), LoadVideoThumbnail()};
            await Task.WhenAll(tasks);
        }
        
        protected virtual NotificationFeedArgs GetVideoArgs()
        {
            return new NotificationFeedArgs(VideoManager, PageManager, ContextData.VideoId, null);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void GoToVideo()
        {
            PageManager.MoveNext(PageId.Feed, GetVideoArgs());
        }

        private async Task LoadVideoThumbnail()
        {
            if (IsDestroyed || ContextData == null) return;

            var isNonLevelVideo = ContextData.LevelId == 0;
            if (isNonLevelVideo)
            {
                _levelThumbnail.DisplayNonLevelVideoThumbnail();
                return;
            }

            var isVideoAvailable = await GetVideo();
            CancellationSource?.Token.ThrowIfCancellationRequested();

            if (IsDestroyed || ContextData == null) return;
            _contentNotAvailableOverlay.SetActive(!isVideoAvailable);

            if (!isVideoAvailable || IsDestroyed || ContextData == null) return;

            var levelThumbnailInfo = GetLevelThumbnailInfo();
            _levelThumbnail.Initialize(levelThumbnailInfo);
        }

        private Level GetLevelThumbnailInfo()
        {
            var lvl = new Level
            {
                Id = ContextData.LevelId
            };
            lvl.Event.Add(new Event()
            {
                Id = ContextData.ThumbnailEventInfo.Id,
                Files = ContextData.ThumbnailEventInfo.Files
            });
            return lvl;
        }
        
        private async Task<bool> GetVideo()
        {
            var result = await Bridge.GetVideoAsync(ContextData.VideoId, CancellationSource.Token);

            if (result.IsSuccess)
            {
                Video = result.ResultObject;
            }

            return result.IsSuccess;
        }
    }
}