using I2.Loc;
using UnityEngine;
using UnityEngine.Serialization;

namespace UIManaging.Pages.EditUsername
{
    [CreateAssetMenu(menuName = "L10N/EditUsernameLocalization", fileName = "EditUsernameLocalization")]
    public class EditUsernameLocalization: ScriptableObject
    {
        [SerializeField] private LocalizedString _usernameUpdated;
        [SerializeField] private LocalizedString _usernameAndLoginMethodUpdated;
        [SerializeField] private LocalizedString _usernameUpdateFailed;
        [Header("Confirmation Popup")]
        [SerializeField] private LocalizedString _confirmTitle;
        [SerializeField] private LocalizedString _confirmDescription;
        [SerializeField] private LocalizedString _confirmYes;
        [SerializeField] private LocalizedString _confirmNo;
        [Header("Username Update Status Info")]
        [SerializeField] private LocalizedString _nextUpdateStatusInfoOne;
        [SerializeField] private LocalizedString _nextUpdateStatusInfoMany;
        
        public string UsernameUpdated => _usernameUpdated;
        public string UsernameAndLoginMethodUpdated => _usernameAndLoginMethodUpdated;
        public string UsernameUpdateFailed => _usernameUpdateFailed;
        public string ConfirmTitle => _confirmTitle;
        public string ConfirmDescription => _confirmDescription;
        public string ConfirmYes => _confirmYes;
        public string ConfirmNo => _confirmNo;

        public string GetNextUsernameUpdateInfo(int daysLeft) =>
            string.Format(daysLeft == 1 ? _nextUpdateStatusInfoOne : _nextUpdateStatusInfoMany, daysLeft);
    }
}