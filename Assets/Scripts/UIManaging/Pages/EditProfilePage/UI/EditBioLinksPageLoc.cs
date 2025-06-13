using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.EditProfilePage.UI
{
    internal sealed class EditBioLinksPageLoc : MonoBehaviour
    {
        [SerializeField] private LocalizedString _linksNotValid;
        [SerializeField] private LocalizedString _linksChanged;
        [SerializeField] private LocalizedString _linksConfirmationTitle;
        [SerializeField] private LocalizedString _linksConfirmationDesc;
        [SerializeField] private LocalizedString _linksConfirmationPositive;
        [SerializeField] private LocalizedString _linksConfirmationNegative;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public string LinksNotValid => _linksNotValid;
        public string LinksChanged => _linksChanged;
        public string LinksConfirmationTitle => _linksConfirmationTitle;
        public string LinksConfirmationDesc => _linksConfirmationDesc;
        public string LinksConfirmationPositive => _linksConfirmationPositive;
        public string LinksConfirmationNegative => _linksConfirmationNegative;

    }
}