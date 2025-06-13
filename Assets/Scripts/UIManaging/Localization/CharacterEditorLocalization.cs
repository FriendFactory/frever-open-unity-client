using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/CharacterEditorLocalization", fileName = "CharacterEditorLocalization")]
    public class CharacterEditorLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _favouriteOutfitsTab;
        [SerializeField] private LocalizedString _recentOutfitsTab;
        [SerializeField] private LocalizedString _welcomeGiftOverlayDescriptionFormat;
        
        public LocalizedString FavouriteOutfitsTab => _favouriteOutfitsTab;
        public LocalizedString RecentOutfitsTab => _recentOutfitsTab;
        public string WelcomeGiftOverlayDescriptionFormat => _welcomeGiftOverlayDescriptionFormat;
    }
}