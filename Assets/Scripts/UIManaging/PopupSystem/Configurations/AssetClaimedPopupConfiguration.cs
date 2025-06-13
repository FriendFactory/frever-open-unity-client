using UnityEngine;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class AssetClaimedPopupConfiguration: PopupConfiguration
    {
        public Sprite Thumbnail;
        public int? Level;

        public AssetClaimedPopupConfiguration()
        {
            PopupType = PopupType.AssetClaimedPopup;
        }
    }
}