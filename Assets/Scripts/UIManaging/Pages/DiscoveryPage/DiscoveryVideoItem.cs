using System.Threading;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Common;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Pages.Feed.Core.Metrics;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.DiscoveryPage
{
    public class DiscoveryVideoItem : VideoListItem
    {
        [SerializeField] private Button _portraitProfileButton;
        [SerializeField] private UserPortraitView _userPortraitView;
        [SerializeField] private TMP_Text _userNameText;
        [SerializeField] private LikeMetricsView _likeMetricsView;
        
        [Inject] private PageManager _pageManager;

        private CancellationTokenSource _cancellationSource;

        protected override void OnEnable()
        {
            base.OnEnable();
            _portraitProfileButton.onClick.AddListener(OnOpenProfileButtonClicked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _portraitProfileButton.onClick.RemoveListener(OnOpenProfileButtonClicked);
            CancelThumbnailLoading();
        }

        public void Refresh()
        {
            _videoThumbnail.Initialize(new VideoThumbnailModel(ContextData.Video.ThumbnailUrl));
        }
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            CancelThumbnailLoading();
            _cancellationSource = new CancellationTokenSource();

            var userPortraitModel = new UserPortraitModel
            {
                Resolution = Resolution._128x128,
                UserGroupId = ContextData.Video.GroupId,
                UserMainCharacterId = ContextData.Video.Owner.MainCharacterId.Value,
                MainCharacterThumbnail = ContextData.Video.Owner.MainCharacterFiles,
            };

            _userPortraitView.Initialize(userPortraitModel);
            _userNameText.text = ContextData.Video.Owner.Nickname;

            var id = ContextData.Video.Id;
            var likes = ContextData.Video.KPI.Likes;
            var isLiked = ContextData.Video.LikedByCurrentUser;
            var toggleMetricsModel = new ToggleMetricsModel(id, ContextData.Video.GroupId, likes, isLiked);

            _likeMetricsView.Initialize(toggleMetricsModel);
            _videoThumbnail.Initialize(new VideoThumbnailModel(ContextData.Video.ThumbnailUrl));
        }

        private void OnOpenProfileButtonClicked()
        {
            OnDataForRemoteUserDownloaded(ContextData.Video.GroupId, ContextData.Video.Owner.Nickname);
        }

        private void OnDataForRemoteUserDownloaded(long userGroupId, string nickname)
        {
            _pageManager.MoveNext(PageId.UserProfile, new UserProfileArgs(userGroupId, nickname));
        }

        private void CancelThumbnailLoading()
        {
            if (_cancellationSource == null) return;
            _cancellationSource.Cancel();
            _cancellationSource = null;
        }
    }
}