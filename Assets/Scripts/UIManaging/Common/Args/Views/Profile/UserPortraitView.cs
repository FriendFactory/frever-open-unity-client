using System;
using System.Threading;
using Abstract;
using Extensions;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Color = UnityEngine.Color;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Common.Args.Views.Profile
{
    public class UserPortraitView : BaseContextDataView<UserPortraitModel>
    {
        [SerializeField] private RawImage _portraitImage;
        [SerializeField] private Image _maskImage;
        [SerializeField] private Texture2D _blockedTexture;
        [SerializeField] private Texture2D _missingTexture;
        
        [Inject] private CharacterThumbnailProvider _characterThumbnailProvider;
        private CancellationTokenSource _cancellationSource;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnDestroy()
        {
            base.OnDestroy();

            CleanUp();
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetMaskImage(Sprite image)
        {
            _maskImage.sprite = image;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            HidePortraitImage();
            RefreshThumbnail();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            CancelTextureLoading();

            if (_portraitImage.texture == null || ContextData?.UserState != UserPortraitModel.State.Available) return;

            _characterThumbnailProvider.ReleaseIfNotUsed(ContextData.UserMainCharacterId);
            _portraitImage.texture = null;
        }

        protected virtual async void DownloadThumbnail(CharacterInfo characterInfo, Resolution resolution, Action<Texture2D> onSuccess = null, Action onFailure = null)
        {
            try
            {
                var thumbnail = await _characterThumbnailProvider.GetCharacterThumbnail(characterInfo, resolution);
                
                if (thumbnail)
                {
                    onSuccess?.Invoke(thumbnail);
                }
                else
                {
                    onFailure?.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        protected void CancelTextureLoading()
        {
            _cancellationSource?.Cancel();
            _cancellationSource = null;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void RefreshThumbnail()
        {
            if (IsDestroyed) return;

            CancelTextureLoading();

            switch (ContextData.UserState)
            {
                case UserPortraitModel.State.Blocked:
                    SetTexture(_blockedTexture);
                    return;
                case UserPortraitModel.State.Missing:
                    SetTexture(_missingTexture);
                    return;
                default:
                    _cancellationSource = new CancellationTokenSource();
                    DownloadThumbnailByCharacterData(Resolution._128x128);
                    break;
            }
        }

        private void DownloadThumbnailByCharacterData(Resolution resolution)
        {
            if (ContextData.MainCharacterThumbnail != null)
            {            
                var characterInfo = new CharacterInfo
                {
                    Id = ContextData.UserMainCharacterId,
                    Files = ContextData.MainCharacterThumbnail
                };
                
                DownloadThumbnail(characterInfo, resolution, OnThumbnailTextureDownloaded, OnThumbnailTextureFailedToDownload);
            }
            else
            {
                DownloadThumbnailByUserGroupId(resolution);
            }
        }

        private void DownloadThumbnailByUserGroupId(Resolution resolution)
        {
            _characterThumbnailProvider.GetThumbnailByUserGroupId(ContextData.UserGroupId, resolution, OnThumbnailTextureDownloaded, HidePortraitImage);
        }

        private void SetTexture(Texture texture2D)
        {
            _portraitImage.SetActive(true);
            _portraitImage.color = Color.white;
            _portraitImage.texture = texture2D;
        }
        
        private void OnThumbnailTextureDownloaded(Texture2D thumbnail)
        {
            if (IsDestroyed)
            {
                _characterThumbnailProvider.ReleaseIfNotUsed(thumbnail);
                return;
            }

            SetTexture(thumbnail);
        }

        private void OnThumbnailTextureFailedToDownload()
        {
            HidePortraitImage();
        }

        private void HidePortraitImage()
        {
            if(IsDestroyed) return;
            _portraitImage.color = Color.white.SetAlpha(0f);
        }
    }
}