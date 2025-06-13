using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.EditProfilePage.UI
{
    internal sealed class EditBioPageLoc : MonoBehaviour
    {
        [SerializeField] private LocalizedString _bioPlaceholder;
        [SerializeField] private LocalizedString _bioChanged;
        [SerializeField] private LocalizedString _bioModerationError;
        [SerializeField] private LocalizedString _bioConfirmationTitle;
        [SerializeField] private LocalizedString _bioConfirmationDesc;
        [SerializeField] private LocalizedString _bioConfirmationPositive;
        [SerializeField] private LocalizedString _bioConfirmationNegative;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public string BioPlaceholder => _bioPlaceholder;
        public string BioChanged => _bioChanged;
        public string BioModerationError => _bioModerationError;
        public string BioConfirmationTitle => _bioConfirmationTitle;
        public string BioConfirmationDesc => _bioConfirmationDesc;
        public string BioConfirmationPositive => _bioConfirmationPositive;
        public string BioConfirmationNegative => _bioConfirmationNegative;

    }
}