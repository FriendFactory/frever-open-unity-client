using System.Collections.Generic;
using Bridge.NotificationServer;

namespace UIManaging.Pages.NotificationPage
{
    internal static class NotificationExtensions
    {
        private static readonly Dictionary<NotificationTabType, HashSet<NotificationType>> MAP =
            new()
            {
                {
                    NotificationTabType.Social, new HashSet<NotificationType>
                    {
                        NotificationType.NewFollower,
                        NotificationType.NewLikeOnVideo,
                        NotificationType.NewCommentOnVideo,
                        NotificationType.VideoDeleted,
                        NotificationType.NewMentionInCommentOnVideo,
                        NotificationType.NewMentionOnVideo,
                        NotificationType.CrewInvitationReceived,
                        NotificationType.CrewJoinRequestAccepted,
                        NotificationType.CrewJoinRequestReceived,
                        NotificationType.InvitationAccepted,
                        NotificationType.FriendJoinedCrew,
                        NotificationType.Welcome,
                        NotificationType.FirstFollower,
                        NotificationType.InvitationAccepted,
                        NotificationType.NewFriendVideo,
                        NotificationType.NewCommentOnVideoYouHaveCommented,
                        NotificationType.YourVideoConverted,
                        NotificationType.NumberOfRemixesOnVideo,
                        NotificationType.NewAssetsInWardrobe,
                        NotificationType.NewScenesInApp,
                        NotificationType.NewVideoInCategory,
                        NotificationType.NumberOfLikesOnVideo,
                        NotificationType.NewTrendVideo
                    }
                },
                {
                    NotificationTabType.Rewards, new HashSet<NotificationType>
                    {
                        NotificationType.SeasonQuestAccomplished,
                        NotificationType.VideoRatingCompleted,
                        NotificationType.NewStatusReached,
                        NotificationType.NewLevelReached,
                        NotificationType.BattleResultCompleted,
                        NotificationType.VideoStyleTransformed,
                    }
                },
                {
                    NotificationTabType.Tagged, new HashSet<NotificationType>
                    {
                        NotificationType.YouTaggedOnVideo,
                        NotificationType.NonCharacterTagOnVideo,
                        NotificationType.YourVideoRemixed,
                    }
                }
            };

        public static bool IsTypeOf(this NotificationType notificationType, NotificationTabType targetType)
        {
            return MAP[targetType].Contains(notificationType);
        }
    }
}