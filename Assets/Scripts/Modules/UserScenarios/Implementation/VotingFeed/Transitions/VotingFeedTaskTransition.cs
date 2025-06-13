using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Common.ModelsMapping;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Templates;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using UIManaging.SnackBarSystem;

namespace Modules.UserScenarios.Implementation.VotingFeed.Transitions
{
    [UsedImplicitly]
    internal sealed class VotingFeedTaskTransition: TaskStartTransitionBase<VotingFeedContext>
    {
        public override ScenarioState To => ScenarioState.VotingFeedCharacterEditor;

        public VotingFeedTaskTransition(IBridge bridge, IMapper mapper, SnackBarHelper snackBarHelper, ILevelHelper levelHelper, 
            ITemplateProvider templateProvider, CharacterManager characterManager, IMetadataProvider metadataProvider): 
            base(bridge, mapper, snackBarHelper, levelHelper, templateProvider, characterManager, metadataProvider) { }

        protected override async Task UpdateContext()
        {
            var taskInfo = Context.Task;

            Context.CharacterEditor.OpenedFrom = ScenarioState.PostRecordEditor;
            Context.LevelEditor.ShowLoadingPagePopup = true;
            Context.PostRecordEditor.CheckIfUserMadeEnoughChangesForTask = false;
            
            await InitializeCharacter(taskInfo.IsDressed);
            await InitializeTask(taskInfo);
            
            InitializeShowingTaskInfo(taskInfo.Pages.First());
        }
    }
}