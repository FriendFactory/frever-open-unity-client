using Abstract;
using Bridge.Services.UserProfile;
using TMPro;
using UnityEngine;

namespace UIManaging.Common.Args.Views.Profile
{
    public class ProfileKPIView : BaseContextDataView<ProfileKPI>
    {
        [SerializeField] private TextMeshProUGUI _likesAmountText;
        [SerializeField] private TextMeshProUGUI _followersAmountText;
        [SerializeField] private TextMeshProUGUI _followingAmountText;
        [SerializeField] private TextMeshProUGUI _friendsAmountText;
        
        protected override void OnInitialized()
        {
            _likesAmountText.text = ContextData.VideoLikesCount.ToString();
            _followersAmountText.text = ContextData.FollowersCount.ToString();
            _followingAmountText.text = ContextData.FollowingCount.ToString();
            _friendsAmountText.text = ContextData.FriendsCount.ToString();
        }
    }
}