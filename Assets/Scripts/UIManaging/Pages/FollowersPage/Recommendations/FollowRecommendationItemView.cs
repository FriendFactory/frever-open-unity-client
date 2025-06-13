using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Recommendations;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Pages.FollowersPage.UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.FollowersPage.Recommendations
{
    internal class FollowRecommendationItemView: BaseContextDataView<FollowRecommendation>
    {
        [SerializeField] private UserPortrait _recommendeeThumbnail;
        [SerializeField] private FollowUserButton _followUserButton;
        [SerializeField] private Button _profileButton;
        [SerializeField] private FollowRecommendationTextHandler _recommendationTextHandler;

        [Inject] private IBridge _bridge;
        [Inject] private PageManager _pageManager;

        private CancellationTokenSource _tokenSource;

        protected override async void OnInitialized()
        {
            _tokenSource = new CancellationTokenSource();

            await LoadRecommendeeThumbnailAsync(_tokenSource.Token);
            await InitializeFollowButtonAsync(_tokenSource.Token);
            
            if (_tokenSource.IsCancellationRequested) return; 
            
            _recommendationTextHandler.UpdateText(ContextData);
            
            _profileButton.onClick.AddListener(GoToRemoteUserProfile);
        }

        protected override void BeforeCleanup()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            
            _recommendeeThumbnail.CleanUp();
            _recommendationTextHandler.Clear();
            
            _profileButton.onClick.RemoveListener(GoToRemoteUserProfile);
        }

        private async Task InitializeFollowButtonAsync(CancellationToken token)
        {
            var result = await _bridge.GetProfile(ContextData.Group.Id, token);
            
            if (result.IsRequestCanceled) return;
            
            if (result.IsError || result.IsRequestCanceled)
            {
                Debug.LogError($"[{GetType().Name}] Failed to get user profile # {result.ErrorMessage}");
                return;
            }

            var followButtonArgs = new FollowUserButtonArgs(result.Profile);
            
            _followUserButton.Initialize(followButtonArgs);
        }

        private async Task LoadRecommendeeThumbnailAsync(CancellationToken token)
        {
            var userPortraitModel = new UserPortraitModel()
            {
                UserGroupId = ContextData.Group.Id,
                UserMainCharacterId = ContextData.Group.MainCharacterId ?? 0,
                MainCharacterThumbnail = ContextData.Group.MainCharacterFiles,
                Resolution = Resolution._128x128
            };
            
            await _recommendeeThumbnail.InitializeAsync(userPortraitModel, token);
            _recommendeeThumbnail.ShowContent();
        }

        private void GoToRemoteUserProfile()
        {
            _pageManager.MoveNext(PageId.UserProfile, new UserProfileArgs(ContextData.Group.Id, ContextData.Group.Nickname));
        }
    }
}