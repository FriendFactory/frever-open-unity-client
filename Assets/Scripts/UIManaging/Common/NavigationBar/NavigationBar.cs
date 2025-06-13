using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Extensions;
using Modules.AssetsStoraging.Core;
using Modules.Notifications;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.SeasonPage;
using UnityEngine;
using Zenject;

namespace UIManaging.Common
{
    public class NavigationBar: MonoBehaviour
    {
        [SerializeField] private List<NavigationBarToggle> _toggles;
        
        [Space]
        [SerializeField] private NavBarNotificationBubble _styleNotifications;
        [SerializeField] private NavBarNotificationBubble _crewNotifications;
        [SerializeField] private PeriodicUpdater _updater;

        [Inject] private IBridge _bridge;
        [Inject] private PageManager _pageManager;
        [Inject] private SeasonRewardsHelper _seasonRewardsHelper;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private INotificationHandler _notificationHandler;

        private CancellationTokenSource _tokenSource;
        private bool _periodicUpdateActive;

        private static int _lastStyleNotificationsCount;
        private static int _lastCrewNotificationsCount;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _tokenSource = new CancellationTokenSource();
            
            _periodicUpdateActive = true;
            
            _pageManager.PageDisplayed += SwitchToggles;
            
            if (_lastStyleNotificationsCount == 0)
            {
                _styleNotifications.Hide();
            }
            else
            {
                _styleNotifications.Show(_lastStyleNotificationsCount);
            }

            if (_lastCrewNotificationsCount == 0)
            {
                _crewNotifications.Hide();
            }
            else
            {
                _crewNotifications.Show(_lastCrewNotificationsCount);
            }

            _updater.CallUpdateFunc = RefreshCrewButtonBubble;
            _updater.IsRefreshing = () => _periodicUpdateActive;
        }

        private void OnDisable()
        {
            _periodicUpdateActive = false;

            _updater.CallUpdateFunc = null;
            _updater.IsRefreshing = null;
            
            _pageManager.PageDisplayed -= SwitchToggles;
            
            if (_tokenSource == null) return;

            _tokenSource.CancelAndDispose();
            _tokenSource = null;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void SwitchToggles(PageData pageData)
        {
            if (!_toggles.Exists(t => t.TargetPages.Contains(pageData.PageId)))
            {
                _toggles.ForEach(t => t.Switch(false));
                return;
            }

            foreach (var toggle in _toggles)
            {
                var shouldBeOn = toggle.TargetPages.Contains(pageData.PageId);
                toggle.Switch(shouldBeOn);
            }

            await RefreshNotificationBubbles();
        }

        private async Task RefreshNotificationBubbles()
        {
            await RefreshStyleButtonBubble();
            
            if (_tokenSource is null || _tokenSource.IsCancellationRequested) return;
            
            await RefreshCrewButtonBubble();
        }

        private async Task RefreshStyleButtonBubble()
        {
            var seasonRewardsCount = await _seasonRewardsHelper.IsClaimableRewardAvailable(true);

            var creatorScore = _localUser.LevelingProgress.CreatorScore;
            var creatorScoreBadge = _localUser.LevelingProgress.CreatorScoreBadge;
            var creatorScoreRewards = _dataFetcher.MetadataStartPack.CreatorBadges;
            var scoreCount = creatorScoreRewards.Count(r => r.Level > creatorScoreBadge && r.CreatorScoreRequired <= creatorScore);

            var notificationsCount = _notificationHandler.UnreadNotificationsCount;

            var styleCount = seasonRewardsCount + scoreCount + notificationsCount;
            _lastStyleNotificationsCount = styleCount;
            
            if (_tokenSource == null || _tokenSource.IsCancellationRequested) return;
            
            if (styleCount == 0)
            {
                _styleNotifications.Hide();
                return;
            }
            _styleNotifications.Show(styleCount);
        }

        private async Task<bool> RefreshCrewButtonBubble()
        {
            _periodicUpdateActive = false;
            
            var result = await _bridge.GetChatUnreadMessagesCount(_tokenSource.Token);
            
            _periodicUpdateActive = true;

            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
                return false;
            }

            if (!result.IsSuccess)
            {
                return true;
            }

            _lastCrewNotificationsCount = result.Count;

            if (_tokenSource == null || _tokenSource.IsCancellationRequested)
            {
                return true;
            }
            
            if (result.Count == 0)
            {
                _crewNotifications.Hide();
            }
            else
            {
                _crewNotifications.Show(result.Count);
            }
            
            return true;
        }
    }
}