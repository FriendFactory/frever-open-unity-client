using System;
using Bridge.Models.ClientServer;
using JetBrains.Annotations;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.PopupSystem.Popups.ChatSettings
{
    [UsedImplicitly]
    public class ChatMemberModel
    {
        [Inject] private CharacterThumbnailsDownloader _thumbnailsDownloader;

        public GroupShortInfo UserInfo { get; }
        public Action<bool> HideAction { get; }

        public ChatMemberModel(GroupShortInfo userInfo, Action<bool> hideAction)
        {
            UserInfo = userInfo;
            HideAction = hideAction;
        }

        public void DownloadThumbnail(Action<Texture2D> actionResult)
        {
            _thumbnailsDownloader.GetCharacterThumbnailByUserGroupId(UserInfo.Id, Resolution._128x128, actionResult);
        }
        
        [UsedImplicitly]
        public class Factory: PlaceholderFactory<GroupShortInfo, Action<bool>, ChatMemberModel> { }

    }
}