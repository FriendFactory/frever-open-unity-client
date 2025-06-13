using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/AvatarPageLocalization", fileName = "AvatarPageLocalization")]
    public class AvatarPageLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _avatarPageHeader;
        
        [SerializeField] private LocalizedString _deleteCharacterPopupTitle;
        [SerializeField] private LocalizedString _deleteCharacterPopupDesc;
        [SerializeField] private LocalizedString _deleteCharacterPopupConfirmButton;
        [SerializeField] private LocalizedString _deleteCharacterPopupCancelButton;
 
        [SerializeField] private LocalizedString _deletingCharacterLoadingTitle;
        
        [SerializeField] private LocalizedString _characterDeletedSnackbarMsg;
        [SerializeField] private LocalizedString _mainCharacterUpdatedSnackbarMsg;
        [SerializeField] private LocalizedString _maxCharacterCountReachedSnackbarMsg;
        
        [SerializeField] private LocalizedString _unableToDeleteMainCharacterPopupTitle;
        [SerializeField] private LocalizedString _unableToDeleteMainCharacterPopupDesc;
        
        public string AvatarPageHeader => _avatarPageHeader;
        
        public string DeleteCharacterPopupTitle => _deleteCharacterPopupTitle;
        public string DeleteCharacterPopupDesc => _deleteCharacterPopupDesc;
        public string DeleteCharacterPopupConfirmButton => _deleteCharacterPopupConfirmButton;
        public string DeleteCharacterPopupCancelButton => _deleteCharacterPopupCancelButton;
        
        public string DeletingCharacterLoadingTitle => _deletingCharacterLoadingTitle;
        public string CharacterDeletedSnackbarMsg => _characterDeletedSnackbarMsg;
        public string MainCharacterUpdatedSnackbarMsg => _mainCharacterUpdatedSnackbarMsg;
        public string MaxCharacterCountReachedSnackbarMsg => _maxCharacterCountReachedSnackbarMsg;
        public string UnableToDeleteMainCharacterPopupTitle => _unableToDeleteMainCharacterPopupTitle;
        public string UnableToDeleteMainCharacterPopupDesc => _unableToDeleteMainCharacterPopupDesc;
    }
}
