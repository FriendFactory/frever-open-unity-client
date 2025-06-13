using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.ClientServer;
using Navigation.Args;
using Navigation.Core;
using UnityEngine;
using UnityEngine.UI;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Crews
{
    internal abstract class PortraitMessageView : MessageItemView<MessageItemModel>
    {
        [Header("Avatar")]
        [SerializeField] private RectTransform _thumbnailHolder;
        [SerializeField] private RawImage _userThumbnail;
        [SerializeField] private Button _thumbnailButton;
        [SerializeField] private Texture2D _defaultUserIcon;

        private Dictionary<long, Texture2D> _userThumbnailsCache;
        private GroupShortInfo _groupInfo;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected RectTransform ThumbnailHolder => _thumbnailHolder;
        protected Texture2D DefaultUserIcon => _defaultUserIcon;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetUserThumbnailsCache(Dictionary<long, Texture2D> userThumbnailsCache)
        {
            _userThumbnailsCache = userThumbnailsCache;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _userThumbnail.texture = _defaultUserIcon;

            _groupInfo = ContextData.ChatMessage.Group;
            if (_groupInfo == null) return;

            DownloadUserProfileThumbnail();

            _thumbnailButton.onClick.AddListener(PrefetchUser);
        }

        protected override void BeforeCleanup()
        {
            _userThumbnail.texture = _defaultUserIcon;
            _thumbnailButton.onClick.RemoveListener(PrefetchUser);

            base.BeforeCleanup();
        }

        protected async Task<Texture2D> DownloadUserProfileThumbnail(GroupShortInfo groupInfo, CancellationToken cancellationToken)
        {
            if (groupInfo == null)
            {
                return Texture2D.grayTexture;
            }
            
            if (_userThumbnailsCache.TryGetValue(groupInfo.Id, out var cachedTexture))
            {
                return cachedTexture;
            }

            var texture = await DownloadCharacterThumbnail(groupInfo, Resolution._128x128, cancellationToken);

            if (texture == null) return _defaultUserIcon;

            if (!_userThumbnailsCache.ContainsKey(groupInfo.Id))
            {
                _userThumbnailsCache.Add(groupInfo.Id, texture);
            }

            return texture;

        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async Task<Texture2D> DownloadCharacterThumbnail(GroupShortInfo groupInfo, Resolution resolution, CancellationToken cancellationToken)
        {
            var characterInfo = new CharacterInfo
            {
                Id = groupInfo.MainCharacterId ?? 0,
                Files = groupInfo.MainCharacterFiles
            };

            var result = await Bridge.GetThumbnailAsync(characterInfo, resolution, true, cancellationToken);

            if (result.IsSuccess)
            {
                var thumbnail = (Texture2D) result.Object;
                return thumbnail;
            }

            if (result.IsError)
            {
                Debug.LogWarning($"Failed to download thumbnail of {nameof(CharacterInfo)} " +
                                 $"with {nameof(CharacterInfo.Id)}={characterInfo.Id} with resolution {resolution}.\n" +
                                 $"Reason: {result.ErrorMessage}");
            }

            return null;
        }

        private async void DownloadUserProfileThumbnail()
        {
            var texture = await DownloadUserProfileThumbnail(_groupInfo, CancellationSource.Token);
            _userThumbnail.texture = texture;
        }

        protected void PrefetchUser()
        {
            PrefetchDataForUser(ContextData.GroupId);
        }

        protected void PrefetchDataForUser(long groupId)
        {
            ContextData.OnMovingToProfileStart();
            GoToProfile(groupId);
        }

        private void GoToProfile(long groupId)
        {
            var transitionArgs = new PageTransitionArgs
            {
                TransitionFinishedCallback = ContextData.OnMovingToProfileFinished,
                SaveCurrentPageToHistory = true
            };

            PageManager.MoveNext
            (
                PageId.UserProfile,
                new UserProfileArgs(groupId, null),
                transitionArgs
            );
        }
    }
}