using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.EditProfilePage.UI
{
    internal sealed class EditProfilePageLoc : MonoBehaviour
    {
        [SerializeField] private LocalizedString _pageHeader;
        [SerializeField] private LocalizedString _bioPlaceholder;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public string PageHeader => _pageHeader;
        public string BioPlaceholder => _bioPlaceholder;

    }
}