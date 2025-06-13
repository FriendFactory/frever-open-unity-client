using System;
using System.Threading;
using Abstract;
using Extensions;
using TMPro;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class TransferOwnershipListItemModel
    {
        public readonly long GroupId;
        public readonly string Username;
        public readonly string RoleName;
        public readonly bool IsOnline;
        public ToggleGroup ToggleGroup;

        
        public TransferOwnershipListItemModel(long groupId, string username, string roleName, bool isOnline)
        {
            GroupId = groupId;
            Username = username;
            RoleName = roleName;
            IsOnline = isOnline;
        }
    }

    internal sealed class TransferOwnershipListItem : BaseContextDataView<TransferOwnershipListItemModel>
    {
        [SerializeField] private RawImage _userThumbnail;
        [SerializeField] private GameObject _statusBall;
        [SerializeField] private TMP_Text _username;
        [SerializeField] private TMP_Text _role;
        [SerializeField] private Toggle _toggle;

        private CancellationTokenSource _tokenSource;
        
        [Inject] private CharacterThumbnailsDownloader _thumbnailsDownloader;
        
        public Action<long> OnToggleValueChanged;

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(OnToggleValueChange);
            _tokenSource = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _tokenSource.CancelAndDispose();
            OnToggleValueChanged = null;
        }

        protected override void OnInitialized()
        {
            
            _thumbnailsDownloader.GetCharacterThumbnailByUserGroupId(ContextData.GroupId, Resolution._128x128,
                                                                     OnThumbnailDownloadCompleted,
                                                                     cancellationToken: _tokenSource.Token);
            _statusBall.SetActive(ContextData.IsOnline);
            _username.text = ContextData.Username;
            _role.text = ContextData.RoleName;
            _toggle.group = ContextData.ToggleGroup;
        }

        private void OnThumbnailDownloadCompleted(Texture2D thumbnail)
        {
            _userThumbnail.texture = thumbnail;
        }

        private void OnToggleValueChange(bool value)
        {
            _toggle.interactable = !value;
            OnToggleValueChanged?.Invoke(value ? ContextData.GroupId : -1);
        }
        
    }
}