using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(fileName = "MusicGalleryLocalization", menuName = "L10N/MusicGalleryLocalization", order = 0)]
    public class MusicGalleryLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _moodsCategoryHeader;
        [Header("Uploads")]
        [SerializeField] private LocalizedString _uploadsCategoryHeader;
        [SerializeField] private LocalizedString _uploadsPublicSoundsTab;
        [SerializeField] private LocalizedString _uploadsMySoundsTab;
        [Header("User Sound Settings")]
        [SerializeField] private LocalizedString _userSoundSettingsHeader;
        [SerializeField] private LocalizedString _userSoundSettingsErrorMessage;
        [SerializeField] private LocalizedString _userSoundSettingsNotUniqueErrorMessage;
        [Header("Search")]
        [SerializeField] private LocalizedString _searchTrendingTab;
        [SerializeField] private LocalizedString _searchMoodTab;
        [SerializeField] private LocalizedString _searchUserSoundsTab;
        [SerializeField] private LocalizedString _searchCommercialTab;

        public string MoodsCategoryHeader => _moodsCategoryHeader;
        public string UploadsCategoryHeader => _uploadsCategoryHeader;
        public string UploadsPublicSoundsTab => _uploadsPublicSoundsTab;
        public string UploadsMySoundsTab => _uploadsMySoundsTab;
        public string UserSoundSettingsHeader => _userSoundSettingsHeader;
        public string UserSoundSettingsErrorMessage => _userSoundSettingsErrorMessage;
        public string UserSoundSettingsNotUniqueErrorMessage => _userSoundSettingsNotUniqueErrorMessage;
        public string SearchTrendingTab => _searchTrendingTab;
        public string SearchMoodTab => _searchMoodTab;
        public string SearchUserSoundsTab => _searchUserSoundsTab;
        public string SearchCommercialTab => _searchCommercialTab;
    }
}