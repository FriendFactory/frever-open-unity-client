using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using UIManaging.PopupSystem;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class LevelEditorToVideoMessageScenarioTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;
        private readonly ILevelForVideoMessageProvider _levelForVideoMessageProvider;
        private readonly PopupManagerHelper _popupManagerHelper;
        
        public override ScenarioState To => ScenarioState.ExitToVideoMessageCreation;

        public LevelEditorToVideoMessageScenarioTransition(ILevelManager levelManager, ILevelForVideoMessageProvider levelForVideoMessageProvider, PopupManagerHelper popupManagerHelper)
        {
            _levelManager = levelManager;
            _levelForVideoMessageProvider = levelForVideoMessageProvider;
            _popupManagerHelper = popupManagerHelper;
        }

        protected override Task UpdateContext()
        {
            return Task.CompletedTask;
        }

        protected override async Task OnRunning()
        {
            _popupManagerHelper.ShowLoadingOverlay(Constants.LoadingPopupMessages.VIDEO_MESSAGE_HEADER);
            var videoMessageLevel = await _levelForVideoMessageProvider.GetLevelForVideoMessage(Context.CharacterSelection.Race);
            _levelManager.UnloadNotUsedByLevelAssets(videoMessageLevel);
            await base.OnRunning();
        }
    }
}