using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bridge;
using Common;
using Modules.AssetsStoraging.Core;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;
using Sirenix.OdinInspector;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.SnackBarSystem;
using UnityEngine;
using VoxelBusters.EssentialKit;
using Zenject;

namespace Modules.DeepLinking
{
    public class DeepLinksHandler : MonoBehaviour
    {
        private const string PROFILE_LINK_REGEX = @"^https://web\.frever-api\.com/@([a-zA-Z0-9._@+-]+)/?([a-zA-Z0-9]+)?$";
        private const string SEASON_LINK_REGEX = @"^https://web\.frever-api\.com/season/([0-9]+)$";

        private const int LINK_HANDLING_TIMEOUT = 180000;
        private const int LINK_HANDLING_DELAY = 1000;

        [Inject] private IBridge _bridge;
        [Inject] private PageManager _pageManager;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private VideoManager _videoManager;
        [Inject] private IDataFetcher _dataFetcher;
        
        private readonly HashSet<PageId> _startPages = new HashSet<PageId>()
            { PageId.Feed, PageId.GamifiedFeed, PageId.HomePage, PageId.HomePageSimple };

        private string _lastLink;
        private bool _startPageLoaded;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            DeepLinkServices.OnUniversalLinkOpen += OnUniversalLinkOpen;
            _pageManager.PageDisplayed += OnPageDisplayed;
        }

        private void OnDisable()
        {
            DeepLinkServices.OnUniversalLinkOpen -= OnUniversalLinkOpen;
            _pageManager.PageDisplayed -= OnPageDisplayed;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnUniversalLinkOpen(DeepLinkServicesDynamicLinkOpenResult result)
        {
            OnUniversalLinkOpen(result.RawUrlString);
        }

        private void OnUniversalLinkOpen(string link)
        {
            _lastLink = link;
            TryToHandleDeepLink();
        }

        private async void TryToHandleDeepLink()
        {
            var isLinkHandled = false;
            var timeout = LINK_HANDLING_TIMEOUT;

            do
            {
                if (string.IsNullOrEmpty(_lastLink)) break;

                if (_startPageLoaded)
                {
                    CheckProfileLink(_lastLink);
                    CheckVideoLink(_lastLink);
                    CheckSeasonLink(_lastLink);

                    isLinkHandled = true;
                }
                else
                {
                    await Task.Delay(LINK_HANDLING_DELAY);
                    timeout -= LINK_HANDLING_DELAY;
                }
            }
            while (!isLinkHandled && timeout > 0);

            _lastLink = null;
        }

        private async void CheckProfileLink(string link)
        {
            var profileRegex = new Regex(PROFILE_LINK_REGEX);
            var match = profileRegex.Match(link);

            if (!match.Success) return;

            var userName = match.Groups[1].Value;
            var result = await _bridge.GetPublicProfileFor(userName);

            if (result.IsSuccess)
            {
                _pageManager.MoveNext(PageId.UserProfile, new UserProfileArgs(result.Profile.MainGroupId, userName));
            }
            else if (result.IsError)
            {
                _snackBarHelper.ShowInformationSnackBar("The requested profile could not be not found.", 2);
                Debug.LogError($"Failed to get profile for \"{userName}\". Reason: {result.ErrorMessage}");
            }
            else if (result.IsRequestCanceled)
            {
                Debug.Log($"Request to get profile for \"{userName}\" was canceled.");
            }
        }

        private async void CheckVideoLink(string link)
        {
            var videoRegex = new Regex(Constants.Regexes.VIDEO_LINK);
            var match = videoRegex.Match(link);

            if (!match.Success) return;

            var videoGuid = match.Groups[2].Value;
            var result = await _bridge.GetVideoShareInfo(videoGuid);

            if (result.IsSuccess)
            {
                var videoId = result.Model.VideoId;
                var userName = result.Model.GroupNickName;
                await OpenVideoInFeed(videoId, userName);
            }
            else if (result.IsError)
            {
                _snackBarHelper.ShowInformationSnackBar("The requested video could not be not found.", 2);
                Debug.LogError($"Failed to get video \"{videoGuid}\". Reason: {result.ErrorMessage}");
            }
            else if (result.IsRequestCanceled)
            {
                Debug.Log($"Request to get video \"{videoGuid}\" was canceled.");
            }
        }

        private async Task OpenVideoInFeed(long videoId ,string userName)
        {
            var result = await _bridge.GetVideoAsync(videoId);
            if (result.IsSuccess)
            {
                var video = result.ResultObject;
                if (video.IsVotingTask)
                {
                    OpenVideoInTaskFeed(videoId, video.TaskId);
                }
                else
                {
                    await OpenVideoInUserFeed(videoId, userName);
                }
            }
            else if (result.IsError)
            {
                Debug.LogError($"Failed to get video \"{videoId}\". Reason: {result.ErrorMessage}");
            }
            else if (result.IsRequestCanceled)
            {
                Debug.Log($"Request to get video \"{videoId}\" was canceled.");
            }
        }

        private void CheckSeasonLink(string link)
        {
            var videoRegex = new Regex(SEASON_LINK_REGEX);
            var match = videoRegex.Match(link);
            
            if (!match.Success) return;

            if (!long.TryParse(match.Groups[1].Value, out var seasonId)) return;

            var currentSeason = _dataFetcher.CurrentSeason;

            if (currentSeason == null || currentSeason.Id != seasonId)
            {
                _snackBarHelper.ShowInformationSnackBar("The requested season could not be not found.", 2);
                return;
            }
            
            _pageManager.MoveNext(new SeasonPageArgs(SeasonPageArgs.Tab.Rewards));
        }

        private async Task OpenVideoInUserFeed(long videoId, string userName)
        {
            var profileResult = await _bridge.GetPublicProfileFor(userName);

            if (profileResult.IsSuccess)
            {
                var groupId = profileResult.Profile.MainGroupId;
                _pageManager.MoveNext(PageId.Feed, new RemoteUserFeedArgs(_videoManager, groupId, videoId));
            }
            else if (profileResult.IsError)
            {
                _snackBarHelper.ShowInformationSnackBar("The requested profile could not be not found.", 2);
                Debug.LogError($"Failed to get profile for \"{userName}\". Reason: {profileResult.ErrorMessage}");
            }
            else if (profileResult.IsRequestCanceled)
            {
                Debug.Log($"Request to get profile for \"{userName}\" was canceled.");
            }
        }

        private void OpenVideoInTaskFeed(long videoId, long taskId)
        {
            _pageManager.MoveNext(PageId.Feed, new TaskFeedArgs(_videoManager, videoId, taskId));
        }

        private void OnPageDisplayed(PageData pageData)
        {
            if (!_startPages.Contains(pageData.PageId)) return;

            _startPageLoaded = true;
            
            _pageManager.PageDisplayed -= OnPageDisplayed;
        }

        //---------------------------------------------------------------------
        // Editor Testing
        //---------------------------------------------------------------------

        #if UNITY_EDITOR

        [Header("Editor Testing")]
        [SerializeField] private string _profileLink = "https://web.frever-api.com/@TestUser/develop";
        [SerializeField] private string _videoLink = "https://web.frever-api.com/video/d_T0bbgChB206iaHmxc4yOA";
        [SerializeField] private string _seasonLink = "https://web.frever-api.com/season/10";

        [Button]
        private void OpenProfile()
        {
            OnUniversalLinkOpen(_profileLink);
        }


        [Button]
        private void OpenVideo()
        {
            OnUniversalLinkOpen(_videoLink);
        }

        [Button]
        private void OpenSeason()
        {
            OnUniversalLinkOpen(_seasonLink);
        }

        #endif
    }
}