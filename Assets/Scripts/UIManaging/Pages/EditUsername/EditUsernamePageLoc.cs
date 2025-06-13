using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.EditUsername
{
    public sealed class EditUsernamePageLoc : MonoBehaviour
    {
        [SerializeField] private LocalizedString _usernameChanged;
        [SerializeField] private LocalizedString _usernameInUse;
        [SerializeField] private LocalizedString _usernameConfirmationTitle;
        [SerializeField] private LocalizedString _usernameConfirmationDesc;
        [SerializeField] private LocalizedString _usernameConfirmationPositive;
        [SerializeField] private LocalizedString _usernameConfirmationNegative;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public string UsernameChanged => _usernameChanged;
        public string UsernameInUse => _usernameInUse;
        public string UsernameConfirmationTitle => _usernameConfirmationTitle;
        public string UsernameConfirmationDesc => _usernameConfirmationDesc;
        public string UsernameConfirmationPositive => _usernameConfirmationPositive;
        public string UsernameConfirmationNegative => _usernameConfirmationNegative;
    }
}