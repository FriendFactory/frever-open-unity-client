using System;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.VideoDetails
{
    internal sealed class NonLevelVideoPostPreviewPanelModel: BasePostVideoPreviewPanelModel
    {
        public Texture2D VideoThumbnail { get; }
        
        public NonLevelVideoPostPreviewPanelModel(Action onPreviewRequested, Texture2D videoThumbnail) : base(onPreviewRequested)
        {
            VideoThumbnail = videoThumbnail;
        }
    }
}