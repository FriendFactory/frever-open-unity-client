using I2.Loc;
using UnityEngine;
using UnityEngine.Serialization;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/InviteRewardPopupLocalization", fileName = "InviteRewardPopupLocalization")]
    public class InviteRewardPopupLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _friendInviteHeader;
        [SerializeField] private LocalizedString _friendInviteDescriptionFormat;
        [SerializeField] private LocalizedString _creatorInviteHeaderFormat;
        
        [SerializeField] private LocalizedString _userSignedUpTitleFormat;
        [SerializeField] private LocalizedString _userSignedUpDescriptionFormat;
        
        public string FriendInviteHeader => _friendInviteHeader;
        public string FriendInviteDescriptionFormat => _friendInviteDescriptionFormat;
        public string CreatorInviteHeaderFormat => _creatorInviteHeaderFormat;
        
        public string UserSignedUpTitleFormat => _userSignedUpTitleFormat;
        public string UserSignedUpDescriptionFormat => _userSignedUpDescriptionFormat;
    }
}