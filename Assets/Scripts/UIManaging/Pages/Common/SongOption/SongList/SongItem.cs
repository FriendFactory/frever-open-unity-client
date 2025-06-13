using System.Threading;
using Bridge.Models.Common;
using Bridge.Models.Common.Files;

namespace UIManaging.Pages.Common.SongOption.SongList
{
    public class SongItem : PlayableItemBase<PlayableSongModel>
    {

        protected override IPlayableMusic Music => ContextData.SongInfo;

        protected override void DownloadThumbnail()
        {
            CancellationTokenSource = new CancellationTokenSource();
            MusicDownloadHelper.DownloadThumbnail(ContextData.SongInfo, Resolution._128x128, OnThumbnailLoaded, OnThumbnailLoadingFailed, CancellationTokenSource.Token);
        }
    }
}