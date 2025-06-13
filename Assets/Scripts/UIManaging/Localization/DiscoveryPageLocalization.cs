using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/DiscoveryPageLocalization", fileName = "DiscoveryPageLocalization")]
    public class DiscoveryPageLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _usersSearchTab;
        [SerializeField] private LocalizedString _hashtagsSearchTab;
        [SerializeField] private LocalizedString _soundsSearchTab;
        [SerializeField] private LocalizedString _crewsSearchTab;
        [Space]
        [SerializeField] private LocalizedString _defaultSearchInputPlaceholder;
        [SerializeField] private LocalizedString _usersSearchInputPlaceholder;
        [SerializeField] private LocalizedString _hashtagsSearchInputPlaceholder;
        [SerializeField] private LocalizedString _soundsSearchInputPlaceholder;
        [SerializeField] private LocalizedString _crewsSearchInputPlaceholder;
        [Space]
        [SerializeField] private LocalizedString _soundUsedCounterTextFormat;
        
        public string UsersSearchTab => _usersSearchTab;
        public string HashtagsSearchTab => _hashtagsSearchTab;
        public string SoundsSearchTab => _soundsSearchTab;
        public string CrewsSearchTab => _crewsSearchTab;
        
        public string DefaultSearchInputPlaceholder => _defaultSearchInputPlaceholder;
        public string UsersSearchInputPlaceholder => _usersSearchInputPlaceholder;
        public string HashtagsSearchInputPlaceholder => _hashtagsSearchInputPlaceholder;
        public string SoundsSearchInputPlaceholder => _soundsSearchInputPlaceholder;
        public string CrewsSearchInputPlaceholder => _crewsSearchInputPlaceholder;
        public string SoundUsedCounterTextFormat => _soundUsedCounterTextFormat;
    }
}