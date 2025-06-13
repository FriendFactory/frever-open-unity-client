using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/NativeGalleryPermissionPopupLocalization", fileName = "NativeGalleryPermissionPopupLocalization")]
    public class NativeGalleryPermissionPopupLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _readAccessTitle;
        [SerializeField] private LocalizedString _readAccessDescription;
        [SerializeField] private LocalizedString _writeAccessTitle;
        [SerializeField] private LocalizedString _writeAccessDescription;
        
        [SerializeField] private LocalizedString _allowButtonText;
        [SerializeField] private LocalizedString _settingsButtonText;
        
        public string ReadAccessTitle => _readAccessTitle;
        public string ReadAccessDescription => _readAccessDescription;
        public string WriteAccessTitle => _writeAccessTitle; 
        public string WriteAccessDescription => _writeAccessDescription;
        public string AllowButtonText => _allowButtonText; 
        public string SettingsButtonText => _settingsButtonText;
    }
}