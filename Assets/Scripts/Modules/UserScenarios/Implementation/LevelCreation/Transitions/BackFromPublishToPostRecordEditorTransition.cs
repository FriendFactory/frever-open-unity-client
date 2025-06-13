using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using UIManaging.Localization;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class BackFromPublishToPostRecordEditorTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;

        [Inject] private LoadingOverlayLocalization _loadingOverlayLocalization;
        
        public BackFromPublishToPostRecordEditorTransition(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        public override ScenarioState To => ScenarioState.PostRecordEditor;
        
        protected override async Task UpdateContext()
        {
            Context.NavigationMessage = _loadingOverlayLocalization.GoingBackToClipEditorHeader;
            Context.LevelData = _levelManager.CurrentLevel;
            Context.PostRecordEditor.IsPreviewMode = false;
            Context.OriginalLevelData = await _levelManager.CurrentLevel.CloneAsync();
            Context.LevelToStartOver = await _levelManager.CurrentLevel.CloneAsync();
        }
    }
}