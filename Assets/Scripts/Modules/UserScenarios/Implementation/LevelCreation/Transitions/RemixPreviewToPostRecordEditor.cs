using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class RemixPreviewToPostRecordEditor: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;
        private readonly PageManager _pageManager;
        
        public override ScenarioState To => ScenarioState.PostRecordEditor;

        public RemixPreviewToPostRecordEditor(ILevelManager levelManager, PageManager pageManager)
        {
            _levelManager = levelManager;
            _pageManager = pageManager;
        }

        protected override async Task UpdateContext()
        {
            Context.PostRecordEditor.IsPreviewMode = false;
            Context.PostRecordEditor.DecompressBundlesAfterPreview = false;
            Context.OriginalLevelData = await Context.LevelData.CloneAsync();
            Context.LevelToStartOver = Context.OriginalLevelData;
        }

        protected override Task OnRunning()
        {
            var firstEvent = Context.LevelData.GetFirstEvent();
            _levelManager.UnloadNotUsedByEventsAssets(firstEvent);
            _pageManager.CurrentPage.Hide();
            return base.OnRunning();
        }
    }
}