using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    public class EditTemplateLocalization : MonoBehaviour
    {
        [SerializeField] private LocalizedString _templateUnlockRequirementDescription;
        [SerializeField] private LocalizedString _templateUnlockCreatorRankDescription;
        [SerializeField] private LocalizedString _templateUnlockRequirementHint;
        
        [SerializeField] private LocalizedString _templateVideoPrivacyPopupTitle;
        [SerializeField] private LocalizedString _templateVideoPrivacyPopupText;
        [SerializeField] private LocalizedString _templateVideoPrivacyPopupYesText;
        [SerializeField] private LocalizedString _templateVideoPrivacyPopupNoText;
        
        [SerializeField] private LocalizedString _videoPrivacyUpdatedSnackbarMessage;
        [SerializeField] private LocalizedString _templateNameMinLengthMessage;
        [SerializeField] private LocalizedString _templateNameMaxLengthMessage;
        [SerializeField] private LocalizedString _templateNameErrorMessage;
        
        [SerializeField] private LocalizedString _templateNamePlaceholder;

        public string TemplateUnlockScoreRequirementDescription => _templateUnlockRequirementDescription;
        public string TemplateUnlockCreatorRankDescription => _templateUnlockCreatorRankDescription;
        public string TemplateUnlockScoreRequirementHint => _templateUnlockRequirementHint;

        public string TemplateVideoPrivacyPopupTitle => _templateVideoPrivacyPopupTitle;
        public string TemplateVideoPrivacyPopupText => _templateVideoPrivacyPopupText;
        public string TemplateVideoPrivacyPopupYesText => _templateVideoPrivacyPopupYesText;
        public string TemplateVideoPrivacyPopupNoText => _templateVideoPrivacyPopupNoText;
        
        public string VideoPrivacyUpdatedSnackbarMessage => _videoPrivacyUpdatedSnackbarMessage;
        public string TemplateNameMinLengthMessage => _templateNameMinLengthMessage;
        public string TemplateNameMaxLengthMessage => _templateNameMaxLengthMessage;
        public string TemplateNameErrorMessage => _templateNameErrorMessage;
        
        public string TemplateNamePlaceholder => _templateNamePlaceholder;
    }
}
