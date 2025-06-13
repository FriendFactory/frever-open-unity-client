using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/UmaEditorCategoriesLocalization", fileName = "UmaEditorCategoriesLocalization")]
    public class UmaEditorCategoriesLocalization : LocalizationMapping
    {
        [SerializeField] private LocalizedString _favouriteOutfitsTab;
        [SerializeField] private LocalizedString _recentOutfitsTab;
        
        public string FavouriteOutfitsTab => _favouriteOutfitsTab;
        public string RecentOutfitsTab => _recentOutfitsTab;
    }
}