using System;
using Bridge.Models.ClientServer.Template;
using Navigation.Core;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine.Events;

namespace Navigation.Args
{
    public abstract class BaseVideoTemplatePageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.VideosBasedOnTemplatePage;
        public string TemplateName;
        public abstract TemplateType TemplateType { get; }
        
        public TemplateInfo TemplateInfo;
        public int UsageCount { get; set; }

        public Action OnBackButtonRequested;
        public UnityAction OnJoinTemplateRequested;

        public abstract BaseVideoListLoader GetVideoListLoader(PageManager pageManager, VideoManager videoManager);
    }
}