using System;
using Bridge.Models.VideoServer;
using Models;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class OnboardingPublishPageArgs : PageArgs
    {
        public Level LevelData;
        public override PageId TargetPage => PageId.OnboardingPublishPage;
        public Action<VideoUploadingSettings> OnMoveNextRequested { get; private set; }
        public Action<Level> OnPreviewRequested;
        public bool ShowTaggedMembersCount { get; private set; }
        public string Description { get; private set; } = string.Empty;
        public VideoAccess DefaultVideoAccess { get; private set; }

        public static OnboardingPublishPageArgs PrivacySettingsOnly(
            Level levelData,
            string description,
            Action<VideoUploadingSettings> onMoveNextRequested,
            Action<Level> previewRequested)
        {
            return new OnboardingPublishPageArgs
            {
                LevelData = levelData,
                DefaultVideoAccess = VideoAccess.Private,
                ShowTaggedMembersCount = false,
                Description = description,
                OnMoveNextRequested = onMoveNextRequested,
                OnPreviewRequested = previewRequested,
            };
        }

        public static OnboardingPublishPageArgs WithTaggedMembers(
            Level levelData,
            string description,
            Action<VideoUploadingSettings> onMoveNextRequested,
            Action<Level> previewRequested)
        {
            return new OnboardingPublishPageArgs()
            {
                LevelData = levelData,
                ShowTaggedMembersCount = true,
                DefaultVideoAccess = VideoAccess.ForFriends,
                Description = description,
                OnMoveNextRequested = onMoveNextRequested,
                OnPreviewRequested = previewRequested,
            };
        } 
    }
}
