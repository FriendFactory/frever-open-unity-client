using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.LevelCreation.States
{
    [UsedImplicitly]
    internal sealed class VideoMessageEditorState: StateBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        
        public override ScenarioState Type => ScenarioState.VideoMessageEditor;
        public override ITransition[] Transitions => new[] { MoveBack, MoveNext, ExitToLevelCreation, NonLevelVideoUploading }.RemoveNulls();
        
        public ITransition MoveBack;
        public ITransition MoveNext;
        public ITransition ExitToLevelCreation;
        public ITransition NonLevelVideoUploading;

        public VideoMessageEditorState(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public override void Run()
        {
            var videoMessageArgs = new VideoMessagePageArgs
            {
                Level = Context.LevelData,
                OnMoveNext = OnMoveNextRequested,
                OnMoveBackRequested = OnMoveBackRequested, 
                OnLevelCreationRequested = OnLevelCreationRequested,
                OnNonLevelVideoUploadRequested = OnNonLevelVideoPublishingRequested
            };
            _pageManager.MoveNext(videoMessageArgs);
        }

        private async void OnMoveBackRequested()
        {
            await MoveBack.Run();
        }
        
        private async void OnMoveNextRequested(Level level)
        {
            Context.LevelData = level;
            await MoveNext.Run();
        }

        private async void OnLevelCreationRequested()
        {
            await ExitToLevelCreation.Run();
        }

        private async void OnNonLevelVideoPublishingRequested(NonLeveVideoData nonLeveVideoData)
        {
            Context.PublishContext.NonLevelVideoData = nonLeveVideoData;
            await NonLevelVideoUploading.Run();
        }
    }
}