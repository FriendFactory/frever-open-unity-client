using System;
using Bridge.Models.ClientServer.Recommendations;
using DG.Tweening;
using TMPro;
using UIManaging.Localization;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.FollowersPage.Recommendations
{
    public class FollowRecommendationTextHandler: MonoBehaviour
    {
        private const int MAX_NAME_LENGTH = 7;

        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TMP_Text _username;
        [SerializeField] private TMP_Text _reason;
        [Header("Animation")] 
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private Ease _ease = Ease.Linear;

        [Inject] private ProfileLocalization _localization;
        
        private Tween _textFadeInTween;

        private void Awake()
        {
            Clear();
            InitializeTextFadeInTween();
        }

        private void OnDestroy()
        {
            _textFadeInTween.Kill();
        }

        public void UpdateText(FollowRecommendation recommendation)
        {
            _username.text = recommendation.Group.Nickname;
            _reason.text = GetFormattedReason(recommendation);

            _textFadeInTween.Restart();
        }

        public void Clear()
        {
            _group.alpha = 0f;
            
            _username.text = string.Empty;
            _reason.text = string.Empty;
        }

        private void InitializeTextFadeInTween()
        {
            _textFadeInTween = _group.DOFade(1f, _duration)
                                     .SetEase(_ease)
                                     .SetAutoKill(false)
                                     .Pause();
        }
        
        private string GetFormattedReason(FollowRecommendation recommendation)
        {
            switch (recommendation.Reason)
            {
                case RecommendationReason.CommonFriends:
                    return GetCommonTextDescription();
                case RecommendationReason.Influential:
                    return _localization.FollowRecommendationReasonPopularCreator;
                case RecommendationReason.Personalized:
                    return _localization.FollowRecommendationReasonSimilarInterests;
                case RecommendationReason.FollowBack:
                    return _localization.FollowRecommendationReasonFollowingYou;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            string GetCommonTextDescription()
            {
                var commonFriends = recommendation.CommonFriends;
                
                if (commonFriends.Length == 0) return string.Format(_localization.FollowRecommendationReasonFollowedBy, string.Empty);
                
                var followedBy = TruncateNickname(commonFriends[0].Nickname);
                var otherFriendsCount = commonFriends.Length - 1;

                return otherFriendsCount > 0 
                    ? string.Format(_localization.FollowRecommendationReasonFollowedByMultiple, followedBy, otherFriendsCount.ToString())
                    : string.Format(_localization.FollowRecommendationReasonFollowedBy, followedBy);
            }
        }

        private static string TruncateNickname(string nickname) => nickname.Length <= MAX_NAME_LENGTH
            ? nickname
            : $"{nickname.Substring(0, MAX_NAME_LENGTH)}...";
    }
}