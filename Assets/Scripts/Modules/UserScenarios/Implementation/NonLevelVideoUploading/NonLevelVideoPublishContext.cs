using System.Collections.Generic;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.VideoServer;
using Common.Publishers;
using Models;
using Modules.UserScenarios.Implementation.Common;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.NonLevelVideoUploading
{
    internal sealed class NonLevelVideoPublishContext: IExitContext
    {
        public NonLeveVideoData NonLeveVideoData { get; set; }
        public ChatInfo OpenChatOnComplete { get; set; }
        public PageId OpenedFromPage { get; set; }
        public long? UploadedVideoId;
        public VideoAccess Access;
        public List<long> SelectedUsers;
        public string DescriptionText;
        public ExternalLinkType ExternalLinkType;
        public string ExternalLink;
        public PublishingType PublishingType;
        public MessagePublishInfo MessagePublishInfo;
    }
}