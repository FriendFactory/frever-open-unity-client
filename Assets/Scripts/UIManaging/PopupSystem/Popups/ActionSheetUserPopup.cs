using System;
using System.Collections.Generic;
using System.Threading;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common.Files;
using Bridge.Services.UserProfile;
using TMPro;
using UIManaging.Pages.Common.Helpers;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class ActionSheetUserPopup: ActionSheetPopup<ActionSheetUserConfiguration>
    {
        [SerializeField] private RawImage _userThumbnail;
        [SerializeField] private TextMeshProUGUI _userNameText;
        
        [Inject] private CharacterThumbnailsDownloader _characterThumbnailsDownloader;

        private CancellationTokenSource _cancellationTokenSource;
        private ActionSheetUserConfiguration _configuration;

        protected override void OnConfigure(ActionSheetUserConfiguration configuration)
        {
            base.OnConfigure(configuration);
            _configuration = configuration;
            DownloadPortraitTexture();
            _userNameText.text = configuration.Profile.NickName;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void DownloadPortraitTexture()
        {
            var resolution = Resolution._128x128;
            var character = _configuration.Profile.MainCharacter;
            DownloadThumbnailByCharacterData(character.Id, character.Files, resolution);
        }

        private void DownloadThumbnailByCharacterData(long characterId, List<FileInfo> fileInfo, Resolution resolution)
        {
            var characterModel = new CharacterInfo
            {
                Id = characterId,
                Files = fileInfo
            };

            _cancellationTokenSource = new CancellationTokenSource();
            _characterThumbnailsDownloader.GetCharacterThumbnail(characterModel, resolution, OnThumbnailDownloaded, null, _cancellationTokenSource.Token);
        }

        private void OnThumbnailDownloaded(Texture2D texture)
        {
            _userThumbnail.texture = texture;
            _userThumbnail.enabled = true;
        }

        private void OnDisable()
        {
            _userThumbnail.enabled = false;
            CancelThumbnailDownloading();
        }

        private void CancelThumbnailDownloading()
        {
            if (_cancellationTokenSource == null) return;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = null;
        }
    }

    public sealed class ActionSheetUserConfiguration : ActionSheetPopupConfiguration
    {
        public Profile Profile;
    }
}