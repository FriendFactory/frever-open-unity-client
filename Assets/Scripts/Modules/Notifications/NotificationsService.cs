using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.NotificationServer;
using Bridge.Results;
using Common.UserNotifications;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.Notifications.NotificationItemModels;
using Modules.Notifications.NotificationProviders;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;

namespace Modules.Notifications
{
    [UsedImplicitly]
    public sealed class NotificationsService: INotificationEventSource
    {
        private const int REFRESH_WAIT_TIME = 30000;
        
        private readonly INotificationHandler _notificationHandler;
        private readonly IBridge _bridge;
        private readonly BaseNotificationProvider[] _notificationProviders;
        
        private CancellationTokenSource _cancellationToken;
        private bool _isRefreshing;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public NotificationsService(INotificationHandler notificationHandler, IBridge bridge, LocalUserDataHolder dataHolder, IDataFetcher dataFetcher)
        {
            _notificationHandler = notificationHandler;
            _bridge = bridge;

            _notificationProviders = new BaseNotificationProvider[]
            {
                new NewFollowerNotificationProvider(),
                new RemixedYourVideoNotificationProvider(),
                new FriendNewVideoNotificationProvider(),
                new TaggedOnVideoNotificationProvider(),
                new NewLikeOnVideoNotificationProvider(),
                new CommentOnVideoNotificationProvider(),
                new CommentOnVideoYouHaveCommentedOnNotificationProvider(),
                new VideoDeletedNotificationProvider(),
                new NewMentionOnVideoNotificationProvider(),
                new NewMentionInCommentOnVideoNotificationProvider(),
                new SeasonLikesNotificationProvider(dataFetcher, _bridge),
                new InvitationAcceptedNotificationProvider(dataHolder),
                new NonCharacterTagOnVideoNotificationProvider(),
                new StyleBattleResultNotificationProvider(),
                new СrewFriendJoinedNotificationProvider(dataHolder),
                new СrewInvitationNotificationProvider(),
                new СrewJoinRequestAcceptedNotificationProvider(),
                new СrewJoinRequestNotificationProvider(),
                new VideoGotRatingNotificationProvider(),
            };   
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<NotificationBase> NewNotificationReceived;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void GetLatestNotifications(Action<NotificationItemModel[]> onComplete = null, CancellationToken token = default)
        {
            await DownloadLatestNotifications(onComplete, token);
        }

        public async void Start()
        {
            if(_isRefreshing) StopRefreshing();
            
            try
            {
                _cancellationToken = new CancellationTokenSource();
                await StartRefreshing(_cancellationToken.Token);
            }
            catch (OperationCanceledException)
            {
                //Do nothing
            }
        }

        public void StopRefreshing()
        {
            _cancellationToken?.Cancel();
            _isRefreshing = false;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private async Task StartRefreshing(CancellationToken token)
        {
            _isRefreshing = true;
            
            while (_isRefreshing)
            {
                await Task.Delay(REFRESH_WAIT_TIME, token);
                
                if (!_bridge.IsLoggedIn)
                {
                    StopRefreshing();
                    break;
                }
                
                await DownloadLatestNotifications(token: token);
            }
        }
        
        private async Task DownloadLatestNotifications(Action<NotificationItemModel[]> onComplete = null, CancellationToken token = default)
        {
            var result = await _bridge.MyLatestNotifications(null, token);
            
            if (result.IsSuccess)
            {
                var cached = _notificationHandler.GetNotifications();
                var latestCached = cached.FirstOrDefault();
                var latestFromBackend = result.Models.FirstOrDefault();
                
                if (latestCached?.Id == latestFromBackend?.Id)
                {
                    onComplete?.Invoke(_notificationHandler.GetNotifications());
                    return;
                }

                CheckForNewNotReadNotifications(latestCached, result);

                _notificationHandler.ClearNotifications();

                var newNotificationModels = new List<NotificationItemModel>();
                foreach (var notification in result.Models)
                {
                    var provider = _notificationProviders.FirstOrDefault(x => x.Type == notification.NotificationType);
                    var notificationItemModel = provider?.GetNotificationItemModel(notification);

                    if (notificationItemModel == null || !notificationItemModel.IsValid()) continue;

                    notificationItemModel.HasRead = notification.HasRead;

                    newNotificationModels.Add(notificationItemModel);
                }
                
                if (newNotificationModels.Count != 0) _notificationHandler.AddNotifications(newNotificationModels);

                var notificationModels = _notificationHandler.GetNotifications();
                onComplete?.Invoke(notificationModels);
                return;
            }

            if (result.IsError)
            {
                Debug.LogError("Failed to download latest notifications [Reason] " + result.ErrorMessage);
            }
        }

        private void CheckForNewNotReadNotifications(NotificationItemModel latestCached, ArrayResult<NotificationBase> result)
        {
            var newNotifications = latestCached == null
                ? result.Models
                : result.Models.TakeWhile(x => x.Id != latestCached.Id);
            foreach (var newNotReadNotification in newNotifications.Where(x => !x.HasRead))
            {
                NewNotificationReceived?.Invoke(newNotReadNotification);
            }
        }
    }
}