using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.AppSettingsPage.UI
{
    internal sealed class AppSettingsPageLoc : MonoBehaviour
    {
        [SerializeField] private LocalizedString _pageHeader;
        [Space]
        [SerializeField] private LocalizedString _profileHeader;
        [SerializeField] private LocalizedString _editProfileTitle;
        [Space]
        [SerializeField] private LocalizedString _accountHeader;
        [SerializeField] private LocalizedString _manageAccountTitle;
        [SerializeField] private LocalizedString _privacyTitle;
        [SerializeField] private LocalizedString _communitySupportTitle;
        [SerializeField] private LocalizedString _faqTitle;
        [SerializeField] private LocalizedString _termsOfUseTitle;
        [SerializeField] private LocalizedString _privacyPolicyTitle;
        [SerializeField] private LocalizedString _openSourceTitle;
        [SerializeField] private LocalizedString _becomeTesterTitle;
        [SerializeField] private LocalizedString _epidemicSoundTitle;
        [SerializeField] private LocalizedString _epidemicSoundDesc;
        [Space]
        [SerializeField] private LocalizedString _appHeader;
        [SerializeField] private LocalizedString _clearCacheTitle;
        [SerializeField] private LocalizedString _clearCacheDesc;
        [SerializeField] private LocalizedString _hapticsEnabledTitle;
        [SerializeField] private LocalizedString _advancedTitle;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public LocalizedString PageHeader => _pageHeader;

        public LocalizedString ProfileHeader => _profileHeader;
        public LocalizedString EditProfileTitle => _editProfileTitle;

        public LocalizedString AccountHeader => _accountHeader;
        public LocalizedString ManageAccountTitle => _manageAccountTitle;
        public LocalizedString PrivacyTitle => _privacyTitle;
        public LocalizedString CommunitySupportTitle => _communitySupportTitle;
        public LocalizedString FaqTitle => _faqTitle;
        public LocalizedString TermsOfUseTitle => _termsOfUseTitle;
        public LocalizedString PrivacyPolicyTitle => _privacyPolicyTitle;
        public LocalizedString OpenSourceTitle => _openSourceTitle;
        public LocalizedString BecomeTesterTitle => _becomeTesterTitle;
        public LocalizedString EpidemicSoundTitle => _epidemicSoundTitle;
        public LocalizedString EpidemicSoundDesc => _epidemicSoundDesc;

        public LocalizedString AppHeader => _appHeader;
        public LocalizedString ClearCacheTitle => _clearCacheTitle;
        public LocalizedString ClearCacheDesc => _clearCacheDesc;
        public LocalizedString HapticsEnabled => _hapticsEnabledTitle;
        public LocalizedString AdvancedTitle => _advancedTitle;

    }
}