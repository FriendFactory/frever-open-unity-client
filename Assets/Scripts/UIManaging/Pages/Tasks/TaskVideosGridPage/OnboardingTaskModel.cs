using System;
using Bridge;
using Bridge.Models.ClientServer.Tasks;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;

namespace UIManaging.Pages.Tasks.TaskVideosGridPage
{
    public sealed class OnboardingTaskModel : TaskModel
    {
        public event Action VideoClicked;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public OnboardingTaskModel(VideoManager videoManager, PageManager pageManager, IBridge bridge, TaskFullInfo taskFullInfo) 
            : base(videoManager, pageManager, bridge, taskFullInfo)
        {
            
        }

        public OnboardingTaskModel(VideoManager videoManager, PageManager pageManager, IBridge bridge, TaskInfo taskInfo) 
            : base(videoManager, pageManager, bridge, taskInfo)
        {
            
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            VideoClicked?.Invoke();
        }

        public override void OnTaskClicked()
        {
            
        }
    }
}