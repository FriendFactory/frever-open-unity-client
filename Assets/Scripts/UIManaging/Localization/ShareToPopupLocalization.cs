using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/ShareToPopupLocalization", fileName = "ShareToPopupLocalization")]
    public class ShareToPopupLocalization : ScriptableObject
    {
         
        [SerializeField] private LocalizedString _friendsCategory;
        [SerializeField] private LocalizedString _chatsCategory;
        [SerializeField] private LocalizedString _doneButton;
        [SerializeField] private LocalizedString _sendButton;
        
        public string FriendsCategory => _friendsCategory;
        public string ChatsCategory => _chatsCategory;
        public string DoneButton => _doneButton;
        public string SendButton => _sendButton;
    }
}