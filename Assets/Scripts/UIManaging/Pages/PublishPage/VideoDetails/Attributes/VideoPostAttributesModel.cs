using System;
using System.Linq;
using Bridge;
using Bridge.Models.VideoServer;
using JetBrains.Annotations;
using Laphed.Rx;
using Models;
using Modules.AssetsStoraging.Core;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.VideoDetails.Attributes
{
    [UsedImplicitly]
    internal sealed class VideoPostAttributesModel
    {
        private readonly IBridge _bridge;
        private readonly LocalUserDataHolder _localUserDataHolder;
        private readonly IDataFetcher _dataFetcher;

        public VideoPostAttributesModel(IBridge bridge, LocalUserDataHolder localUserDataHolder, IDataFetcher dataFetcher)
        {
            _bridge = bridge;
            _localUserDataHolder = localUserDataHolder;
            _dataFetcher = dataFetcher;
        }

        public ReactiveProperty<VideoAccess> VideoAccess { get; } = new();
        public ReactiveProperty<bool> UploadVideo { get; } = new();
        public ReactiveProperty<string> TemplateName { get; } = new();
        public ReactiveProperty<int> TaggedUsersCount { get; } = new();
        public ReactiveProperty<string> OriginalCreator { get; } = new();

        public void RefreshTaggedGroupsInfo(Level level)
        {
            var uniqueGroupIds = level.Event
                                    .SelectMany(ev => ev.CharacterController
                                                        .Where(cc => cc.Character.GroupId != _localUserDataHolder.GroupId)
                                                        .Select(cc => cc.Character.GroupId))
                                    .Distinct().ToArray();

            TaggedUsersCount.Value = uniqueGroupIds.Length;
        }
    }
}