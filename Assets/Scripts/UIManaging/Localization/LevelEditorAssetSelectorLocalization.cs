using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    public class LevelEditorAssetSelectorLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _assetSearchEmptyPlaceholder;
        [SerializeField] private LocalizedString _friendCharacterSearchEmptyPlaceholder;
        [SerializeField] private LocalizedString _outfitSearchEmptyPlaceholder;
        
        public string AssetSearchEmptyPlaceholder => _assetSearchEmptyPlaceholder;
        public string FriendCharacterSearchEmptyPlaceholder => _friendCharacterSearchEmptyPlaceholder;
        public string OutfitSearchEmptyPlaceholder => _outfitSearchEmptyPlaceholder;
    }
}