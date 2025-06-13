using System.Text;
using Abstract;
using Bridge.Models.ClientServer;
using Extensions;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Localization;
using UnityEngine;
using Zenject;

namespace UIManaging.Common
{
    public class FollowerMembersView : BaseContextDataView<FollowerMembersModel>
    {
        [SerializeField] private UserPortraitView[] _userPortraitViews;
        [SerializeField] private GameObject _moreCounter;
        [SerializeField] private TMP_Text _followersCounterText;
        [SerializeField] private TMP_Text _moreCounterText;
        
        [SerializeField] private Sprite _circleMaskSprite;
        [SerializeField] private Sprite _halfMoonMaskSprite;

        [Inject] private CrewPageLocalization _localization;
        
        protected override void OnInitialized()
        {
            ProjectContext.Instance.Container.InjectGameObject(gameObject);
            
            if (ContextData.Members.IsNullOrEmpty())
            {
                gameObject.SetActive(false);
                return;
            }

            if (ContextData.FriendsCount == 0 && ContextData.FollowersCount == 0 && ContextData.FollowingCount == 0)
            {
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);
            
            _userPortraitViews.ForEach(item => item.SetActive(false));
            _moreCounter.SetActive(false);
            
            var showMoreCounter = ContextData.TotalMembersCount > _userPortraitViews.Length;
            var displayedCount = Mathf.Min(ContextData.TotalMembersCount, _userPortraitViews.Length);
            if (showMoreCounter) displayedCount--;
            var lastIndex = 0;
            
            for (var i = 0; i < displayedCount; i++)
            {
                lastIndex = i;
                
                var view = _userPortraitViews[i];
                var user = ContextData.Members[i];
                view.gameObject.SetActive(true);
                view.Initialize(new UserPortraitModel
                {
                    UserGroupId = user.Id,
                    UserMainCharacterId = user.MainCharacterId ?? 0,
                    MainCharacterThumbnail = user.MainCharacterFiles
                });

                view.SetMaskImage(_halfMoonMaskSprite);
            }

            if (!showMoreCounter)
            {
                _userPortraitViews[lastIndex].SetMaskImage(_circleMaskSprite);
            }

            var sb = new StringBuilder();

            if (ContextData.FriendsCount > 0)
            {
                sb.Append(ContextData.FriendsCount > 1
                              ? string.Format(_localization.CrewListItemFriendsPluralCounterFormat,
                                              ContextData.FriendsCount)
                              : string.Format(_localization.CrewListItemFriendsCounterFormat,
                                              ContextData.FriendsCount));
            }
            if (ContextData.FollowersCount > 0)
            {
                if(sb.Length > 0) sb.Append(", ");
                sb.Append(ContextData.FollowersCount > 1
                              ? string.Format(_localization.CrewListItemFollowersPluralCounterFormat,
                                              ContextData.FollowersCount)
                              : string.Format(_localization.CrewListItemFollowersCounterFormat,
                                              ContextData.FollowersCount));
            }
            if (ContextData.FollowingCount > 0)
            {
                if(sb.Length > 0) sb.Append(", ");
                sb.Append(ContextData.FollowingCount > 1
                              ? string.Format(_localization.CrewListItemFollowingPluralCounterFormat,
                                              ContextData.FollowingCount)
                              : string.Format(_localization.CrewListItemFollowingCounterFormat,
                                              ContextData.FollowingCount));
            }

            _followersCounterText.text = sb.ToString();
            
            if (!showMoreCounter) return;
            _moreCounter.SetActive(true);
            _moreCounterText.text = $"+{ContextData.TotalMembersCount - _userPortraitViews.Length}";
        }
    }

    public class FollowerMembersModel
    {
        public GroupShortInfo[] Members { get; set; }
        public int FriendsCount { get; set; }
        public int FollowingCount { get; set; }
        public int FollowersCount { get; set; }
        public int TotalMembersCount => FriendsCount + FollowingCount + FollowersCount;
    }
}