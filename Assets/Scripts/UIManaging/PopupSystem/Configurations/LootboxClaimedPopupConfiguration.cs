using Bridge.Models.ClientServer.AssetStore;
using Bridge.Models.ClientServer.UserActivity;
using UnityEngine;

namespace UIManaging.PopupSystem.Configurations
{
    public class LootboxClaimedPopupConfiguration : PopupConfiguration
    {
        public LootBoxAsset[] PossibleRewards;
        public AssetInfo Reward;
        public string LootboxTitle;
        public Sprite Thumbnail;

        public LootboxClaimedPopupConfiguration()
        {
            PopupType = PopupType.LootBoxClaimedPopup;
        }
    }
}