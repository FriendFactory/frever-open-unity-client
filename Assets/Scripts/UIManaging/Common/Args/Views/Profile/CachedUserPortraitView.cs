using System;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Common.Args.Views.Profile
{
    public class CachedUserPortraitView : UserPortraitView
    {
        [Inject] private CharacterThumbnailsDownloader _characterThumbnailsDownloader;

        protected override void DownloadThumbnail(CharacterInfo characterInfo, Resolution resolution, Action<Texture2D> onSuccess = null, Action onFailure = null)
        {
            _characterThumbnailsDownloader.GetCachedCharacterThumbnail(characterInfo, resolution, onSuccess, str => onFailure?.Invoke());
        }

        protected override void BeforeCleanup()
        {
            CancelTextureLoading();
        }
    }
}


