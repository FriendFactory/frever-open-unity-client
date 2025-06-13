using System.IO;
using Common.Thumbnails;
using UnityEngine;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace Modules.LevelManaging.Editing.ThumbnailCreator
{
    internal sealed class EventTimelineThumbnailCreator : BaseThumbnailCreator
    {
        public EventTimelineThumbnailCreator(Vector2Int thumbnailsSize, Resolution thumbnailResolution) : base(thumbnailsSize, thumbnailResolution)
        {
            _saveDirectoryPath = Path.Combine(Application.persistentDataPath, "EventThumbnails");
        }

        protected override Vector2Int GetTextureSize(Vector2Int screenshotResolution)
        {
            return Vector2Int.one * screenshotResolution.x;
        }
    }
}