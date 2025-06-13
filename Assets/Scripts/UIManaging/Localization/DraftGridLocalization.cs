using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/DraftGridLocalization", fileName = "DraftGridLocalization")]
    public class DraftGridLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _selectButton;
        [SerializeField] private LocalizedString _cancelButton;
        
        [SerializeField] private LocalizedString _deleteVideosButtonFormat;
        [SerializeField] private LocalizedString _deleteVideosPluralButtonFormat;
        [SerializeField] private LocalizedString _deleteVideosPopupTitle;
        [SerializeField] private LocalizedString _deleteVideosPopupTitlePluralFormat;
        [SerializeField] private LocalizedString _deleteVideosPopupDesc;
        [SerializeField] private LocalizedString _deleteVideosConfirmButton;
        [SerializeField] private LocalizedString _deleteVideosCancelButton;
        
        public string SelectButton => _selectButton;
        public string CancelButton => _cancelButton;
        
        public string DeleteVideosButtonFormat => _deleteVideosButtonFormat;
        public string DeleteVideosPluralButtonFormat => _deleteVideosPluralButtonFormat;
        public string DeleteVideosPopupTitle => _deleteVideosPopupTitle;
        public string DeleteVideosPopupTitlePluralFormat => _deleteVideosPopupTitlePluralFormat;
        public string DeleteVideosPopupDesc => _deleteVideosPopupDesc;
        public string DeleteVideosConfirmButton => _deleteVideosConfirmButton;
        public string DeleteVideosCancelButton => _deleteVideosCancelButton;
    }
}