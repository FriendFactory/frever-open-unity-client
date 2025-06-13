using System;
using System.Threading;
using Abstract;
using Bridge;
using Extensions;
using TMPro;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class SidebarRequestListItemModel
    {
        public int Index;
        public bool Blocked;
        public long UserGroupId;
        public string Username;
        public long RequestId;
        
        public SidebarRequestListItemModel(int index, long groupId, long requestId, string username, bool blocked)
        {
            Index = index;
            UserGroupId = groupId;
            Blocked = blocked;
            Username = username;
            RequestId = requestId;
        }
    }

    internal sealed class SidebarRequestListItem : BaseContextDataView<SidebarRequestListItemModel>
    {
        [SerializeField] private RawImage _portrait;
        [SerializeField] private Sprite _blockedPortrait;
        [SerializeField] private TMP_Text _username;

        [Space]
        [SerializeField] private Button _rejectButton;
        [SerializeField] private Button _acceptButton;
        
        [Inject] private IBridge _bridge;
        [Inject] private CharacterThumbnailsDownloader _thumbnailsDownloader;
        
        private CancellationTokenSource _tokenSource;

        public Action<int, long, long> RejectActionRequested;
        public Action<int, long, long> AcceptActionRequested;

        private void OnEnable()
        {
            _acceptButton.onClick.AddListener(RequestAcceptAction);
            _rejectButton.onClick.AddListener(RequestRejectAction);
            _tokenSource = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _tokenSource?.CancelAndDispose();
            _tokenSource = null;
            
            _acceptButton.onClick.RemoveAllListeners();
            _rejectButton.onClick.RemoveAllListeners();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            RejectActionRequested = null;
            AcceptActionRequested = null;
        }

        protected override void OnInitialized()
        {
            GetUserPortrait();
            if (ContextData.Blocked)
            {
                _username.text = $"[<b>BLOCKED</b>] {ContextData.Username}";
                return;
            }

            _username.text = ContextData.Username;
        }

        private void GetUserPortrait()
        {
            _thumbnailsDownloader.GetCharacterThumbnailByUserGroupId(ContextData.UserGroupId, Resolution._128x128, DownloadCompleted, DownloadFailed,cancellationToken:_tokenSource.Token);

            void DownloadCompleted(Texture2D texture2D)
            {
                if (texture2D == null)
                {
                    _portrait.texture = _blockedPortrait.texture;
                    return;
                }
                _portrait.texture = texture2D;
            }

            void DownloadFailed(string _)
            {
                _portrait.texture = _blockedPortrait.texture;
            }
        }

        private void RequestAcceptAction()
        {
            AcceptActionRequested?.Invoke(ContextData.Index, ContextData.UserGroupId, ContextData.RequestId);
        }
        
        private void RequestRejectAction()
        {
            RejectActionRequested?.Invoke(ContextData.Index, ContextData.UserGroupId, ContextData.RequestId);
        }
    }
}