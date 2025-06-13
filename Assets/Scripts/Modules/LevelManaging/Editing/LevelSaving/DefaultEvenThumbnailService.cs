using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bridge.Models.Common.Files;
using JetBrains.Annotations;
using UnityEngine;
using Event = Models.Event;
using Resolution = Bridge.Models.Common.Files.Resolution;
using FileInfo = Bridge.Models.Common.Files.FileInfo;

namespace Modules.LevelManaging.Editing.LevelSaving
{
    //todo: hotfix for events which has no thumbnails FREV-9574, because we couldn't fix it for a long time. We will set the default thumbnails instead
    public interface IDefaultThumbnailService
    {
        Task Init();
        void FillMissedEventThumbnailsByDefault(params Event[] events);
    }

    [UsedImplicitly]
    internal sealed class DefaultEvenThumbnailService: IDefaultThumbnailService
    {
        private string DefaultThumbnailPath => $"{Application.persistentDataPath}/DefaultEventThumbnail.png";

        public async Task Init()
        {
            if (!File.Exists(DefaultThumbnailPath))
            {
                var defaultImageBytes = Texture2D.blackTexture.EncodeToPNG();
                using (var stream = File.Create(DefaultThumbnailPath))
                {
                    await stream.WriteAsync(defaultImageBytes, 0, defaultImageBytes.Length);
                }
            }
        }

        public void FillMissedEventThumbnailsByDefault(params Event[] events)
        {
            foreach (var ev in events)
            {
                if(ev.Files != null && ev.Files.Count == 2) continue;
                var fileInfo128 = new FileInfo(DefaultThumbnailPath, FileType.Thumbnail, Resolution._128x128);
                var fileInfo512 = new FileInfo(DefaultThumbnailPath, FileType.Thumbnail, Resolution._512x512);
                ev.Files = new List<FileInfo>
                {
                    fileInfo128,
                    fileInfo512
                };
            }
        }
    }
}