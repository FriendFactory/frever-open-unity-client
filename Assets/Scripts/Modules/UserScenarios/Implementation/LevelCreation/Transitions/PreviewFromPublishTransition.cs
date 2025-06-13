using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.Common.Files;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class PreviewFromPublishTransition : TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        public override ScenarioState To => ScenarioState.PostRecordEditor;
        
        protected override Task UpdateContext()
        {
            Context.SaveEventThumbnails = Context.LevelData.Event.Any(x => !x.HasActualThumbnail);
            Context.NavigationMessage = "Loading preview...";
            Context.PostRecordEditor.IsPreviewMode = true;
            return Task.CompletedTask;
        }
    }

    [UsedImplicitly]
    internal sealed class BackToPublishAfterPreview : TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;
        private readonly IBridge _bridge;
        
        public override ScenarioState To => ScenarioState.Publish;

        public BackToPublishAfterPreview(ILevelManager levelManager, IBridge bridge)
        {
            _levelManager = levelManager;
            _bridge = bridge;
        }

        protected override Task UpdateContext()
        {
            //nothing
            return Task.CompletedTask;
        }

        protected override async Task OnRunning()
        {
            var firstEvent = _levelManager.CurrentLevel.GetFirstEvent();
            _levelManager.UnloadNotUsedByEventsAssets(firstEvent);
            if (Context.SaveEventThumbnails)
            {
                var thumbnails = Context.LevelData.GetEventsWithRefreshedThumbnails()
                                        .ToDictionary(arg => arg.Id, arg => arg.Files);
                var resp = await _bridge.UpdateEventThumbnails(thumbnails);
                if (resp.IsSuccess)
                {
                    ReplaceThumbnailsForEvents(thumbnails);
                }
                Context.SaveEventThumbnails = false;
            }
            _levelManager.DeactivateAllAssets();
            await base.OnRunning();
        }

        private void ReplaceThumbnailsForEvents(IReadOnlyDictionary<long, List<FileInfo>> thumbnails)
        {
            foreach (var pair in thumbnails)
            {
                var eventId = pair.Key;
                var ev = Context.LevelData.GetEvent(eventId);
                ev.Files = pair.Value;
                ev.HasActualThumbnail = true;
            }
        }
    }
}