using System;
using UnityEngine;

namespace UIManaging.SnackBarSystem.Configurations
{
    public sealed class VideoPublishedSnackBarConfiguration : SnackBarConfiguration
    {
        internal override SnackBarType Type => SnackBarType.VideoPublished;
    
        public Texture2D Thumbnail { get; set; }
        public string Description { get; set; }
        public string SharingUrl { get; set; }
        public bool IsNonLevelVideo { get; set; }
        public Action OnClick { get; set; }
        public Action OnShareClick { get; set; }
    }
}
