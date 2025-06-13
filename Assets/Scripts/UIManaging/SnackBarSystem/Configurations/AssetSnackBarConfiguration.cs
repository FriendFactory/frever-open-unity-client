using UnityEngine;

namespace UIManaging.SnackBarSystem.Configurations
{
    public class AssetSnackBarConfiguration: SnackBarConfiguration
    {
        internal override SnackBarType Type => SnackBarType.AssetClaimed;

        public readonly string Description;
        public readonly Sprite Thumbnail;

        public AssetSnackBarConfiguration(string title, string description, Sprite thumbnail)
        {
            Title = title;
            Description = description;
            Thumbnail = thumbnail;
        }
    }
}