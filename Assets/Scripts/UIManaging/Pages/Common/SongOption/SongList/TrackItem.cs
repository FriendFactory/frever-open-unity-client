using System.Threading;
using Bridge.Models.Common;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.SongOption.SongList
{
    public sealed class TrackItem : PlayableItemBase<PlayableTrackModel>
    {
        [SerializeField] private Image _explicitContentIcon;
        
        protected override IPlayableMusic Music => ContextData.TrackInfo;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            _explicitContentIcon.SetActive(ContextData.TrackInfo.ExplicitContent);
        }

        protected override void DownloadThumbnail()
        {
            CancellationTokenSource = new CancellationTokenSource();
            MusicDownloadHelper.DownloadThumbnail(ContextData.TrackInfo, OnThumbnailLoaded, cancellationToken: CancellationTokenSource.Token);
        }
    }
}
