using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.Common.Files;
using Common.UI;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Resolution = Bridge.Models.Common.Files.Resolution;
using UserProfile = Bridge.Services.UserProfile.Profile;

namespace UIManaging.Common.Args.Views.Profile
{
    public class UserPortrait : UIElementWithPlaceholder<UserPortraitModel>
    {
        [SerializeField] private RawImage _portraitImage;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;

        [Inject] private CharacterThumbnailsDownloader _characterThumbnailsDownloader;

        private Texture _portraitTexture;
        private Color _placeholderColor;
        private bool _canReinitialize;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _placeholderColor = _portraitImage.color;
        }
        
        //---------------------------------------------------------------------
        // Public 
        //---------------------------------------------------------------------

        public override async Task InitializeAsync(UserPortraitModel model, CancellationToken token)
        {
            if (IsInitialized && !_canReinitialize) return;

            await base.InitializeAsync(model, token);
        }

        public async Task InitializeAsync(UserProfile profile, Resolution resolution, CancellationToken token, bool canReinitialize = false)
        {
            if (IsInitialized && !canReinitialize) return;

            IsInitialized = false;
            
            _canReinitialize = canReinitialize;
            var model = new UserPortraitModel
            {
                Resolution = resolution,
                UserGroupId = profile.MainGroupId,
                UserMainCharacterId = profile.MainCharacter.Id,
                MainCharacterThumbnail = profile.MainCharacter.Files
            };

            await InitializeAsync(model, token);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override InitializationResult OnInitialize(UserPortraitModel model, CancellationToken token)
        {
            _portraitImage.enabled = false;
            
            var characterModel = new CharacterInfo
            {
                Id = model.UserMainCharacterId,
                Files = model.MainCharacterThumbnail
            };

            DownloadPortraitTexture(model, token);
            if (_characterThumbnailsDownloader.HasCachedThumbnail(characterModel,
                                                                  model.MainCharacterThumbnail.First(m => m.Resolution == model.Resolution)))
            {
                return InitializationResult.Done;
            }
            
            return InitializationResult.Wait;
        }

        protected override void OnInitializationCancelled() { }

        protected override void OnShowContent()
        {
            if (_portraitTexture == null || _portraitTexture.height == 0)
            {
                return;
            }

            if (_aspectRatioFitter != null)
            {
                _aspectRatioFitter.aspectRatio = (float) _portraitTexture.width / _portraitTexture.height;
            }

            if (_portraitImage == null) return;

            _portraitImage.color = Color.white;
            _portraitImage.texture = _portraitTexture;
            _portraitImage.enabled = true;
        }

        protected override void OnCleanUp()
        {
            if (!_portraitTexture) return;
            
            _portraitImage.color = _placeholderColor;
            _portraitImage.texture = null;

            _portraitTexture = null;
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        private void OnTextureDownloaded(Texture texture2D)
        {
            _portraitTexture = texture2D;
            CompleteInitialization();
        }

        void OnTextureFailedToDownload(string reason)
        {
            CompleteInitialization();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void DownloadPortraitTexture(UserPortraitModel model, CancellationToken token)
        {
            var resolution = model.Resolution ?? Resolution._128x128;

            var characterId = model.UserMainCharacterId;
            var fileInfo = model.MainCharacterThumbnail;
            DownloadThumbnailByCharacterData(characterId, fileInfo, resolution, token);
        }

        private void DownloadThumbnailByCharacterData(long characterId, List<FileInfo> fileInfo, Resolution resolution, CancellationToken token)
        {
            var characterModel = new CharacterInfo
            {
                Id = characterId,
                Files = fileInfo
            };

             _characterThumbnailsDownloader.GetCharacterThumbnail(
                    characterModel, 
                    resolution, 
                    OnTextureDownloaded, 
                    OnTextureFailedToDownload, 
                    token);
        }
    }
}
