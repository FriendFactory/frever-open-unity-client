using System;
using Common.Collections;
using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/LootboxPopupLocalization", fileName = "LootboxPopupLocalization")]
    public class LootboxPopupLocalization : ScriptableObject
    {
        [SerializeField] private RarityMapping _rarityRanks;
        
        [SerializeField] private LocalizedString _openLootboxDescriptionFormat;
        [SerializeField] private LocalizedString _openButton;
        [SerializeField] private LocalizedString _confirmButton;
        [SerializeField] private LocalizedString _assetClaimedSnackbarTitle;
        [SerializeField] private LocalizedString _assetClaimedSnackbarMessage;

        public string GetRarityLocalized(string rarityTitle)
        {
            var translationAvailable = _rarityRanks.TryGetValue(rarityTitle, out var translation);
            return translationAvailable && !string.IsNullOrEmpty(translation)
                ? (string)translation 
                : rarityTitle;
        }
        
        public string OpenLootboxDescriptionFormat => _openLootboxDescriptionFormat;
        public string OpenButton => _openButton;
        public string ConfirmButton => _confirmButton;
        public string AssetClaimedSnackbarTitle => _assetClaimedSnackbarTitle;
        public string AssetClaimedSnackbarMessage => _assetClaimedSnackbarMessage;
        
        
        [Serializable] public class RarityMapping : SerializedDictionary<string, LocalizedString> { }
    }
}