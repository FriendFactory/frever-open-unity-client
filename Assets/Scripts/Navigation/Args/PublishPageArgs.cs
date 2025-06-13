using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.ClientServer.Template;
using Bridge.Models.VideoServer;
using Bridge.Services.UserProfile;
using Common.Publishers;
using Models;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class PublishPageArgs : PageArgs
    {
        public Level LevelData;
        public PublishingType PublishingType;
        public override PageId TargetPage => PageId.PublishPage;

        public Action OnMoveBackRequested;
        public Action<VideoUploadingSettings> OnPublishRequested;
        public Action<Level, VideoUploadingSettings> OnPreviewRequested;
        public Action OnSaveToDraftsRequested;
        public ShareDestination ShareDestination;
        public VideoUploadingSettings VideoUploadingSettings;
        public TemplateInfo InitialTemplate;
        public GroupInfo OriginalCreator;
    }

    public struct ShareDestination
    {
        public ChatShortInfo[] Chats;
        public Profile[] Users;
    }
    
    public struct VideoUploadingSettings
    {
        public PublishingType PublishingType;
        public PublishInfo PublishInfo;
        public MessagePublishInfo MessagePublishInfo;
    }
        
    public class PublishInfo
    {
        public VideoAccess Access;
        public List<long> SelectedUsers;
        public string DescriptionText;
        public bool SaveToDevice;
        public bool IsLandscapeMode;
        public bool IsUltraHDMode;
        public bool GenerateTemplate;
        public bool ShowTemplatePopup;
        public string GenerateTemplateWithName;
        public ExternalLinkType ExternalLinkType;
        public string ExternalLink;
        public Action OnClearPublishData;
        public bool AllowRemix;
        public bool AllowComment;
    }
        
    public class MessagePublishInfo
    {
        public string MessageText;
        public ShareDestination ShareDestination;
    }
}
