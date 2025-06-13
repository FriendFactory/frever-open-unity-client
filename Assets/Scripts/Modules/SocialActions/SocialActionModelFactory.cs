using System;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.SocialActions;
using Bridge.Models.ClientServer.Tasks;
using Bridge.Models.ClientServer.Template;
using Bridge.Services.UserProfile;
using Common.BridgeAdapter;
using JetBrains.Annotations;
using Modules.CharacterManagement;
using Modules.UniverseManaging;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.PopupSystem;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Modules.SocialActions
{
    public sealed class SocialActionModelFactory : MonoBehaviour
    {
        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private PageManager _pageManager;
        [Inject] private IScenarioManager _scenarioManager;
        [Inject] private ILevelService _levelManager;
        [Inject] private CharacterManager _characterManager;
        [Inject] private LocalUserDataHolder _localUserData;
        [Inject] private PopupManager _popupManager;
        [Inject] private VideoManager _videoManager;
        [Inject] private IUniverseManager _universeManager;

        [Space] 
        [FormerlySerializedAs("_taskBackground")] [SerializeField] private Sprite _blueBackground;
        [FormerlySerializedAs("_userBackground")] [SerializeField] private Sprite _pinkBackground;
        
        [Space] 
        [SerializeField] private Sprite _templateIcon;
        [SerializeField] private Sprite _userIcon;
        [SerializeField] private Sprite _taskIcon;
        [SerializeField] private Sprite _videoIcon;
        [SerializeField] private Sprite _playVideoIcon;
        
        public Task<SocialActionCardModel> Create(SocialActionFullInfo socialActionInfo,
            Action<Guid, long> onDeleteCardRequested, Action<Guid, long> onActionCompleted,
            CancellationToken token)
        {
            switch (socialActionInfo.ActionType)
            {
                case SocialActionType.PopularAccount:
                    return CreatePopularAccountCardModel(socialActionInfo, onDeleteCardRequested, onActionCompleted, token);
                
                case SocialActionType.MutualVideo:
                    return CreateMutualVideoCardModel(socialActionInfo, onDeleteCardRequested, onActionCompleted, token);

                case SocialActionType.LikelyFriend:
                    return CreateLikelyFriendCardModel(socialActionInfo, onDeleteCardRequested, onActionCompleted, token);

                case SocialActionType.FollowBack:
                    return CreateFollowBackCardModel(socialActionInfo, onDeleteCardRequested, onActionCompleted, token);

                case SocialActionType.StyleBattle:
                    return CreateStyleBattleCardModel(socialActionInfo, onDeleteCardRequested, onActionCompleted, token);

                case SocialActionType.TrendingTemplate:
                    return CreateTrendingTemplateCardModel(socialActionInfo, onDeleteCardRequested, onActionCompleted, token);
                
                case SocialActionType.LikeVideo:
                    return CreateLikeVideoCardModel(socialActionInfo, onDeleteCardRequested, onActionCompleted, token);

                default:
                    return Task.FromResult<SocialActionCardModel>(null);
            }
        }

        private async Task<SocialActionCardModel> CreatePopularAccountCardModel(SocialActionFullInfo socialActionInfo,
            Action<Guid, long> onDeleteCardRequested, Action<Guid, long> onActionCompleted,
            CancellationToken token)
        {
            const string DESCRIPTION = "{0} Followers";
            const string MESSAGE = "<link=\"mention:{1}\"><style=\"Mention\">@{0}</style></link> is blowing up, start to follow to not miss out on the action!";

            var otherUserId = socialActionInfo.TargetGroup.Id;

            var profile = await GetProfile(otherUserId, token);
            if (profile == null) return null;

            return new SocialActionCardModel
            {
                Background = _pinkBackground,
                RecommendationId = socialActionInfo.RecommendationId,
                ActionId = socialActionInfo.ActionId,
                HeaderButtonClick = () => OpenProfilePage(otherUserId, profile.NickName),
                DeleteAction = onDeleteCardRequested,
                ActionCompleted = onActionCompleted,
                Description = string.Format(DESCRIPTION, profile.KPI.FollowersCount),
                Header = $"<link=\"mention:{profile.MainGroupId}\"><style=\"Mention\">@{profile.NickName}</style></link>",
                Message = string.Format(MESSAGE, profile.NickName, profile.MainGroupId),
                SocialAction =
                    new FollowUserAction(otherUserId, profile.NickName, _bridge, _snackBarHelper, _pageManager),
                PerformActionLabel = "Follow",
                PerformActionIcon = _userIcon,
                ThumbnailProfile = profile
            };
        }

        [CanBeNull]
        private async Task<SocialActionCardModel> CreateMutualVideoCardModel(SocialActionFullInfo socialActionInfo,
            Action<Guid, long> onDeleteCardRequested, Action<Guid, long> onActionCompleted, CancellationToken token)
        {
            const string DESCRIPTION = "Your friend";
            const string MESSAGE = "You just became friends with user <link=\"mention:{1}\"><style=\"Mention\">@{0}</style></link>. Create a video together";

            var profile = await GetProfile(socialActionInfo.TargetGroup.Id, token);
            if (profile == null) return null;

            var action = await CreateMutualVideoAction(socialActionInfo, token);
            if (token.IsCancellationRequested) return null;

            return new SocialActionCardModel
            {
                Background = _blueBackground,
                RecommendationId = socialActionInfo.RecommendationId,
                ActionId = socialActionInfo.ActionId,
                HeaderButtonClick = () => OpenProfilePage(profile.MainGroupId, profile.NickName),
                DeleteAction = onDeleteCardRequested,
                ActionCompleted = onActionCompleted,
                Description = DESCRIPTION,
                Header = $"<link=\"mention:{profile.MainGroupId}\"><style=\"Mention\">@{profile.NickName}</style></link>",
                Message = string.Format(MESSAGE, profile.NickName, profile.MainGroupId),
                SocialAction = action,
                PerformActionLabel = "Generate video",
                PerformActionIcon = _videoIcon,
                MarkInstantlyAsDone = false,
                ThumbnailProfile = profile
            };
        }

        private async Task<ISocialAction> CreateMutualVideoAction(SocialActionFullInfo socialActionFullInfo,
            CancellationToken token)
        {
            var friendProfile = await GetProfile(socialActionFullInfo.TargetGroup.Id, token);
            if (token.IsCancellationRequested) return null;
            
            var characterIds = new[] { _localUserData.UserProfile.MainCharacter.Id, friendProfile.MainCharacter.Id };

            return new MutualVideoAction(socialActionFullInfo.RecommendationId, socialActionFullInfo.ActionId, socialActionFullInfo.TargetVideo.Id, characterIds, _bridge, _scenarioManager,
                                         _levelManager, _characterManager, _popupManager);
        }

        private async Task<SocialActionCardModel> CreateLikelyFriendCardModel(SocialActionFullInfo socialActionFullInfo,
            Action<Guid, long> onDeleteCardRequested, Action<Guid, long> onActionCompleted,
            CancellationToken token)
        {
            const string DESCRIPTION = "{0} Followers";
            const string MESSAGE = "<link=\"mention:{1}\"><style=\"Mention\">@{0}</style></link> is followed by your friends, start following you too.";


            var otherUserId = socialActionFullInfo.TargetGroup.Id;
            var profile = await GetProfile(otherUserId, token);
            if (profile == null) return null;

            return new SocialActionCardModel
            {
                Background = _pinkBackground,
                RecommendationId = socialActionFullInfo.RecommendationId,
                ActionId = socialActionFullInfo.ActionId,
                HeaderButtonClick = () => OpenProfilePage(profile.MainGroupId, profile.NickName),
                DeleteAction = onDeleteCardRequested,
                ActionCompleted = onActionCompleted,
                Description = string.Format(DESCRIPTION, profile.KPI.FollowersCount),
                Header = $"<link=\"mention:{profile.MainGroupId}\"><style=\"Mention\">@{profile.NickName}</style></link>",
                Message = string.Format(MESSAGE, profile.NickName, profile.MainGroupId),
                SocialAction =
                    new FollowUserAction(otherUserId, profile.NickName, _bridge, _snackBarHelper, _pageManager),
                PerformActionLabel = "Follow",
                PerformActionIcon = _userIcon,
                ThumbnailProfile = profile
            };
        }

        private async Task<SocialActionCardModel> CreateFollowBackCardModel(SocialActionFullInfo socialActionFullInfo,
            Action<Guid, long> onDeleteCardRequested, Action<Guid, long> onActionCompleted, CancellationToken token)
        {
            const string DESCRIPTION = "Your follower";
            const string MESSAGE = "<link=\"mention:{1}\"><style=\"Mention\">@{0}</style></link> started to follow you, follow back to become friends!";

            var otherUserId = socialActionFullInfo.TargetGroup.Id;
            var profile = await GetProfile(otherUserId, token);
            if (profile == null) return null;

            return new SocialActionCardModel
            {
                Background = _pinkBackground,
                RecommendationId = socialActionFullInfo.RecommendationId,
                ActionId = socialActionFullInfo.ActionId,
                HeaderButtonClick = () => OpenProfilePage(profile.MainGroupId, profile.NickName),
                DeleteAction = onDeleteCardRequested,
                ActionCompleted = onActionCompleted,
                Description = DESCRIPTION,
                Header = $"<link=\"mention:{profile.MainGroupId}\"><style=\"Mention\">@{profile.NickName}</style></link>",
                Message = string.Format(MESSAGE, profile.NickName, profile.MainGroupId),
                SocialAction =
                    new FollowUserAction(otherUserId, profile.NickName, _bridge, _snackBarHelper, _pageManager),
                PerformActionLabel = "Follow back",
                PerformActionIcon = _userIcon,
                ThumbnailProfile = profile
            };
        }

        private async Task<SocialActionCardModel> CreateStyleBattleCardModel(SocialActionFullInfo socialActionFullInfo,
            Action<Guid, long> onDeleteCardRequested, Action<Guid, long> onActionCompleted,
            CancellationToken token)
        {
            const string DESCRIPTION = "{0} Followers";
            const string MESSAGE = "<link=\"mention:{1}\"><style=\"Mention\">@{0}</style></link> did the style challenge: {2}, do it you too and see who get highest score!";

            var profile = await GetProfile((long)Convert.ToDouble(socialActionFullInfo.Reason), token);
            if (profile == null) return null;

            var taskInfo = await GetTaskFullInfo(socialActionFullInfo.TargetTaskId.Value, token);
            if (taskInfo is null) return null;

            return new SocialActionCardModel
            {
                Background = _blueBackground,
                RecommendationId = socialActionFullInfo.RecommendationId,
                ActionId = socialActionFullInfo.ActionId,
                HeaderButtonClick = () => OpenTaskPage(taskInfo),
                DeleteAction = onDeleteCardRequested,
                ActionCompleted = onActionCompleted,
                Description = string.Format(DESCRIPTION, profile.KPI.FollowersCount),
                Header = $"<link=\"mention:{profile.MainGroupId}\"><style=\"Mention\">@{profile.NickName}</style></link>",
                Message = string.Format(MESSAGE, profile.NickName, profile.MainGroupId,taskInfo.Name),
                SocialAction = new StyleBattleAction(socialActionFullInfo.ActionId, taskInfo, _scenarioManager, _pageManager, _videoManager, _universeManager),
                PerformActionLabel = "Join the challenge",
                PerformActionIcon = _taskIcon,
                MarkInstantlyAsDone = false,
                ThumbnailProfile = profile
            };
        }

        private async Task<SocialActionCardModel> CreateTrendingTemplateCardModel(SocialActionFullInfo socialActionFullInfo,
            Action<Guid, long> onDeleteCardRequested, Action<Guid, long> onActionCompleted, CancellationToken token)
        {
            const string DESCRIPTION = "Trending template";
            const string MESSAGE =
                "{0} users are using this trending template: {1}. Be one of the first among to join!";

            var templateInfo = await GetTemplateInfo(socialActionFullInfo.TargetTemplateId.Value, token);
            if (templateInfo is null) return null;

            return new SocialActionCardModel
            {
                Background = _blueBackground,
                RecommendationId = socialActionFullInfo.RecommendationId,
                ActionId = socialActionFullInfo.ActionId,
                HeaderButtonClick = () => OpenTemplatePage(templateInfo),
                DeleteAction = onDeleteCardRequested,
                ActionCompleted = onActionCompleted,
                Description = DESCRIPTION,
                Header = templateInfo.Title,
                Message = string.Format(MESSAGE, templateInfo.UsageCount, templateInfo.Title),
                SocialAction = new TrendingTemplateAction(socialActionFullInfo.RecommendationId, socialActionFullInfo.ActionId, templateInfo, 
                                                          _scenarioManager, _popupManager),
                PerformActionLabel = "Go to template",
                PerformActionIcon = _templateIcon,
                MarkInstantlyAsDone = false
            };
        }

        private async Task<SocialActionCardModel> CreateLikeVideoCardModel(SocialActionFullInfo socialActionInfo, 
            Action<Guid, long> onDeleteCardRequested, Action<Guid, long> onActionCompleted, CancellationToken token)
        {
            const string DESCRIPTION = "{0} Followers";
            const string MESSAGE = "Your friend <link=\"mention:{1}\"><style=\"Mention\">@{0}</style></link> just posted a video, check it out and like it!";

            var otherUserId = socialActionInfo.TargetGroup.Id;
            
            var profile = await GetProfile(otherUserId, token);
            if (profile == null) return null;

            return new SocialActionCardModel
            {
                Background = _pinkBackground,
                RecommendationId = socialActionInfo.RecommendationId,
                ActionId = socialActionInfo.ActionId,
                HeaderButtonClick = () => OpenProfilePage(profile.MainGroupId, profile.NickName),
                DeleteAction = onDeleteCardRequested,
                ActionCompleted = onActionCompleted,
                Description = string.Format(DESCRIPTION, profile.KPI.FollowersCount),
                Header = $"<link=\"mention:{profile.MainGroupId}\"><style=\"Mention\">@{profile.NickName}</style></link>",
                Message = string.Format(MESSAGE, profile.NickName, profile.MainGroupId),
                SocialAction = new LikeVideoAction(otherUserId, socialActionInfo.TargetVideo.Id, _pageManager, _videoManager),
                PerformActionLabel = "Watch video",
                PerformActionIcon = _playVideoIcon,
                ThumbnailProfile = profile
            };
        }

        private async Task<TemplateInfo> GetTemplateInfo(long templateId, CancellationToken token)
        {
            var templateResult = await _bridge.GetEventTemplate(templateId, token);
            if (templateResult.IsSuccess) return templateResult.Model;

            if (templateResult.IsError) Debug.LogError(templateResult.ErrorMessage);

            return null;
        }

        private async Task<TaskFullInfo> GetTaskFullInfo(long taskId, CancellationToken token)
        {
            var taskResult = await _bridge.GetTaskFullInfoAsync(taskId, token);
            if (taskResult.IsSuccess) return taskResult.Model;

            if (taskResult.IsError) Debug.LogError(taskResult.ErrorMessage);

            return null;
        }

        [ItemCanBeNull]
        private async Task<Profile> GetProfile(long groupId, CancellationToken token)
        {
            var result = await _bridge.GetProfile(groupId, token);

            return result.IsSuccess ? result.Profile : null;
        }

        private void OpenProfilePage(long groupdId, string nickname)
        {
            var args = new UserProfileArgs(groupdId, nickname);
            _pageManager.MoveNext(args);
        }

        private void OpenTemplatePage(TemplateInfo templateInfo)
        {
            var args = new VideosBasedOnTemplatePageArgs
            {
                TemplateInfo = templateInfo,
                TemplateName = templateInfo.Title, 
                UsageCount = templateInfo.UsageCount
            };

            _pageManager.MoveNext(args);
        }
        
        private void OpenTaskPage(TaskFullInfo taskFullInfo)
        {
            var args = new TaskDetailsPageArgs(taskFullInfo);
            
            _pageManager.MoveNext(args);
        }
    }
}